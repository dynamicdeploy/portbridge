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

    class MultiplexedPipeConnection : MultiplexedConnection
    {
        NamedPipeServerStream pipeServer;
        MultiplexConnectionOutputPump outputPump;

        public event EventHandler Closed;

        public MultiplexedPipeConnection(NamedPipeServerStream pipeServer, Microsoft.Samples.ServiceBus.Connections.QueueBufferedStream multiplexedOutputStream)
            : base(pipeServer.Write)
        {
            this.pipeServer = pipeServer;
            this.outputPump = new MultiplexConnectionOutputPump(pipeServer.Read, multiplexedOutputStream.Write, Id);
            this.outputPump.BeginRunPump(PumpComplete, null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.pipeServer != null)
            {
                this.pipeServer.Close();
                this.pipeServer = null;
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
