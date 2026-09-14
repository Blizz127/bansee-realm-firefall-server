using System.Collections.Generic;

namespace WebHost.ClientApi.Server.Models;

public class ServerList
{
    public IEnumerable<ServerZone> ZoneList { get; set; }
}
