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

    public class FirewallRuleCollection : ConfigurationElementCollection
    {
        public FirewallRuleCollection()
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
            return new FirewallRuleElement();
        }

        public FirewallRuleElement this[int index]
        {
            get
            {
                return (FirewallRuleElement)BaseGet(index);
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

        public int IndexOf(FirewallRuleElement FirewallRule)
        {
            return BaseIndexOf(FirewallRule);
        }

        public void Add(FirewallRuleElement FirewallRule)
        {
            BaseAdd(FirewallRule);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(FirewallRuleElement FirewallRule)
        {
            if (BaseIndexOf(FirewallRule) >= 0)
            {
                BaseRemove(FirewallRule);
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
                return "rule";
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }
    }
}
