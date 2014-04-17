//---------------------------------------------------------------------------------
// Microsoft (R) .NET Services 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------

namespace PortBridge
{
    using System;
    using System.Configuration;

    public class HostMappingElement : ConfigurationElement
    {
        internal const string targetHostString = "targetHost";
        internal const string allowedPortsString = "allowedPorts";
        internal const string allowedPipesString = "allowedPipes";

        public HostMappingElement(string targetHost, string allowedPorts, string allowedPipes)
        {
            this.TargetHost = targetHost;
            this.AllowedPorts = allowedPorts;
            this.AllowedPipes = allowedPipes;
        }

        public HostMappingElement()
        {
        }

        [ConfigurationProperty(targetHostString, DefaultValue = "localhost", IsRequired = false, IsKey = true)]
        public string TargetHost
        {
            get
            {
                return (string)this[targetHostString];
            }
            set
            {
                this[targetHostString] = value;
            }
        }

        [ConfigurationProperty(allowedPortsString, DefaultValue = "*", IsRequired = false)]
        public string AllowedPorts
        {
            get
            {
                return (string)this[allowedPortsString];
            }
            set
            {
                this[allowedPortsString] = value;
            }
        }

        [ConfigurationProperty(allowedPipesString, DefaultValue = "", IsRequired = false)]
        public string AllowedPipes
        {
            get
            {
                return (string)this[allowedPipesString];
            }
            set
            {
                this[allowedPipesString] = value;
            }
        }
    }
}
