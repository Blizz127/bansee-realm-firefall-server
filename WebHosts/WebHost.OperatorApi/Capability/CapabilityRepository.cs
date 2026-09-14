using System;
using System.Threading.Tasks;
using WebHost.OperatorApi.Exceptions;

namespace WebHost.OperatorApi.Capability;

public class CapabilityRepository : ICapabilityRepository
{
    public async Task<HostInformation> GetHostInformationAsync(string environment, int build)
    {
        var host = Environment.GetEnvironmentVariable("PIN_PUBLIC_HOST") ?? "100.72.127.15";

        // HTTP everywhere for login reliability. Alienware FirefallClient.exe is patched
        // to skip Oracle's "must be HTTPS" scheme check (see patch_pin_oracle.py).
        string Http(int port) => $"http://{host}:{port}";

        return await Task.FromResult(new HostInformation
                                     {
                                         FrontendHost = Http(4499),
                                         StoreHost = Http(4499),
                                         ChatServer = Http(4407),
                                         ReplayHost = $"{Http(4499)}/{environment}-{build}",
                                         WebHost = Http(4499),
                                         MarketHost = Http(4499),
                                         IngameHost = Http(4403),
                                         ClientapiHost = Http(4402),
                                         WebAssetHost = Http(4499),
                                         WebAccountsHost = Http(4499),
                                         RhsigscanHost = Http(4499)
                                     });
    }

    public async Task<ProductInformation> GetProductInformationAsync(string productName)
    {
        if (productName != "Firefall_Beta")
        {
            throw new NotFoundException($"Product '{productName}' is unknown");
        }

        return await Task.FromResult(new ProductInformation { Build = "beta-1973", Environment = "production", Region = "NA", PatchLevel = 0 });
    }
}
