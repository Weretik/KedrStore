using System;
using System.ServiceModel;
using BuildingBlocks.Integrations.OneC.Generated;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Integrations.OneC;

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
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("[DEBUG_LOG] ERROR: OneC Credentials are empty!");
        }
        else
        {
             // Temporary debug: show first 2 chars of pass to verify it's not literal "latest" or something
             Console.WriteLine($"[DEBUG_LOG] OneC Auth Check: User starts with '{(username.Length > 0 ? username[0] : "")}', Pass starts with '{(password.Length > 0 ? password[0] : "")}'");
        }


        var isHttps = endpoint.StartsWith("https", StringComparison.OrdinalIgnoreCase);

        var binding = new BasicHttpBinding(isHttps ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None)
        {
            MaxReceivedMessageSize = int.MaxValue,
            AllowCookies = true,
            SendTimeout = TimeSpan.FromMinutes(5),
            ReceiveTimeout = TimeSpan.FromMinutes(5)
        };

        var address = new EndpointAddress(endpoint);

        var client = new Kontra1PortTypeClient(binding, address);

        // Используем кастомный инспектор, так как стандартный ClientCredentials часто конфликтует с настройками 1С Web-сервисов
        client.Endpoint.EndpointBehaviors.Add(new BasicAuthEndpointBehavior(username, password));

        return client;
    }
}
