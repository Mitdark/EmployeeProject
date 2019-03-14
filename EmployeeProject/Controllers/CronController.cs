using Website.Services;
using System.Web.Http;
using Umbraco.Web.WebApi;
using System;
using Newtonsoft.Json;
using System.Web;

namespace Website.Controllers
{
    public class CronController : UmbracoApiController
    {
        [HttpGet]
        public string SyncEmployees()
        {
            EmployeesService ec = new EmployeesService();

            try
            {
                return JsonConvert.SerializeObject(ec.SyncEmployees());
            }
            catch (Exception e) {
                return String.Format("Something went wrong trying to parse data from: {0}\n{1}", EmployeesService.syncUrl, e.Message);
            }
        }
    }
}