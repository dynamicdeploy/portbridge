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
    using System.Net;
    using System.Net.Sockets;
    using System.ServiceModel;
    using Microsoft.ServiceBus;
    using System.Collections.Generic;

    public class ServiceConnectionForwarder
    {
        const string localPipePrefix = @"\\.\pipe\";
        bool useHybrid;
        StreamServerHost streamServerHost;
        Uri endpointVia;
        string endpointRole;
        NetTcpRelayBinding streamBinding;
        TransportClientEndpointBehavior relayCreds;
        List<int> allowedPorts;
        List<string> allowedPipes;
        bool noPortConstraints;
        bool noPipeConstraints;
        string targetHost;

        public ServiceConnectionForwarder(string serviceNamespace, string issuerName, string issuerSecret, string targetHost, string targetHostAlias, string allowedPortsString, string allowedPipesString, bool useHybrid)
        {
            this.useHybrid = useHybrid;
            this.targetHost = targetHost;
            this.noPipeConstraints = false;
            this.noPortConstraints = false;
            this.allowedPipes = new List<string>();
            this.allowedPorts = new List<int>();

            allowedPortsString = allowedPortsString.Trim();
            if (allowedPortsString == "*")
            {
                this.noPortConstraints = true;
            }
            else
            {
                noPortConstraints = false;
                string[] portList = allowedPortsString.Split(',');
                for (int i = 0; i < portList.Length; i++)
                {
                    this.allowedPorts.Add(int.Parse(portList[i].Trim()));
                }
            }

            allowedPipesString = allowedPipesString.Trim();
            if (allowedPipesString == "*")
            {
                noPipeConstraints = true;
            }
            else
            {
                noPipeConstraints = false;
                string[] pipeList = allowedPipesString.Split(',');
                for (int i = 0; i < pipeList.Length; i++)
                {
                    string pipeName = pipeList[i].Trim();
                    if (pipeName.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!pipeName.StartsWith(localPipePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ArgumentException(string.Format("Invalid pipe name in allowedPipesString. Only relative and local paths permitted: {0}", pipeName), "allowedPipesString");
                        }
                        else
                        {
                            pipeName = pipeName.Substring(localPipePrefix.Length);
                        }
                    }
                    this.allowedPipes.Add(pipeName);
                }
            }

            endpointVia = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, "./PortBridge/" + targetHostAlias);
            endpointRole = "sb:";

            streamBinding = new NetTcpRelayBinding(useHybrid ? EndToEndSecurityMode.None : EndToEndSecurityMode.Transport, RelayClientAuthenticationType.RelayAccessToken);
            streamBinding.TransferMode = TransferMode.Buffered;
            streamBinding.MaxReceivedMessageSize = 1024 * 1024;
            streamBinding.ConnectionMode = useHybrid ? TcpRelayConnectionMode.Hybrid : TcpRelayConnectionMode.Relayed;

            relayCreds = new TransportClientEndpointBehavior();
            //relayCreds.CredentialType = TransportClientCredentialType.SharedSecret;
            //relayCreds.Credentials.SharedSecret.IssuerName = issuerName;
            //relayCreds.Credentials.SharedSecret.IssuerSecret = issuerSecret;

            this.relayCreds.TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(issuerName, issuerSecret);
        }

        public bool OpenService()
        {
            this.streamServerHost = new StreamServerHost();

            var ep = streamServerHost.AddServiceEndpoint(typeof(IDataExchange), streamBinding, endpointRole, endpointVia);
            ep.Behaviors.Add(relayCreds);

            try
            {
                streamServerHost.Open();
                streamServerHost.BeginAccept(StreamAccepted, null);
            }
            catch (Exception e)
            {
                Trace.TraceError("Unable to connect: {0}", e.Message);
                return false;
            }
            return true;
        }

        public void CloseService()
        {
            try
            {
                streamServerHost.Close();
            }
            finally
            {
                streamServerHost.Abort();
            }
        }

        void StreamAccepted(IAsyncResult asyncResult)
        {
            try
            {
                StreamConnection streamConnection = streamServerHost.EndAccept(asyncResult);
                if (streamConnection != null)
                {
                    streamServerHost.BeginAccept(StreamAccepted, null);
                    if (streamConnection.ConnectionInfo.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
                    {
                        int port;

                        if (!int.TryParse(streamConnection.ConnectionInfo.Substring(4), out port))
                        {
                            try
                            {
                                streamConnection.Stream.Close();
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Error closing stream: {0}", ex.Message);
                            }
                            return;
                        }
                        else
                        {
                            bool portAllowed = noPortConstraints;
                            Trace.TraceInformation("Incoming connection for port {0}", port);
                            if (!portAllowed)
                            {
                                for (int i = 0; i < allowedPorts.Count; i++)
                                {
                                    if (port == allowedPorts[i])
                                    {
                                        portAllowed = true;
                                        break;
                                    }
                                }
                            }
                            if (!portAllowed)
                            {
                                Trace.TraceWarning("Incoming connection for port {0} not permitted", port);
                                try
                                {
                                    streamConnection.Stream.Close();
                                }
                                catch (Exception ex)
                                {
                                    Trace.TraceError("Error closing stream: {0}", ex.Message);
                                }
                                return;
                            }
                        }
                    }
                    else if (streamConnection.ConnectionInfo.StartsWith("np:", StringComparison.OrdinalIgnoreCase))
                    {
                        string pipeName = streamConnection.ConnectionInfo.Substring(3);
                        Trace.TraceInformation("Incoming connection for pipe {0}", pipeName);

                        bool pipeAllowed = noPipeConstraints;
                        if (!pipeAllowed)
                        {
                            for (int i = 0; i < allowedPipes.Count; i++)
                            {
                                if (pipeName.Equals(allowedPipes[i], StringComparison.OrdinalIgnoreCase))
                                {
                                    pipeAllowed = true;
                                    break;
                                }
                            }
                        }
                        if (!pipeAllowed)
                        {
                            Trace.TraceWarning("Incoming connection for pipe {0} not permitted", pipeName);
                            try
                            {
                                streamConnection.Stream.Close();
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Error closing stream: {0}", ex.Message);
                            }
                            return;
                        }
                    }
                    else
                    {
                        Trace.TraceError("Unable to handle connection for {0}", streamConnection.ConnectionInfo);
                        streamConnection.Stream.Close();
                        return;
                    }

                    MultiplexConnectionInputPump connectionPump =
                        new MultiplexConnectionInputPump(streamConnection.Stream.Read,
                                                         OnCreateConnection,
                                                         streamConnection);
                    connectionPump.Run(false);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error accepting connection: {0}", ex.Message);
            }

        }

        private MultiplexedConnection OnCreateConnection(int connectionId, object streamConnectionObject)
        {
            StreamConnection streamConnection = (StreamConnection)streamConnectionObject;

            if (streamConnection.ConnectionInfo.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
            {
                int port = int.Parse(streamConnection.ConnectionInfo.Substring(4));
                TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);
                tcpClient.LingerState.Enabled = true;
                tcpClient.NoDelay = true;
                tcpClient.Connect(targetHost, port);

                return new MultiplexedServiceTcpConnection(streamConnection, tcpClient, connectionId);
            }
            else if (streamConnection.ConnectionInfo.StartsWith("np:", StringComparison.OrdinalIgnoreCase))
            {
                string pipe = streamConnection.ConnectionInfo.Substring(3);

                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipe, PipeDirection.InOut, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Impersonation);
                pipeClient.Connect();
                return new MultiplexedServiceNamedPipeConnection(streamConnection, pipeClient, connectionId);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
