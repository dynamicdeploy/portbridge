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
    using System.ServiceProcess;

    partial class PortBridgeService : ServiceBase
    {
        PortBridgeServiceForwarderHost host;

        public PortBridgeService(PortBridgeServiceForwarderHost host)
        {
            this.host = host;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            host.Open();
        }

        protected override void OnStop()
        {
            host.Close();
        }
    }
}
