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
    using System.IO.Pipes;
    using System.ServiceModel;
    using Microsoft.ServiceBus;
    using System.Security.AccessControl;
    using System.Security.Principal;

    public class NamedPipeClientConnectionForwarder : IDisposable, IClientConnectionForwarder
    {
        NetTcpRelayBinding streamBinding;
        TransportClientEndpointBehavior relayCreds;
        Uri endpointVia;
        Microsoft.Samples.ServiceBus.Connections.QueueBufferedStream multiplexedOutputStream;
        MultiplexConnectionInputPump inputPump;
        StreamBufferWritePump outputPump;
        Dictionary<int, MultiplexedPipeConnection> connections;
        object connectionLock = new object();
        object connectLock = new object();
        string toPipe;
        string localPipe;
        IDataExchangeChannel dataChannel;
        ChannelFactory<IDataExchangeChannel> dataChannelFactory;


        public NamedPipeClientConnectionForwarder(string serviceNamespace, string issuerName, string issuerSecret, string targetHost, string localPipe, string toPipe, bool useHybrid)
        {
            this.toPipe = toPipe;
            this.localPipe = localPipe;

            this.connections = new Dictionary<int, MultiplexedPipeConnection>();
            this.endpointVia = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, string.Format("/PortBridge/{0}", targetHost));
            this.streamBinding = CreateClientBinding(useHybrid);

            this.relayCreds = new TransportClientEndpointBehavior();
            //this.relayCreds.CredentialType = TransportClientCredentialType.SharedSecret;
            //this.relayCreds.Credentials.SharedSecret.IssuerName = issuerName;
            //this.relayCreds.Credentials.SharedSecret.IssuerSecret = issuerSecret;

            this.relayCreds.TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuerName, issuerSecret);
        }

        public void Dispose()
        {
            lock (connectLock)
            {
                if (this.dataChannel != null)
                {
                    if (this.dataChannel.State == CommunicationState.Opened)
                    {
                        this.dataChannel.Close();
                    }
                }
                if (this.dataChannelFactory != null)
                {
                    if (this.dataChannelFactory.State == CommunicationState.Opened)
                    {
                        this.dataChannelFactory.Close();
                    }
                }
            }
        }


        public void Open()
        {
            try
            {
                PipeSecurity pipeSecurity = new PipeSecurity();
                // deny network access, allow read/write to everyone locally and the owner/creator
                pipeSecurity.SetSecurityDescriptorSddlForm("D:(D;;FA;;;NU)(A;;0x12019f;;;WD)(A;;0x12019f;;;CO)");

                NamedPipeServerStream pipeListener = 
                    new NamedPipeServerStream(localPipe, PipeDirection.InOut, 
                                              NamedPipeServerStream.MaxAllowedServerInstances, 
                                              PipeTransmissionMode.Message, PipeOptions.Asynchronous, 
                                              4096, 4096, pipeSecurity);
                pipeListener.BeginWaitForConnection(ClientAccepted, pipeListener);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to open listener: {0}", ex.Message);
                throw;
            }
        }

        public void Close()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to stop listener: {0}", ex.Message);
                throw;
            }
        }

        MultiplexedConnection CorrelateConnection(int connectionId, object state)
        {
            MultiplexedPipeConnection connection = null;
            connections.TryGetValue(connectionId, out connection);
            return connection;
        }

        void ClientAccepted(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeServerStream pipeListener = asyncResult.AsyncState as NamedPipeServerStream;
                pipeListener.EndWaitForConnection(asyncResult);

                NamedPipeServerStream nextPipeListener = new NamedPipeServerStream(localPipe, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                nextPipeListener.BeginWaitForConnection(ClientAccepted, nextPipeListener);

                try
                {
                    Stream socketStream = pipeListener;

                    EnsureConnection();

                    MultiplexedPipeConnection multiplexedConnection = new MultiplexedPipeConnection(pipeListener, this.multiplexedOutputStream);
                    multiplexedConnection.Closed += new EventHandler(MultiplexedConnectionClosed);
                    lock (connectionLock)
                    {
                        connections.Add(multiplexedConnection.Id, multiplexedConnection);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Unable to establish connection: {0}", ex.Message);
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("Failure accepting client: {0}", ex.Message);
            }
        }

        void MultiplexedConnectionClosed(object sender, EventArgs e)
        {
            MultiplexedPipeConnection connection = (MultiplexedPipeConnection)sender;
            connections.Remove(connection.Id);
        }

        void EnsureConnection()
        {
            lock (connectLock)
            {
                if (this.dataChannel == null || this.dataChannel.State != CommunicationState.Opened)
                {
                    this.multiplexedOutputStream = new ThrottledQueueBufferedStream(10);

                    Microsoft.Samples.ServiceBus.Connections.QueueBufferedStream multiplexedInputStream = new Microsoft.Samples.ServiceBus.Connections.QueueBufferedStream();
                    this.dataChannelFactory = CreateDataChannelFactory(multiplexedInputStream);
                    this.dataChannelFactory.Open();

                    this.dataChannel = dataChannelFactory.CreateChannel(new EndpointAddress("sb:"), endpointVia);

                    try
                    {
                        this.dataChannel.Open();
                        this.dataChannel.Closed += DataChannelClosed;
                        this.dataChannel.Faulted += DataChannelClosed;

                        IHybridConnectionStatus status = dataChannel.GetProperty<IHybridConnectionStatus>();
                        if (status != null)
                        {
                            status.ConnectionStateChanged += (o, e) =>
                            {
                                Trace.TraceInformation("Data channel upgraded to direct connection.");
                            };
                        }

                        this.dataChannel.Connect("np:"+toPipe);

                        this.inputPump = new MultiplexConnectionInputPump(multiplexedInputStream.Read, CorrelateConnection, null);
                        this.inputPump.Run(false);

                        this.outputPump = new StreamBufferWritePump(multiplexedOutputStream, WriteToDataChannel);
                        this.dataChannel.Extensions.Add(new DataExchangeChannelFaultHelper(outputPump));
                        this.outputPump.BeginRunPump(MultiplexPumpCompleted, null);

                        return;
                    }
                    catch (AuthorizationFailedException af)
                    {
                        Trace.TraceError("Authorization failed: {0}", af.Message);
                        if (dataChannel != null)
                        {
                            dataChannel.Abort();
                            dataChannel = null;
                        }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        this.dataChannelFactory.Abort();
                        this.dataChannelFactory = null;

                        Trace.TraceError("Unable to establish data channel: {0}", ex.Message);
                        if (dataChannel != null)
                        {
                            dataChannel.Abort();
                            dataChannel = null;
                        }
                        throw;
                    }
                }
            }
        }

        void MultiplexPumpCompleted(IAsyncResult a)
        {
            try
            {
                try
                {
                    Pump.EndRunPump(a);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Multiplex pump failed: {0}", ex.Message);
                }

                lock (connectLock)
                {
                    if (dataChannel != null && dataChannel.State == CommunicationState.Opened)
                    {
                        dataChannel.Disconnect();
                        dataChannel.Close();
                    }
                    dataChannel = null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error closing data channel: {0}", ex.Message);
                lock (connectLock)
                {
                    if (dataChannel != null && dataChannel.State == CommunicationState.Opened)
                    {
                        dataChannel.Abort();
                    }
                    dataChannel = null;
                }
            }
            finally
            {
                foreach (MultiplexedPipeConnection connection in new List<MultiplexedPipeConnection>(connections.Values))
                {
                    try
                    {
                        connections.Remove(connection.Id);
                        connection.Dispose();
                    }
                    catch (Exception ex1)
                    {
                        Trace.TraceError("Error shutting down multiplex connection: {0}", ex1.Message);
                    }
                }
            }
        }

        private void WriteToDataChannel(byte[] b, int o, int s)
        {
            lock (connectLock)
            {
                try
                {
                    dataChannel.Write(new TransferBuffer(b, o, s));
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Failed writing to data channel: {0}", ex.Message);
                    if (dataChannel != null)
                    {
                        dataChannel.Abort();
                        dataChannel = null;
                    }
                    throw;
                }
            }
        }

        void DataChannelClosed(object sender, EventArgs e)
        {
            foreach (MultiplexedPipeConnection connection in new List<MultiplexedPipeConnection>(connections.Values))
            {
                try
                {
                    connections.Remove(connection.Id);
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error shutting down multiplex connection: {0}", ex.Message);
                }
            }
        }

        NetTcpRelayBinding CreateClientBinding(bool useHybrid)
        {
            NetTcpRelayBinding clientBinding = new NetTcpRelayBinding(useHybrid ? EndToEndSecurityMode.None : EndToEndSecurityMode.Transport, RelayClientAuthenticationType.RelayAccessToken);
            clientBinding.TransferMode = TransferMode.Buffered;
            clientBinding.MaxReceivedMessageSize = 1024 * 1024;
            clientBinding.MaxBufferSize = 1024 * 1024;
            clientBinding.SendTimeout = TimeSpan.FromSeconds(120);
            clientBinding.ReceiveTimeout = TimeSpan.FromHours(1);
            clientBinding.ConnectionMode = useHybrid ? TcpRelayConnectionMode.Hybrid : TcpRelayConnectionMode.Relayed;
            return clientBinding;
        }

        private DuplexChannelFactory<IDataExchangeChannel> CreateDataChannelFactory(Microsoft.Samples.ServiceBus.Connections.QueueBufferedStream multiplexedInputStream)
        {
            ReplyChannelStreamConnector connector = new ReplyChannelStreamConnector(multiplexedInputStream);
            DuplexChannelFactory<IDataExchangeChannel> dataChannelFactory = new DuplexChannelFactory<IDataExchangeChannel>(connector, streamBinding);
            dataChannelFactory.Endpoint.Behaviors.Add(relayCreds);
            return dataChannelFactory;
        }
    }
}
