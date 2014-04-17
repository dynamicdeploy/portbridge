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

    public class FirewallRuleElement : ConfigurationElement
    {
        internal const string sourceRangeBeginString = "sourceRangeBegin";
        internal const string sourceRangeEndString = "sourceRangeEnd";
        internal const string sourceString = "source";
        
        public FirewallRuleElement(string sourceRangeBegin, string sourceRangeEnd)
        {
            this.SourceRangeBegin = sourceRangeBegin;
            this.SourceRangeEnd = sourceRangeEnd;
        }

        public FirewallRuleElement(string source)
        {
            this.Source = source;
        }

        public FirewallRuleElement()
        {
        }

        [ConfigurationProperty(sourceRangeBeginString, IsRequired = false)]
        [RegexStringValidator(@"^$|^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$")]
        public string SourceRangeBegin
        {
            get
            {
                return (string)this[sourceRangeBeginString];
            }
            set
            {
                this[sourceRangeBeginString] = value;
            }
        }


        [ConfigurationProperty(sourceRangeEndString, IsRequired = false)]
        [RegexStringValidator(@"^$|^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$")]
        public string SourceRangeEnd
        {
            get
            {
                return (string)this[sourceRangeEndString];
            }
            set
            {
                this[sourceRangeEndString] = value;
            }
        }

        [ConfigurationProperty(sourceString, IsRequired = false)]
        [RegexStringValidator(@"^$|^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$")]
        public string Source
        {
            get
            {
                return (string)this[sourceString];
            }
            set
            {
                this[sourceString] = value;
            }
        }
    }
}
