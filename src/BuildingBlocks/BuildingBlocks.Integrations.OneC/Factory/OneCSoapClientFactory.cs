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


        var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
        {
            MaxReceivedMessageSize = int.MaxValue,
            AllowCookies = true
        };

        var address = new EndpointAddress(endpoint);

        var client = new Kontra1PortTypeClient(binding, address);

        client.Endpoint.EndpointBehaviors.Add(new BasicAuthEndpointBehavior(username, password));

        return client;
    }
}
