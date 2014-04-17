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
    using System.Collections.Generic;
    using Microsoft.Samples.ServiceBus.Connections;

    class PortBridgeServiceForwarderHost
    {
        List<ServiceConnectionForwarder> forwarders;

        public PortBridgeServiceForwarderHost()
        {
            this.forwarders = new List<ServiceConnectionForwarder>();
        }

        public List<ServiceConnectionForwarder> Forwarders
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
                forwarder.OpenService();
            }
        }

        public void Close()
        {
            foreach (var forwarder in forwarders)
            {
                forwarder.CloseService();
            }
        }
    }
}
