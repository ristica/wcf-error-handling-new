using System;
using System.ServiceModel.Configuration;
using Core.Common.ServiceModel.Attributes;

namespace Core.Common.ServiceModel.Extensions
{
    public class NotSupportedExceptionHandlerExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new NotSupportedExceptionHandlerAttribute();
        }

        public override Type BehaviorType => typeof (NotSupportedExceptionHandlerAttribute);
    }
}
