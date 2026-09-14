namespace WebHost.ClientApi.Server.Models;

public class ServerZone
{
    public string ZoneName { get; set; }
    public string MatrixUrl { get; set; }
    public int ProtocolVersion { get; set; }
    public int Match { get; set; }
    public int Revision { get; set; }
    public string Owner { get; set; }
    public int Players { get; set; }
    public string Build { get; set; }
}
