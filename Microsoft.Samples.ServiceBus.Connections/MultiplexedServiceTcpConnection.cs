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
    using System.Net.Sockets;

    class MultiplexedServiceTcpConnection : MultiplexedConnection
    {
        MultiplexConnectionOutputPump outputPump;
        StreamConnection streamConnection;
        TcpClient tcpClient;

        public MultiplexedServiceTcpConnection(StreamConnection streamConnection, TcpClient tcpClient, int connectionId)
            : base(tcpClient.GetStream().Write, connectionId)
        {
            this.streamConnection = streamConnection;
            this.tcpClient = tcpClient;

            this.outputPump = new MultiplexConnectionOutputPump(tcpClient.GetStream().Read, streamConnection.Stream.Write, connectionId);
            this.outputPump.BeginRunPump(PumpCompleted, null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (tcpClient != null)
            {
                try
                {
                    tcpClient.Close();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error closing client: {0}", ex.Message);
                }
                tcpClient = null;
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
