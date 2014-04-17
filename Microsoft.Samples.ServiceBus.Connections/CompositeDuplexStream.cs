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

    public class CompositeDuplexStream : Stream
    {
        Stream inputStream;
        Stream outputStream;

        public CompositeDuplexStream(Stream inputStream, Stream outputStream)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
        }

        protected CompositeDuplexStream()
        {

        }

        public override void Close()
        {
            base.Close();
            this.inputStream.Close();
            this.outputStream.Close();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            try
            {
                outputStream.Flush();
            }
            catch (NotSupportedException)
            {
                // ok
            }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inputStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            outputStream.Write(buffer, offset, count);
        }
    }
}
