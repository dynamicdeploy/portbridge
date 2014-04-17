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
    using System.Threading;

    public class QueueBufferedStream : Stream 
    {
        private InputQueue<byte[]> dataChunks;
        private byte[] currentChunk;
        private int currentChunkPosition;
        private volatile bool isStreamAtEnd;
        private ManualResetEvent done;
        private TimeSpan naglingDelay;


        public QueueBufferedStream()
            :this(TimeSpan.Zero)
        {
        }

        public QueueBufferedStream(TimeSpan naglingDelay)
        {
            this.naglingDelay = naglingDelay;
            this.done = new ManualResetEvent(false);
            this.dataChunks = new InputQueue<byte[]>();
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return !isStreamAtEnd; } }

        public override void Flush() { }
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (count == 0)
            {
                return 0;
            }

            bool waitForChunk = true;
            int bytesRead = 0;

            while (true)
            {
                if (currentChunk == null)
                {
                    if (isStreamAtEnd) return 0;
                    if (waitForChunk)
                    {
                        IAsyncResult dequeueAsyncResult = dataChunks.BeginDequeue(TimeSpan.MaxValue, null, null);
                        if (!dequeueAsyncResult.CompletedSynchronously &&
                             WaitHandle.WaitAny(new WaitHandle[] { dequeueAsyncResult.AsyncWaitHandle, done }) == 1)
                        {
                            return 0;
                        }
                        currentChunk = dataChunks.EndDequeue(dequeueAsyncResult);
                        waitForChunk = false;
                    }
                    else
                    {
                        if (!dataChunks.Dequeue(naglingDelay, out currentChunk))
                        {
                            return bytesRead;
                        }
                    }
                    currentChunkPosition = 0;
                }
                else
                {
                    waitForChunk = false;
                }

                int bytesAvailable = currentChunk.Length - currentChunkPosition;
                int bytesToCopy;
                if (bytesAvailable > count)
                {
                    bytesToCopy = count;
                    Buffer.BlockCopy(currentChunk, currentChunkPosition,
                        buffer, offset, count);
                    currentChunkPosition += count;
                    return bytesRead + bytesToCopy;
                }
                else
                {
                    bytesToCopy = bytesAvailable;
                    Buffer.BlockCopy(currentChunk, currentChunkPosition,
                        buffer, offset, bytesToCopy);
                    currentChunk = null;
                    currentChunkPosition = 0;
                    bytesRead += bytesToCopy;
                    offset += bytesToCopy;
                    count -= bytesToCopy;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (count == 0) return;

            byte[] chunk = new byte[count];
            Buffer.BlockCopy(buffer, offset, chunk, 0, count);

            if (isStreamAtEnd)
            {
                throw new InvalidOperationException("EOF");
            }
            EnqueueChunk(chunk);
        }

        protected InputQueue<byte[]> DataChunksQueue
        {
            get
            {
                return dataChunks;
            }
        }
        
        protected virtual void EnqueueChunk(byte[] chunk)
        {
            dataChunks.EnqueueAndDispatch(chunk);
        }

        public void SetEndOfStream()
        {
            isStreamAtEnd = true;
            done.Set();
        }

        public override void Close()
        {
            SetEndOfStream();
            dataChunks.Close();
            base.Close();
        }
    }
}
