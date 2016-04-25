using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Core.Common.ServiceModel.Attributes
{
    public class NotSupportedExceptionHandlerAttribute : Attribute, IErrorHandler, IServiceBehavior
    {
        #region IErrorHandler implementation

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is NotSupportedException)
            {
                var faultException = new FaultException<NotSupportedException>(new NotSupportedException(error.Message), error.Message);
                fault = Message.CreateMessage(version, faultException.CreateMessageFault(), faultException.Action);
            }
            else
            {
                fault = null;
            }
        }

        public bool HandleError(Exception error)
        {
            return true;
        }

        #endregion

        #region IServiceBehavior implementation


        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = (ChannelDispatcher)channelDispatcherBase;
                channelDispatcher.ErrorHandlers.Add(this);
            }
        }

        #endregion
    }
}
