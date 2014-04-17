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
    using System.IO.Pipes;

    class MultiplexedServiceNamedPipeConnection : MultiplexedConnection
    {
        MultiplexConnectionOutputPump outputPump;
        StreamConnection streamConnection;
        NamedPipeClientStream pipeClient;

        public MultiplexedServiceNamedPipeConnection(StreamConnection streamConnection, NamedPipeClientStream pipeClient, int connectionId)
            : base(pipeClient.Write, connectionId)
        {
            this.streamConnection = streamConnection;
            this.pipeClient = pipeClient;

            this.outputPump = new MultiplexConnectionOutputPump(pipeClient.Read, streamConnection.Stream.Write, connectionId);
            this.outputPump.BeginRunPump(PumpCompleted, null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (pipeClient != null)
            {
                try
                {
                    pipeClient.Close();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error closing client: {0}", ex.Message);
                }
                pipeClient = null;
            }
        }

        void PumpCompleted(IAsyncResult asyncResult)
        {
            try
            {
                MultiplexConnectionOutputPump.EndRunPump(asyncResult);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error in pump: {0}", ex.Message);
            }
            Dispose();
        }
    }
}
