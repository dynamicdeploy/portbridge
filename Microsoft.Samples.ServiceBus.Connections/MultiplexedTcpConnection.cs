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

    class MultiplexedTcpConnection : MultiplexedConnection
    {
        TcpClient tcpClient;
        MultiplexConnectionOutputPump outputPump;

        public event EventHandler Closed;

        public MultiplexedTcpConnection(TcpClient tcpClient, QueueBufferedStream multiplexedOutputStream)
            : base(tcpClient.GetStream().Write)
        {
            this.tcpClient = tcpClient;
            this.outputPump = new MultiplexConnectionOutputPump(tcpClient.GetStream().Read, multiplexedOutputStream.Write, Id);
            this.outputPump.BeginRunPump(PumpComplete, null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.tcpClient != null)
            {
                this.tcpClient.Close();
                this.tcpClient = null;
            }
            if (this.outputPump != null)
            {
                this.outputPump.Dispose();
                this.outputPump = null;
            }
        }


        void PumpComplete(IAsyncResult a)
        {
            try
            {
                MultiplexConnectionOutputPump.EndRunPump(a);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure in multiplex pump: {0}", ex.Message);
            }

            if (this.Closed != null)
            {
                Closed(this, new EventArgs());
            }

            this.Dispose();
        }
    }
}
