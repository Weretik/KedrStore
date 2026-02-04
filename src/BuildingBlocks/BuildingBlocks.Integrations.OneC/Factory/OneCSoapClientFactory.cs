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
