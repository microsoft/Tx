// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace Tx.Samples.WCFInterception
{
    class MessageInspector : IDispatchMessageInspector
    {

        public MessageInspector()
        {
        }

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // Not used.
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            try
            {
                WcfEvents.EndRequest(OperationContext.Current.RequestContext.RequestMessage.Headers.MessageId.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }
}
