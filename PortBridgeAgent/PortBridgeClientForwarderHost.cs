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
    using System.Collections.Generic;
    using Microsoft.Samples.ServiceBus.Connections;

    class PortBridgeClientForwarderHost
    {
        List<IClientConnectionForwarder> forwarders;

        public PortBridgeClientForwarderHost()
        {
            this.forwarders = new List<IClientConnectionForwarder>();
        }

        public List<IClientConnectionForwarder> Forwarders
        {
            get
            {
                return forwarders;
            }
        }

        public void Open()
        {
            foreach (var forwarder in forwarders)
            {
                forwarder.Open();
            }
        }

        public void Close()
        {
            foreach (var forwarder in forwarders)
            {
                forwarder.Close();
            }
        }
    }
}
