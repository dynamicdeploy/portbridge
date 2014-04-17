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

    class PortBridgeAgentSection : ConfigurationSection
    {
        internal const string portMappingsString = "portMappings";
        internal const string serviceBusNamespaceString = "serviceBusNamespace";
        internal const string serviceBusIssuerNameString = "serviceBusIssuerName";
        internal const string serviceBusIssuerSecretString = "serviceBusIssuerSecret";

        [ConfigurationProperty(serviceBusNamespaceString, DefaultValue = null, IsRequired = true)]
        public string ServiceNamespace
        {
            get
            {
                return (string)this[serviceBusNamespaceString];
            }
            set
            {
                this[serviceBusNamespaceString] = value;
            }
        }

        [ConfigurationProperty(serviceBusIssuerNameString, DefaultValue = "owner", IsRequired = false)]
        public string IssuerName
        {
            get
            {
                return (string)this[serviceBusIssuerNameString];
            }
            set
            {
                this[serviceBusIssuerNameString] = value;
            }
        }

        [ConfigurationProperty(serviceBusIssuerSecretString, DefaultValue = null, IsRequired = true)]
        public string IssuerSecret
        {
            get
            {
                return (string)this[serviceBusIssuerSecretString];
            }
            set
            {
                this[serviceBusIssuerSecretString] = value;
            }
        }

        [ConfigurationProperty(portMappingsString, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(PortMappingCollection), AddItemName = "port")]
        public PortMappingCollection PortMappings
        {
            get
            {
                return (PortMappingCollection)base[portMappingsString];
            }
        }

    }
}