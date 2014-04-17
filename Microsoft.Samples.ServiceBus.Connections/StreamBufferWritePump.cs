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

    public class StreamBufferWritePump : BufferPump
    {
        Stream stream;

        public StreamBufferWritePump(Stream stream, BufferWrite bufferWrite)
            : base(stream.Read, bufferWrite)
        {
            this.stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
            }
        }
    }
}
