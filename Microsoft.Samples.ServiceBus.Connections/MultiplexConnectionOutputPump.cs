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
    using System.IO;
    using System.Net.Sockets;

    public class MultiplexConnectionOutputPump : Pump
    {
        BufferRead bufferRead;
        BufferWrite bufferWrite;
        byte[] inputBuffer;
        object threadLock = new object();
        int connectionId;
        const int preambleSize = sizeof(int) + sizeof(ushort);

        public MultiplexConnectionOutputPump(BufferRead bufferRead, BufferWrite bufferWrite, int connectionId)
        {
            this.bufferRead = bufferRead;
            this.bufferWrite = bufferWrite;
            this.inputBuffer = new byte[65536];
            this.connectionId = connectionId;
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
            this.bufferRead.BeginInvoke(inputBuffer, preambleSize, inputBuffer.Length-preambleSize, DoneReading, null);
            return this.Caller;
        }

        public void DoneReading(IAsyncResult readOutputAsyncResult)
        {
            int bytesRead;
            try
            {
                try
                {
                    bytesRead = this.bufferRead.EndInvoke(readOutputAsyncResult);
                }
                catch (IOException ioe)
                {
                    if (ioe.InnerException is SocketException &&
                        (((SocketException)ioe.InnerException).ErrorCode == 10004 ||
                         ((SocketException)ioe.InnerException).ErrorCode == 10054))
                    {
                        Trace.TraceInformation("Socket cancelled with code {0} during pending read: {1}", ((SocketException)ioe.InnerException).ErrorCode, ioe.Message);
                    }
                    else
                    {
                        Trace.TraceError("Unable to read from source: {0}", ioe.Message);
                    }
                    bytesRead = 0;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Unable to read from source: {0}", ex.Message);
                    bytesRead = 0;
                }

                if (bytesRead > 0)
                {
                    lock (this.threadLock)
                    {
                        byte[] connectionIdPreamble = BitConverter.GetBytes(connectionId);
                        Buffer.BlockCopy(connectionIdPreamble, 0, inputBuffer, 0, sizeof(int));
                        byte[] sizePreamble = BitConverter.GetBytes((ushort)bytesRead);
                        Buffer.BlockCopy(sizePreamble, 0, inputBuffer, sizeof(int), sizeof(ushort));

                        this.bufferWrite(inputBuffer, 0, bytesRead + preambleSize);
                    }

                    if (!IsClosed)
                    {
                        try
                        {
                            this.bufferRead.BeginInvoke(inputBuffer, preambleSize, inputBuffer.Length - preambleSize, DoneReading, null);
                        }
                        catch(Exception ex)
                        {
                            Trace.TraceError("Can't start reading from source: {0}", ex.Message);
                            this.SetComplete(ex);
                        }
                    }
                }
                else 
                {
                    lock (this.threadLock)
                    {
                        byte[] connectionIdPreamble = BitConverter.GetBytes(connectionId);
                        Buffer.BlockCopy(connectionIdPreamble, 0, inputBuffer, 0, sizeof(int));
                        byte[] sizePreamble = BitConverter.GetBytes((ushort)0);
                        Buffer.BlockCopy(sizePreamble, 0, inputBuffer, sizeof(int), sizeof(ushort));

                        this.bufferWrite(inputBuffer, 0, preambleSize);
                    }
                    SetComplete();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to write to target: {0}", ex.Message);
                SetComplete(ex);
            }
        }
    }
}
