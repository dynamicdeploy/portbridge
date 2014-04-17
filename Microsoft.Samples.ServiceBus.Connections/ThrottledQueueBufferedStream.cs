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
    using System.Threading;
    
    public class ThrottledQueueBufferedStream : QueueBufferedStream
    {
        private Semaphore sempahore;

        public ThrottledQueueBufferedStream(int throttleCapacity)
        {
            sempahore = new Semaphore(throttleCapacity, throttleCapacity);
        }
        
        public ThrottledQueueBufferedStream(TimeSpan naglingDelay)
            : base(naglingDelay)
        {
            
        }
         

        protected override void EnqueueChunk(byte[] chunk)
        {
            sempahore.WaitOne();
            DataChunksQueue.EnqueueAndDispatch(chunk, ChunkDequeued);
        }

        void ChunkDequeued()
        {
            sempahore.Release();
        }
    }

}
