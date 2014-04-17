using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Countries.DataAccess;
using Countries.Models;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Diagnostics;
namespace Countries.Controllers
{
    public class CountriesController : ApiController
    {
        private string DatabaseConnectionString { get; set; }

        public CountriesController()
        {
            try
            {
                DatabaseConnectionString = ConfigurationManager.ConnectionStrings["DatabaseConnectionString"].ConnectionString;

            }catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        // GET: api/Countries
        public IEnumerable<Country> Get()
        {
            return CountryService.GetCountries(DatabaseConnectionString);
        }

        // GET: api/Countries/5
        public Country Get(string id)
        {
            return CountryService.GetCountry(DatabaseConnectionString, id);
        }

     
    }
}
