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
    using System.ServiceModel;

    [ServiceContract(Namespace="n:", Name="idx", CallbackContract=typeof(IDataExchange), SessionMode=SessionMode.Required)]
    public interface IDataExchange
    {
        [OperationContract(Action="c", IsOneWay = true, IsInitiating=true)]
        void Connect(string i);
        [OperationContract(Action = "w", IsOneWay = true)]
        void Write(TransferBuffer d);
        [OperationContract(Action = "d", IsOneWay = true, IsTerminating = true)]
        void Disconnect();
    }

    public interface IDataExchangeChannel : IDataExchange, IClientChannel { }

}
