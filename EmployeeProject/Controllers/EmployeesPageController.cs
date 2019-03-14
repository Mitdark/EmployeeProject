using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Models;
using Website.Models;

namespace Website.Controllers
{
    public class EmployeesPageController : Umbraco.Web.Mvc.RenderMvcController
    {
        public override ActionResult Index(RenderModel model)
        {
            var homeModel = new EmployeePageModel(ApplicationContext.Current, CurrentPage);

            return CurrentTemplate(homeModel);
        }
    }
}