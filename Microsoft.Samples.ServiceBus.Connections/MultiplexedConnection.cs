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
    using System.Diagnostics;
    using System.Threading;

    public class MultiplexedConnection : IDisposable
    {
        static int lastConnection = 0;

        int connectionId;
        BufferWrite bufferWrite;
      
        public MultiplexedConnection(BufferWrite bufferWrite)
        {
            this.connectionId = Interlocked.Increment(ref lastConnection);
            this.bufferWrite = bufferWrite;
            Trace.TraceInformation("Connection {0} created", connectionId);
        }

        public MultiplexedConnection(BufferWrite bufferWrite, int connectionId)
        {
            this.connectionId = connectionId;
            this.bufferWrite = bufferWrite;
            Trace.TraceInformation("Connection {0} created", connectionId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Trace.TraceInformation("Connection {0} completed", connectionId);
        }

        public int Id
        {
            get
            {
                return this.connectionId;
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (bufferWrite != null)
            {
                bufferWrite(buffer, offset, count);
            }
        }
    }
}
