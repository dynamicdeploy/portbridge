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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;

    public class MultiplexConnectionInputPump
    {
        BufferRead bufferRead;
        byte[] inputBuffer;
        byte[] preambleBuffer;
        ManualResetEvent stopInput;
        bool stopped;
        bool closed;
        Dictionary<int, MultiplexedConnection> connections;
        public EventHandler Completed;
        object connectionLock = new object();
        MultiplexConnectionFactoryHandler connectionFactory;
        object callbackState;

        public MultiplexConnectionInputPump(BufferRead bufferRead, MultiplexConnectionFactoryHandler connectionFactory, object callbackState)
        {
            this.callbackState = callbackState;
            this.bufferRead = bufferRead;
            this.connectionFactory = connectionFactory;
            this.connections = new Dictionary<int, MultiplexedConnection>();
            this.inputBuffer = new byte[65536];
            this.preambleBuffer = new byte[sizeof(int) + sizeof(ushort)];
            this.stopInput = new ManualResetEvent(false);
        }

        public virtual void Close()
        {
            if (!closed)
            {
                this.closed = this.stopped = true;
                this.stopInput.Set();
            }
        }

        public void Run()
        {
            Run(true);
        }

        public void Run(bool completeSynchronously)
        {
            // read from delegate
            this.bufferRead.BeginInvoke(preambleBuffer, 0, preambleBuffer.Length, DoneReadingPreamble, null);
            if (completeSynchronously)
            {
                this.stopInput.WaitOne();
            }
        }

        public void DoneReadingPreamble(IAsyncResult readOutputAsyncResult)
        {
            try
            {
                int bytesRead = this.bufferRead.EndInvoke(readOutputAsyncResult);
                if (bytesRead > 0)
                {
                    if (bytesRead == 1)
                    {
                        bytesRead += this.bufferRead(preambleBuffer, 1, preambleBuffer.Length - 1);
                    }

                    int connectionId = BitConverter.ToInt32(preambleBuffer, 0);
                    ushort frameSize = BitConverter.ToUInt16(preambleBuffer, sizeof(Int32));

                    // we have to get the frame off the wire irrespective of 
                    // whether we can dispatch it
                    if (frameSize > 0)
                    {
                        // read the block synchronously
                        bytesRead = 0;
                        do
                        {
                            bytesRead += this.bufferRead(inputBuffer, bytesRead, (int)frameSize - bytesRead);
                        }
                        while (bytesRead < frameSize);
                    }

                    MultiplexedConnection connection;

                    lock (connectionLock)
                    {
                        if (!connections.TryGetValue(connectionId, out connection))
                        {
                            try
                            {
                                connection = connectionFactory(connectionId, callbackState);
                                if (connection != null)
                                {
                                    connections.Add(connectionId, connection);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Unable to establish multiplexed connection: {0}", ex.Message);
                                connection = null;
                            }
                        }
                    }

                    if (connection != null)
                    {
                        bool shutdownConnection = (frameSize == 0);
                        if (frameSize > 0)
                        {
                            try
                            {
                                connection.Write(inputBuffer, 0, frameSize);
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
                                    Trace.TraceError("Unable to write to multiplexed connection: {0}", ioe.Message);
                                }
                                shutdownConnection = true;
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Unable to write to multiplexed connection: {0}", ex.Message);
                                shutdownConnection = true;
                            }
                        }

                        if (shutdownConnection)
                        {
                            connection.Dispose();
                            lock (connectionLock)
                            {
                                if (connections.ContainsKey(connectionId))
                                {
                                    connections.Remove(connectionId);
                                }
                            }
                        }
                    }

                    if (!stopped)
                    {
                        this.bufferRead.BeginInvoke(preambleBuffer, 0, preambleBuffer.Length, DoneReadingPreamble, null);
                    }
                }
                else
                {
                    OnCompleted();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error starting multiplex pump : {0}", ex.Message);
                OnCompleted();
            }
        }

        void OnCompleted()
        {
            this.Close();
            if (Completed != null)
            {
                Completed(this, new EventArgs());
            }
        }
    }

    public delegate MultiplexedConnection MultiplexConnectionFactoryHandler(int connectionId, object state);

}

