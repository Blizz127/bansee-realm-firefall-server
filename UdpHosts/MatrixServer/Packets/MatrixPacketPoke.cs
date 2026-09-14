using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Shared.Udp;

namespace MatrixServer.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct MatrixPacketPoke
{
    public readonly uint SocketID;
    private fixed byte _type[4];

    public string Type
    {
        get
        {
            fixed (byte* t = _type)
            {
                return Deserializer.ReadFixedString(t, 4);
            }
        }
        set
        {
            fixed (byte* t = _type)
            {
                Serializer.WriteFixed(t, Encoding.ASCII.GetBytes(value[..4]));
            }
        }
    }

    private readonly ushort _unkVersion;
    private readonly ushort _protocolVersion;

    public readonly ushort UnkVersion =>
        BinaryPrimitives.ReverseEndianness(_unkVersion);

    public readonly ushort ProtocolVersion =>
        BinaryPrimitives.ReverseEndianness(_protocolVersion);
}