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
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    [XmlRoot(Namespace = "n:")]
    public class TransferBuffer : IXmlSerializable
    {
        ArraySegment<byte> segment;

        public TransferBuffer()
        {
        }

        public TransferBuffer(byte[] buffer, int offset, int count)
        {
            segment = new ArraySegment<byte>(buffer, offset, count);
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.Read();

            List<ArraySegment<byte>> data = new List<ArraySegment<byte>>();
            for (; ; )
            {
                byte[] buffer = new byte[65536];
                int bytesRead = reader.ReadContentAsBase64(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    data.Add(new ArraySegment<byte>(buffer, 0, bytesRead));
                }
                else
                {
                    if (data.Count == 0)
                    {
                        segment = new ArraySegment<byte>();
                    }
                    else if (data.Count == 1)
                    {
                        segment = data[0];
                    }
                    else if (data.Count > 1)
                    {
                        int totalBytes = data.Sum((seg) => seg.Count);
                        byte[] joinedBuffer = new byte[totalBytes];
                        int offset=0;
                        foreach( var seg in data )
                        {
                            Buffer.BlockCopy(seg.Array, seg.Offset, joinedBuffer, offset, seg.Count); 
                            offset += seg.Count; 
                        }
                        segment = new ArraySegment<byte>(joinedBuffer, 0, totalBytes);
                    }
                    return;
                }
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteBase64(segment.Array, segment.Offset, segment.Count);
            writer.Flush();
        }

        public ArraySegment<byte> Data
        {
            get
            {
                return segment;
            }
        }
    }
}
