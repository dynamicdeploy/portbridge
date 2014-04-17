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

    public abstract class Pump : IDisposable
    {
        bool disposed;
        bool running;
        PumpAsyncResult asyncResult;

        ~Pump()
        {
            Dispose(false);
        }

        internal PumpAsyncResult Caller
        {
            get { return asyncResult; }
            set { asyncResult = value; }
        }

        internal bool IsRunning
        {
            get { return running; }
            set { running = value; }
        }

        protected bool IsClosed
        {
            get { return disposed; }
        }
    
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
        }

        public abstract IAsyncResult BeginRunPump(AsyncCallback callback, object state);
        
        public static void EndRunPump(IAsyncResult asyncResult)
        {
            PumpAsyncResult.End(asyncResult);
        }

        
        public void RunPump()
        {
            Pump.EndRunPump(this.BeginRunPump(null, null));
        }

        protected void SetComplete()
        {
            this.Caller.SetComplete();
            this.Dispose();
        }

        protected void SetComplete(Exception ex)
        {
             this.Caller.SetComplete(ex);
             this.Dispose();
        }

              
        internal class PumpAsyncResult : AsyncResult
        {
            public PumpAsyncResult(AsyncCallback callback, object state)
                :base(callback, state)
            {

            }

            internal static void End(IAsyncResult asyncResult)
            {
                AsyncResult.End<PumpAsyncResult>(asyncResult);
            }

            internal void SetComplete()
            {
                base.Complete(false);
            }

            internal void SetComplete(Exception ex)
            {
                base.Complete(false, ex);
            }
        }
                
    }

    
}
