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
        var securityMode = isHttps ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.TransportCredentialOnly;

        var binding = new BasicHttpBinding(securityMode)
        {
            MaxReceivedMessageSize = int.MaxValue,
            AllowCookies = true,
            SendTimeout = TimeSpan.FromMinutes(5),
            ReceiveTimeout = TimeSpan.FromMinutes(5),
            Security = { Transport = { ClientCredentialType = HttpClientCredentialType.Basic } }
        };

        var address = new EndpointAddress(endpoint);

        var client = new Kontra1PortTypeClient(binding, address);

        client.ClientCredentials.UserName.UserName = username;
        client.ClientCredentials.UserName.Password = password;

        return client;
    }
}
