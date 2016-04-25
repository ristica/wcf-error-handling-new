using System;
using System.ServiceModel.Configuration;
using Core.Common.ServiceModel.Attributes;

namespace Core.Common.ServiceModel.Extensions
{
    public class ArgumentExceptionHandlerExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new ArgumentExceptionHandlerAttribute();
        }

        public override Type BehaviorType => typeof(ArgumentExceptionHandlerAttribute);
    }
}
