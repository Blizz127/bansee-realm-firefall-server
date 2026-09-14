using System.Numerics;

namespace GameServer.StaticDB.Records.customdata;

public record class ZoneNpc
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }

    public uint Type { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }

    /// <summary>Optional nameplate override when SDB LocalizedNameId is empty/wrong (retail DisplayName).</summary>
    public string DisplayName { get; set; }

    /// <summary>Optional provenance / notes (ignored by loader if present as comment fields).</summary>
    public string Comment { get; set; }
}
