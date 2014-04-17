using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Countries.Models
{
      [DataContract]
    public class Country
    {
          [DataMember]
          public int CountryId { get; set; }

          [DataMember]
          public string CountryName { get; set; }

          [DataMember]
          public string CountryCode { get; set; }
    }
}