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

    public class PortMappingCollection : ConfigurationElementCollection
    {
        public PortMappingCollection()
        {
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PortMappingElement();
        }

        public PortMappingElement this[int index]
        {
            get
            {
                return (PortMappingElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public int IndexOf(PortMappingElement PortMapping)
        {
            return BaseIndexOf(PortMapping);
        }

        public void Add(PortMappingElement PortMapping)
        {
            BaseAdd(PortMapping);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(PortMappingElement PortMapping)
        {
            if (BaseIndexOf(PortMapping) >= 0)
            {
                BaseRemove(PortMapping);
            }
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string targetHost)
        {
            BaseRemove(targetHost);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override string ElementName
        {
            get
            {
                return "port";
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }
    }
}
