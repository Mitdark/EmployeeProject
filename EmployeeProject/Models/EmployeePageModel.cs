using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Website.Services;

namespace Website.Models
{
    public class EmployeePageModel : RenderModel
    {
        protected ApplicationContext Application { get; }

        protected IContentService ContentService => Application.Services.ContentService;

        public EmployeePageModel(ApplicationContext application, IPublishedContent content) : base(content)
        {
            Application = application;
        }

        public IEnumerable<IContent> EmployeeList
        {
            get
            {
                HttpRequest hr = HttpContext.Current.Request;

                string query = hr.QueryString["query"];
                int sort = Convert.ToInt32(hr.QueryString["sort"]);

                IEnumerable<IContent> employeeList = Application.Services.ContentService.GetChildren(UmbracoConstants.Content.Employees);

                if (query != null && query.Trim().Length > 0)
                {
                    employeeList = employeeList.Where(x => x.GetValue<string>("employeeName").ToLower().Contains(query.ToLower()));
                }

                if (sort > 0)
                {
                    if (sort == 1)
                    {
                        employeeList = employeeList.OrderBy(x => x.GetValue<string>("employeeName"));
                    }
                    else if (sort == 2)
                    {
                        employeeList = employeeList.OrderBy(x => x.GetValue<string>("age"));
                    }
                }

                return employeeList;
            }
        }

        public string IsSelected(string toCheck, string value)
        {
            HttpRequest hr = HttpContext.Current.Request;

            string queryString = hr.QueryString[toCheck];

            return queryString != null & queryString == value ? " selected" : "";
        }
    }
}