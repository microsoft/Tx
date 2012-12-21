using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace WcfInterception
{
    class InterceptorEndpointBehavior : InterceptorBehaviorExtension, IEndpointBehavior
    {
        public InterceptorEndpointBehavior()
            : base()
        {
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
            // Not used.
        }

        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            // Not used.
        }

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher.DispatchRuntime.Operations.Count > 0)
            {
                foreach (DispatchOperation dispatchOperation in endpointDispatcher.DispatchRuntime.Operations)
                {
                    dispatchOperation.ParameterInspectors.Add(new ParameterInspector());
                }

                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new MessageInspector());
            }
        }

        public void Validate(ServiceEndpoint serviceEndpoint)
        {
            // Not used.
        }

        #endregion
    }
}
