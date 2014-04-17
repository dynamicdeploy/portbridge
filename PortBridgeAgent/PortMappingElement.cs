//---------------------------------------------------------------------------------
// Microsoft (R) .NET Services 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------

namespace PortBridgeAgent
{
    using System;
    using System.Configuration;

    public class PortMappingElement : ConfigurationElement
    {
        internal const string localTcpPortString = "localTcpPort";
        internal const string remoteTcpPortString = "remoteTcpPort";
        internal const string localPipeString = "localPipe";
        internal const string remotePipeString = "remotePipe";
        internal const string targetHostString = "targetHost";
        internal const string bindToString = "bindTo";
        internal const string firewallRules = "firewallRules";

        public PortMappingElement(string targetHost, int localTcpPort, int remoteTcpPort, string bindTo)
        {
            this.TargetHost = targetHost;
            this.LocalTcpPort = localTcpPort;
            this.RemoteTcpPort = remoteTcpPort;
            this.BindTo = bindTo;
        }

        public PortMappingElement()
        {
        }

        [ConfigurationProperty(targetHostString, IsRequired = true)]
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


        [ConfigurationProperty(localTcpPortString, IsRequired = false)]
        public int? LocalTcpPort
        {
            get
            {
                return (int?)this[localTcpPortString];
            }
            set
            {
                this[localTcpPortString] = value;
            }
        }

        [ConfigurationProperty(remoteTcpPortString, IsRequired = false)]
        public int? RemoteTcpPort
        {
            get
            {
                return (int?)this[remoteTcpPortString];
            }
            set
            {
                this[remoteTcpPortString] = value;
            }
        }

        [ConfigurationProperty(localPipeString, IsRequired = false)]
        public string LocalPipe
        {
            get
            {
                return (string)this[localPipeString];
            }
            set
            {
                this[localPipeString] = value;
            }
        }

        [ConfigurationProperty(remotePipeString, IsRequired = false)]
        public string RemotePipe
        {
            get
            {
                return (string)this[remotePipeString];
            }
            set
            {
                this[remotePipeString] = value;
            }
        }

        [ConfigurationProperty(bindToString, IsRequired = false)]
        public string BindTo
        {
            get
            {
                return (string)this[bindToString];
            }
            set
            {
                this[bindToString] = value;
            }
        }

        [ConfigurationProperty(firewallRules, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(FirewallRuleCollection), AddItemName = "allow")]
        public FirewallRuleCollection FirewallRules
        {
            get
            {
                return (FirewallRuleCollection)base[firewallRules];
            }
        }
    }
}
