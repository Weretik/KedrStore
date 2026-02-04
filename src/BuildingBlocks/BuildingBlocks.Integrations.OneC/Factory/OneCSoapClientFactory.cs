using System.ServiceModel;
using BuildingBlocks.Integrations.OneC.Generated;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Integrations.OneC.Factory;

public sealed class OneCSoapClientFactory(IConfiguration configuration)
{
    public Kontra1PortTypeClient Create()
    {
        var endpoint = configuration["OneCSoap:Endpoint"]
                       ?? throw new InvalidOperationException("OneCSoap:Endpoint is missing");
        var username = configuration["OneCSoap:Username"]
                       ?? throw new InvalidOperationException("OneCSoap:Username is missing");
        var password = configuration["OneCSoap:Password"]
                       ?? throw new InvalidOperationException("OneCSoap:Password is missing");

        Console.WriteLine($"[DEBUG_LOG] OneC Credentials - User: '{username}', Pass length: {password?.Length ?? 0}");
        Console.WriteLine($"[DEBUG_LOG] OneC Endpoint: '{endpoint}'");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(endpoint))
        {
            Console.WriteLine("[DEBUG_LOG] ERROR: OneC Credentials or Endpoint are empty!");
        }
        else
        {
             Console.WriteLine($"[DEBUG_LOG] OneC Auth Check: User starts with '{(username.Length > 0 ? username[0] : "")}', Pass starts with '{(password.Length > 0 ? password[0] : "")}'");
             Console.WriteLine($"[DEBUG_LOG] OneC Raw UserName: '{username}'");
             Console.WriteLine($"[DEBUG_LOG] OneC Raw Pass: '{password}'");
        }


        if (string.IsNullOrEmpty(endpoint) || endpoint == "FROM_APPSETTINGS")
        {
            throw new InvalidOperationException("OneCSoap:Endpoint is not configured. Please set a valid URI in appsettings.json or environment variables.");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"OneCSoap:Endpoint '{endpoint}' is not a valid absolute URI.");
        }

        var isHttps = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

        var binding = new BasicHttpBinding(isHttps ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None)
        {
            MaxReceivedMessageSize = int.MaxValue,
            AllowCookies = true,
            SendTimeout = TimeSpan.FromMinutes(5),
            ReceiveTimeout = TimeSpan.FromMinutes(5)
        };

        var address = new EndpointAddress(endpoint);

        var client = new Kontra1PortTypeClient(binding, address);

        if (password != null)
        {
            client.Endpoint.EndpointBehaviors.Add(new BasicAuthEndpointBehavior(username, password));
        }

        return client;
    }
}
