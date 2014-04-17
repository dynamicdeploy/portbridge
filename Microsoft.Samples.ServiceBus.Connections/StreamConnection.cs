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

    public class StreamConnection
    {
        Stream stream;
        string connectionInfo;

        public StreamConnection(Stream stream, string connectionInfo)
        {
            this.stream = stream;
            this.connectionInfo = connectionInfo;
        }

        public Stream Stream
        {
            get
            {
                return stream;
            }
        }

        public string ConnectionInfo
        {
            get
            {
                return connectionInfo;
            }
        }

    }

}
