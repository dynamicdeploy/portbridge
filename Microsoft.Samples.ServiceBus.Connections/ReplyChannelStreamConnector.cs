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
    using System.IO;

    class ReplyChannelStreamConnector : IDataExchange
    {
        Stream stream;

        public ReplyChannelStreamConnector(Stream stream)
        {
            this.stream = stream;
        }

        public void Connect(string connectionInfo)
        {
        }

        public void Write(TransferBuffer data)
        {
            if (data.Data.Array != null)
            {
                this.stream.Write(data.Data.Array, data.Data.Offset, data.Data.Count);
            }
        }

        public void Disconnect()
        {
        }
    }
}
