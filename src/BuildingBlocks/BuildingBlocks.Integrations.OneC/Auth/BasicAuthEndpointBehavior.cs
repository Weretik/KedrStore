using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace BuildingBlocks.Integrations.OneC;

public sealed class BasicAuthEndpointBehavior(string username, string password) : IEndpointBehavior
{
    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        => clientRuntime.ClientMessageInspectors.Add(new BasicAuthMessageInspector(username, password));

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }

    private sealed class BasicAuthMessageInspector : IClientMessageInspector
    {
        private readonly string _authHeaderValue;

        public BasicAuthMessageInspector(string username, string password)
        {
            var raw = $"{username}:{password}";
            _authHeaderValue = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(raw));
        }

        public object? BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
        {
            var http = request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out var existing)
                ? (HttpRequestMessageProperty)existing
                : new HttpRequestMessageProperty();

            http.Headers["Authorization"] = _authHeaderValue;
            request.Properties[HttpRequestMessageProperty.Name] = http;

            Console.WriteLine($"[DEBUG_LOG] WCF Request: Authorization header set (Value starts with: '{_authHeaderValue.Substring(0, Math.Min(10, _authHeaderValue.Length))}...')");

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
    }
}
