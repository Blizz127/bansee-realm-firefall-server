using System;
using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Server.Models;

namespace WebHost.ClientApi.Server;

[ApiController]
public class ServerController : ControllerBase
{
    [Route("api/v1/server/list")]
    [HttpPost]
    [HttpGet]
    [Produces("application/json")]
    public ServerList GetServerList()
    {
        var host = Environment.GetEnvironmentVariable("PIN_PUBLIC_HOST") ?? "100.72.127.15";
        var matrixUrl = $"{host}:25000";

        // Client Open World automatch looks up zone_name "Copacabana Beta".
        // protocol_version / match / revision must stay 0-compatible with JoinWorld filters.
        var zones = new[]
                    {
                        new ServerZone
                        {
                            ZoneName = "Copacabana Beta",
                            MatrixUrl = matrixUrl,
                            ProtocolVersion = 0,
                            Match = 0,
                            Revision = 0,
                            Owner = "PIN",
                            Players = 0,
                            Build = string.Empty
                        },
                        new ServerZone
                        {
                            ZoneName = "New Eden",
                            MatrixUrl = matrixUrl,
                            ProtocolVersion = 0,
                            Match = 0,
                            Revision = 0,
                            Owner = "PIN",
                            Players = 0,
                            Build = string.Empty
                        }
                    };

        return new ServerList { ZoneList = zones };
    }
}
