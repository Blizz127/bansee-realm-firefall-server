using System;
using System.Threading;
using Aero.Protocol;
using MatrixServer.Packets;
using Serilog;
using Shared.Udp;

namespace MatrixServer;

internal class MatrixServer : PacketServer
{
    public MatrixServer(MatrixServerSettings matrixServerSettings,
                        ILogger logger)
        : base(matrixServerSettings.Port, logger)
    {
    }

    protected override void HandlePacket(Packet packet, CancellationToken ct)
    {
        var mem = packet.PacketData;
        if (mem.Length < 8)
        {
            Logger.Warning("[MATRIX] {Remote} short packet ({Bytes} bytes)", packet.RemoteEndpoint, mem.Length);
            return;
        }

        // SocketID is the first dword; type is the next 4 bytes. Do not drop non-zero
        // SocketID packets — after HEHE the client may send KISS/ABRT with the assigned id.
        var socketId = Deserializer.ReadStruct<uint>(mem);
        var matrixPkt = Deserializer.ReadStruct<MatrixPacketBase>(mem);
        Logger.Information("[MATRIX] {Remote} sent {Bytes} bytes (socket={SocketId} type={Type})",
                           packet.RemoteEndpoint, packet.PacketData.Length, socketId, matrixPkt.Type);

        switch (matrixPkt.Type)
        {
            case "POKE":
                if (socketId != 0)
                {
                    Logger.Warning("[MATRIX] Ignoring POKE with non-zero SocketID {SocketId} from {Remote}", socketId, packet.RemoteEndpoint);
                    return;
                }

                var nextSocketId = GenerateSocketId();
                Logger.Information("Assigning SocketID [{SocketID}] to [{RemoteEndpoint}]", nextSocketId, packet.RemoteEndpoint);

                var poke = Deserializer.ReadStruct<MatrixPacketPoke>(mem);

                // Some client builds put the wire protocol in the first ushort; prefer non-zero.
                var wireProtocol = poke.ProtocolVersion != 0 ? poke.ProtocolVersion : poke.UnkVersion;
                if (!TryResolveMatrixProtocol(wireProtocol, out var matrixVersion))
                {
                    Logger.Warning("SocketID [{SocketID}] Unknown ProtocolVersion: {ProtocolVersion} (unk={UnkVersion}); defaulting to V32",
                                   nextSocketId, poke.ProtocolVersion, poke.UnkVersion);
                    matrixVersion = MatrixVersion.V32;
                }
                else
                {
                    Logger.Information("SocketID [{SocketID}] Matrix Protocol {MatrixVersion} ({ProtocolVersion})", nextSocketId, matrixVersion, wireProtocol);
                }

                _ = SendAsync(Serializer.WriteStruct(new MatrixPacketHehe(nextSocketId)), packet.RemoteEndpoint);
                break;
            case "KISS":
                var kiss = Deserializer.ReadStruct<MatrixPacketKiss>(mem);
                if (!TryResolveGssProtocol(kiss.StreamingProtocolVersion, out var gssVersion))
                {
                    Logger.Warning("SocketID [{SocketID}] Unknown StreamingProtocolVersion {StreamingProtocolVersion}; defaulting to V74",
                                   kiss.ReceivedSocketID, kiss.StreamingProtocolVersion);
                    gssVersion = GssVersion.V74;
                }
                else
                {
                    Logger.Information("SocketID [{SocketID}] GSS Protocol {GssVersion} ({StreamingProtocolVersion})",
                                       kiss.ReceivedSocketID, gssVersion, kiss.StreamingProtocolVersion);
                }

                // GameServer listens on UDP 25001 (see start.sh / GameServer.dll.config).
                _ = SendAsync(Serializer.WriteStruct(new MatrixPacketHugg(1, 25001)), packet.RemoteEndpoint);
                break;
            case "ABRT":
                var abrt = Deserializer.ReadStruct<MatrixPacketAbrt>(mem);
                Logger.Information("Received abort with reason: {AbortCode}", abrt.Code);
                break;
            default:
                Logger.Error("Unknown Matrix Packet Type: {Type} (socket={SocketId})", matrixPkt.Type, socketId);
                return;
        }
    }

    /// <summary>
    ///     Aero.Gen maps are often empty in MatrixServer builds; accept 1-based wire ids
    ///     (client 1962 sends Matrix 32 / GSS ~74) by falling back to enum ordinal = wire - 1.
    /// </summary>
    private static bool TryResolveMatrixProtocol(ushort wire, out MatrixVersion version)
    {
        if (ProtocolVersions.TryGetMatrixVersion(wire, out version))
        {
            return true;
        }

        if (wire is >= 1 and <= 32)
        {
            version = (MatrixVersion)(wire - 1);
            return Enum.IsDefined(version);
        }

        version = default;
        return false;
    }

    private static bool TryResolveGssProtocol(ushort wire, out GssVersion version)
    {
        if (ProtocolVersions.TryGetGssVersion(wire, out version))
        {
            return true;
        }

        if (wire is >= 1 and <= 80)
        {
            version = (GssVersion)(wire - 1);
            return Enum.IsDefined(version);
        }

        version = default;
        return false;
    }

    private static uint GenerateSocketId()
    {
        return unchecked((uint)((0xff00ff << 8) | new Random().Next(0, 256)));
    }
}
