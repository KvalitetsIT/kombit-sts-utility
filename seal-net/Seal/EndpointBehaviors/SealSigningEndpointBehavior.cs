using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace dk.nsi.seal
{
    public class SealSigningEndpointBehavior : IEndpointBehavior
    {
        readonly ClientCredentials clientCredentials;

        public SealSigningEndpointBehavior(ClientCredentials clientCredentials) => this.clientCredentials = clientCredentials;

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) => 
            clientRuntime.ClientMessageInspectors.Add(new SealSigningInspector { clientCredentials = clientCredentials });

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) => throw new NotImplementedException();

        public void Validate(ServiceEndpoint endpoint) { }
    }
}
