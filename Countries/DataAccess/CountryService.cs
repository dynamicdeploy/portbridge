using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Countries.Models;
using System.Data;
using System.Data.SqlClient;

namespace Countries.DataAccess
{
    public class CountryService
    {
        public static IEnumerable<Country> GetCountries(string connectionString)
        {
            IList<Country> countries = new List<Country>();
            using (IDataReader reader = SqlHelper.ExecuteReader(connectionString, System.Data.CommandType.Text, "SELECT CountryId, CountryName, CountryCode FROM Country"))
           {
               while(reader.Read())
               {

                   Country c = new Country()
                   {
                       CountryId = Convert.ToInt32(reader["CountryId"]),
                       CountryCode = Convert.ToString(reader["CountryCode"]),
                       CountryName = Convert.ToString(reader["CountryName"]),
                   };

                   countries.Add(c);
               }

           }

           return countries;
        }

        public static Country GetCountry(string connectionString, string countryCode)
        {
            Country c = new Country();
            using (IDataReader reader = SqlHelper.ExecuteReader(connectionString, System.Data.CommandType.Text, "SELECT CountryId, CountryName, CountryCode FROM Country WHERE CountryCode=@countryCode",
                new SqlParameter("@countryCode", countryCode)))
            {
                if (reader.Read())
                {

                    c.CountryId = Convert.ToInt32(reader["CountryId"]);
                    c.CountryCode = Convert.ToString(reader["CountryCode"]);
                    c.CountryName = Convert.ToString(reader["CountryName"]);

                }

            }

            return c;
        }
    }
}