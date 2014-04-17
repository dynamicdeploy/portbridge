//---------------------------------------------------------------------------------
// Microsoft (R) .NET Services 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------

namespace PortBridge
{
    using System;
    using System.Configuration;
    using System.ServiceProcess;
    using Microsoft.Samples.ServiceBus.Connections;
    using Microsoft.ServiceBus;

    class Program
    {
        static bool runOnConsole;
        static bool useHybrid;
        static string serviceNamespace;
        static string issuerName;
        static string issuerSecret;
        static string permittedPorts;
        static string localHostName = Environment.MachineName;

       

        static void Main(string[] args)
        {
            //Added by Tejaswi
          //  ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

            runOnConsole = false;
            useHybrid = false;

            PrintLogo();

            PortBridgeSection settings = ConfigurationManager.GetSection("portBridge") as PortBridgeSection;
            if (settings != null)
            {
                serviceNamespace = settings.ServiceNamespace;
                issuerName = settings.IssuerName;
                issuerSecret = settings.IssuerSecret;
                if (!string.IsNullOrEmpty(settings.LocalHostName))
                {
                    localHostName = settings.LocalHostName;
                }
            }

            if (!ParseCommandLine(args))
            {
                PrintUsage();
                return;
            }

            PortBridgeServiceForwarderHost host = new PortBridgeServiceForwarderHost();
            if (settings != null && settings.HostMappings.Count > 0)
            {
                foreach (HostMappingElement hostMapping in settings.HostMappings)
                {
                    string targetHostAlias = hostMapping.TargetHost;
                    if (string.Equals(targetHostAlias, "localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        targetHostAlias = localHostName;
                    }
                    host.Forwarders.Add(new ServiceConnectionForwarder(serviceNamespace, issuerName, issuerSecret, hostMapping.TargetHost, targetHostAlias, hostMapping.AllowedPorts, hostMapping.AllowedPipes, useHybrid));
                }
            }
            else
            {
                string targetHostAlias = localHostName;
                if (string.Equals(targetHostAlias, "localhost", StringComparison.OrdinalIgnoreCase))
                {
                    targetHostAlias = localHostName;
                }
                host.Forwarders.Add(new ServiceConnectionForwarder(serviceNamespace, issuerName, issuerSecret, "localhost", targetHostAlias, permittedPorts, string.Empty, useHybrid));
            }
            

            if (!runOnConsole)
            {
                ServiceController sc = new ServiceController("PortBridgeService");
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
                host.Open();
                Console.WriteLine("Press [ENTER] to exit.");
                Console.ReadLine();
                host.Close();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new PortBridgeService(host) 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }


        private static void PrintUsage()
        {
            Console.WriteLine("Arguments:");
            Console.WriteLine("\t-n <namespace> .NET Services Service Namespace");
            Console.WriteLine("\t-s <key> .NET Services Issuer Secret (Key)");
            Console.WriteLine("\t-a <port>[,<port>[...]] or '*' Allow connections on these ports");
            Console.WriteLine("\t-h Use hybrid mode");
            Console.WriteLine("\t-c Run service in console-mode");
        }

        private static void PrintLogo()
        {
            Console.WriteLine("Port Bridge Service\n(c) Microsoft Corporation\n\n");
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
                        case 'M':
                        case 'm':
                            localHostName = arg;
                            lastOpt = default(char);
                            break;

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
                        case 'A':
                        case 'a':
                            permittedPorts = arg;
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
