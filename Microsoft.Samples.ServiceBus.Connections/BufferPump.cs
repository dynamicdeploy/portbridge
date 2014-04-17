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
    using System.Threading;

    public class BufferPump : Pump
    {
        BufferRead bufferRead;
        BufferWrite bufferWrite;
        byte[] inputBuffer;
    
        public BufferPump(BufferRead bufferRead, BufferWrite bufferWrite)
            : this(bufferRead, bufferWrite, 65536)
        {
        }

        public BufferPump(BufferRead bufferRead, BufferWrite bufferWrite, int bufferSize)
        {
            this.bufferRead = bufferRead;
            this.bufferWrite = bufferWrite;
            this.inputBuffer = new byte[bufferSize];
        }

        public override IAsyncResult BeginRunPump(AsyncCallback callback, object state)
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Already running");
            }
            else
            {
                this.IsRunning = true;
            }

            this.Caller = new PumpAsyncResult(callback, state);

            this.bufferRead.BeginInvoke(inputBuffer, 0, inputBuffer.Length, DoneReading, null);
         
            return this.Caller;
        }

        public void DoneReading(IAsyncResult readOutputAsyncResult)
        {
            try
            {
                int bytesRead = this.bufferRead.EndInvoke(readOutputAsyncResult);
                if (bytesRead > 0)
                {
                    this.bufferWrite(inputBuffer, 0, bytesRead);
                    if (!this.IsClosed)
                    {
                        this.bufferRead.BeginInvoke(inputBuffer, 0, inputBuffer.Length, DoneReading, null);
                    }
                }
                else
                {
                    SetComplete();
                }
            }
            catch (Exception ex)
            {
                SetComplete(ex);
            }            
        }
    }
}
