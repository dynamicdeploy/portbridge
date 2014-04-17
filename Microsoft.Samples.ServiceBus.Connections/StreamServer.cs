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
    using System.IO;
    using System.ServiceModel;

    [ServiceBehavior(
        InstanceContextMode=InstanceContextMode.PerSession, 
        ConcurrencyMode=ConcurrencyMode.Single, 
        AutomaticSessionShutdown=false)]
    public class StreamServer : IDataExchange
    {
        IDataExchange callback;
        Stream inputStream;
        Stream duplexStream;

        public StreamServer()
        {
            this.callback = null;
            this.inputStream = new ThrottledQueueBufferedStream(5);
        }

        void IDataExchange.Connect(string connectionInfo)
        {
            InstanceContext instanceContext = OperationContext.Current.InstanceContext;
            callback = OperationContext.Current.GetCallbackChannel<IDataExchange>();
            ((IServiceChannel)callback).Faulted += new EventHandler(StreamServer_Faulted);
            duplexStream = new CompositeDuplexStream(inputStream, new StreamOverWriteDelegate((buf, off, count) =>
                           {
                               try
                               {
                                   if (((IServiceChannel)callback).State == CommunicationState.Opened)
                                   {
                                       callback.Write(new TransferBuffer(buf, off, count));
                                   }
                               }
                               catch
                               {
                                   try
                                   {
                                       duplexStream.Close();
                                       if (instanceContext.State == CommunicationState.Opened)
                                       {
                                           instanceContext.ReleaseServiceInstance();
                                       }
                                   }
                                   catch
                                   {
                                       // absorb all errors here.
                                   }
                                   throw;
                               }
                           }));
            
            ((StreamServerHost)OperationContext.Current.Host).AvailableStreams.EnqueueAndDispatch(new StreamConnection(duplexStream, connectionInfo));
        }

        void StreamServer_Faulted(object sender, EventArgs e)
        {
            
        }

        [OperationBehavior(AutoDisposeParameters=false)]
        void IDataExchange.Write(TransferBuffer data)
        {
            inputStream.Write(data.Data.Array, data.Data.Offset, data.Data.Count);
        }

        void IDataExchange.Disconnect()
        {
            duplexStream.Close();
        }

     
    }
}
