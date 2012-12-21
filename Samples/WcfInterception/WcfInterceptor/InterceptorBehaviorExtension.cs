using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;

namespace WcfInterception
{
    public class InterceptorBehaviorExtension : BehaviorExtensionElement
    {
        private const string ApplicationNameProperty = "applicationName";

        object sync = new object();

        static InterceptorBehaviorExtension()
        {
        }

        public InterceptorBehaviorExtension()
        {
        }

        // Not used.
        [ConfigurationProperty(ApplicationNameProperty)]
        public string ApplicationName
        {
            get { return (string)base[ApplicationNameProperty]; }
            set { base[ApplicationNameProperty] = value; }
        }

        public override Type BehaviorType
        {
            get { return this.GetType(); }
        }

        protected override object CreateBehavior()
        {
            return new InterceptorEndpointBehavior();
        }
    }
}
