//---------------------------------------------------------------------------------
// Microsoft (R) .NET Services 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------


namespace Microsoft.Samples.ServiceBus.Connections
{
    using System;
    using System.ServiceModel;

    class DataExchangeChannelFaultHelper : IExtension<IContextChannel>
    {
        StreamBufferWritePump pump;

        public DataExchangeChannelFaultHelper(StreamBufferWritePump pump)
        {
            this.pump = pump;
        }

        public void Attach(IContextChannel owner)
        {
            owner.Faulted += new EventHandler(ChannelFaulted);
        }

        void ChannelClosed(object sender, EventArgs e)
        {
            this.pump.Dispose();
        }

        void ChannelFaulted(object sender, EventArgs e)
        {
            this.pump.Dispose();
        }

        public void Detach(IContextChannel owner)
        {
        }
    }
}
