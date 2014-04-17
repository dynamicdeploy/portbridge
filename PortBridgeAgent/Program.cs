//---------------------------------------------------------------------------------
// Microsoft (R) .NET Services 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------


namespace PortBridgeAgent
{
    using System;
    using System.Configuration;
    using System.ServiceProcess;
    using Microsoft.Samples.ServiceBus.Connections;
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.ServiceBus;

    class Program
    {
        static bool runOnConsole;
        static int fromPort = -1;
        static int toPort = -1;
        static string serviceNamespace;
        static string issuerName;
        static string issuerSecret;
        static string cmdlineTargetHost;
        static bool useHybrid = false;

        static void Main(string[] args)
        {
            //Added by Tejaswi for testing
          //  ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

            PrintLogo();

            PortBridgeAgentSection settings = ConfigurationManager.GetSection("portBridgeAgent") as PortBridgeAgentSection;
            if (settings != null)
            {
                serviceNamespace = settings.ServiceNamespace;
                issuerName = settings.IssuerName;
                issuerSecret = settings.IssuerSecret;
            }

            if (!ParseCommandLine(args))
            {
                PrintUsage();
                return;
            }

            PortBridgeClientForwarderHost host = new PortBridgeClientForwarderHost();
            if (settings != null && settings.PortMappings.Count > 0)
            {
                foreach (PortMappingElement mapping in settings.PortMappings)
                {
                    List<IPRange> firewallRules = new List<IPRange>();
                    if (mapping.FirewallRules != null && mapping.FirewallRules.Count > 0)
                    {
                        foreach (FirewallRuleElement rule in mapping.FirewallRules)
                        {
                            if (!string.IsNullOrEmpty(rule.SourceRangeBegin) &&
                                 !string.IsNullOrEmpty(rule.SourceRangeEnd))
                            {
                                firewallRules.Add(new IPRange(IPAddress.Parse(rule.SourceRangeBegin), IPAddress.Parse(rule.SourceRangeEnd)));
                            }
                            else if (!string.IsNullOrEmpty(rule.Source))
                            {
                                firewallRules.Add(new IPRange(IPAddress.Parse(rule.Source)));
                            }
                        }
                    }

                    if (mapping.LocalTcpPort.HasValue)
                    {
                        if ( !string.IsNullOrEmpty(mapping.LocalPipe) ||
                             !string.IsNullOrEmpty(mapping.RemotePipe))
                        {
                            throw new ConfigurationErrorsException(string.Format("LocalTcpPort {0} defined with incompatible other settings", mapping.LocalTcpPort.Value));
                        }
                        else if (!mapping.RemoteTcpPort.HasValue)
                        {
                            throw new ConfigurationErrorsException(string.Format("LocalTcpPort {0} does not have a matching RemoteTcpPort defined", mapping.LocalTcpPort.Value));
                        }

                        host.Forwarders.Add(new TcpClientConnectionForwarder(serviceNamespace, issuerName, issuerSecret, mapping.TargetHost, mapping.LocalTcpPort.Value, mapping.RemoteTcpPort.Value, mapping.BindTo, useHybrid, firewallRules));
                    }

                    if (!string.IsNullOrEmpty(mapping.LocalPipe))
                    {
                        if ( mapping.LocalTcpPort.HasValue ||
                             mapping.RemoteTcpPort.HasValue)
                        {
                            throw new ConfigurationErrorsException(string.Format("LocalPipe {0} defined with incompatible other settings", mapping.LocalPipe));
                        }
                        else if (string.IsNullOrEmpty(mapping.RemotePipe))
                        {
                            throw new ConfigurationErrorsException(string.Format("LocalPipe {0} does not have a matching RemotePipe defined", mapping.LocalPipe));
                        }

                        host.Forwarders.Add(new NamedPipeClientConnectionForwarder(serviceNamespace, issuerName, issuerSecret, mapping.TargetHost, mapping.LocalPipe, mapping.RemotePipe, useHybrid));
                    }

                }
            }
            else
            {
                List<IPRange> firewallRules = new List<IPRange>();
                firewallRules.Add(new IPRange(IPAddress.Loopback));
                host.Forwarders.Add(new TcpClientConnectionForwarder(serviceNamespace, issuerName, issuerSecret, cmdlineTargetHost, fromPort, toPort, null, useHybrid, firewallRules));
            }

            if (!runOnConsole)
            {
                ServiceController sc = new ServiceController("PortBridgeAgentService");
                try
                {
                    var status = sc.Status;
                }
                catch (SystemException)
                {
                    runOnConsole = true;
                }
            }

            if (runOnConsole)
            {
                try
                {
                    host.Open();
                    Console.WriteLine("Press [ENTER] to exit.");
                    Console.ReadLine();
                    host.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new PortBridgeAgentService(host) 
                };
                ServiceBase.Run(ServicesToRun);
            }

        }


        private static void PrintUsage()
        {
            Console.WriteLine("Arguments (all arguments are required):");
            Console.WriteLine("\t-n <namespace> Service Namespace");
            Console.WriteLine("\t-s <key> Issuer Secret (Key)");
            Console.WriteLine("\t-m <machine> mapped host name of the machine running the PortBridge service");
            Console.WriteLine("\t-l <port> Local TCP port number to map from");
            Console.WriteLine("\t-r <port> Remote TCP port number to map to");
            Console.WriteLine("\t-h Use hybrid mode");
        }

        private static void PrintLogo()
        {
            Console.WriteLine("Port Bridge Agent\n(c) Microsoft Corporation\n\n");
        }

        private static bool ParseCommandLine(string[] args)
        {
            try
            {
                char lastOpt = default(char);

                foreach (var arg in args)
                {
                    if ((arg[0] == '-' || arg[0] == '/'))
                    {
                        if (lastOpt != default(char) || arg.Length != 2) return false;
                        lastOpt = arg[1];
                        switch (lastOpt)
                        {
                            case 'h':
                            case 'H':
                                useHybrid = true;
                                lastOpt = default(char);
                                break;
                            case 'c':
                            case 'C':
                                runOnConsole = true;
                                lastOpt = default(char);
                                break;
                        }
                        continue;
                    }

                    switch (lastOpt)
                    {
                        case 'N':
                        case 'n':
                            serviceNamespace = arg;
                            lastOpt = default(char);
                            break;
                        case 'S':
                        case 's':
                            issuerSecret = arg;
                            lastOpt = default(char);
                            break;
                        case 'M':
                        case 'm':
                            cmdlineTargetHost = arg;
                            lastOpt = default(char);
                            break;
                        case 'L':
                        case 'l':
                            fromPort = int.Parse(arg);
                            lastOpt = default(char);
                            break;
                        case 'R':
                        case 'r':
                            toPort = int.Parse(arg);
                            lastOpt = default(char);
                            break;
                    }

                }

                if (lastOpt != default(char))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}