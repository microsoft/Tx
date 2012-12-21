using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace WcfInterception
{
    class ParameterInspector : IParameterInspector
    {
        #region IParameterInspector Members

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            // Not used.
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            WcfEvents.StartRequest(
                operationName,
                OperationContext.Current.RequestContext.RequestMessage.Headers.MessageId.ToString());
                    
            //interceptor.Trace("ParameterInspector.BeforeCall",
            //    OperationContext.Current,
            //    OperationContext.Current.RequestContext.RequestMessage,
            //    operationName,
            //    inputs,
            //    OperationContext.Current.RequestContext.RequestMessage.Headers.MessageId.ToString());

            return null;
        }

        #endregion
    }
}
