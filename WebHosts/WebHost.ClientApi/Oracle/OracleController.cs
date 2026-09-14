using System;
using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Oracle.Models;

namespace WebHost.ClientApi.Oracle;

[ApiController]
public class OracleController : ControllerBase
{
    [Route("/api/v1/oracle/ticket")]
    [HttpPost]
    public OracleTicket GetOracleTicket()
    {
        var host = Environment.GetEnvironmentVariable("PIN_PUBLIC_HOST") ?? "100.72.127.15";

        // HTTP overrides; client on alienware is patched for Oracle-over-HTTP.
        return new OracleTicket
               {
                   MatrixUrl = $"{host}:25000",
                   Ticket = "UjX52MMObRnEtgZe1pjrRFS6iRz3t7aR69fgLdzwxJQumRt7mhpNqPkejXFTBf3H2a5bZI/zQhO4CvKj+Z5Jctk4yMU4mgPzHiN+FJb+CiKvcQGhjNqAskD3alZQkZ/N+v1dSC25DLGR0Ky/3V1fsw0Y2bh+xsAgoKg1BkIJHiltTW3spuVTUd8fo9oLG0UzhCWP/NNIfcGX+Ur/e7UYxoUCiwHhRH3673Q1TtCoociHwvpjp4QExjp3Cd2LTolR00l8zYAvodMBPJyOuMf/BB8KDkoP8hnpNh8ZIpmxeWXrdZ2R5r8hSAIht3uNMZd/Wa3ewQgqwj/womRSCqhSOpdPFebbgI2TVnth7IA0Zq4EvvI436cBOc1P1wVfvFW6EUebqCzfIxn63UYQWXc1+KnCjLh9r4l60xm36Yes+7zJwS2r02UslF+QgpUuXJw4I4h7OK+YRrHnOFtiKOUnC3hJMUbY6yZAR6/ZdfvBLt9XlA==",
                   Datacenter = host,
                   OperatorOverride = new OperatorOverride
                                     {
                                         IngameHost = $"http://{host}:4403",
                                         ClientapiHost = $"http://{host}:4402"
                                     },
                   SessionId = new Guid("11111111-2222-3333-4444-555555555555"),
                   Hostname = host,
                   Country = "US"
               };
    }
}