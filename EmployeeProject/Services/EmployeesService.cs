using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Website.Models;

namespace Website.Services {

    public class EmployeesService {

        public const string syncUrl = "http://dummy.restapiexample.com/api/v1/employees";

        public const string imageDownloadPath = "~/App_Data/TEMP/Import/";

        protected ApplicationContext Application { get; }

        protected IContentService ContentService => Application.Services.ContentService;

        protected IMediaService MediaService => Application.Services.MediaService;

        public EmployeesService() : this(ApplicationContext.Current) { }

        public EmployeesService(ApplicationContext application)
        {
            Application = application;
        }

        public List<EmployeeStatus> SyncEmployees()
        {
            Directory.CreateDirectory(IOHelper.MapPath(imageDownloadPath));

            List<EmployeeModel> employeeResponse = RetrieveEmployeeList(syncUrl).Take(50).ToList();

            Dictionary<int, IContent> currentEmployees = ContentService.GetChildren(UmbracoConstants.Content.Employees).ToDictionary(x => x.GetValue<int>("employeeId"));

            List<EmployeeStatus> statusList = new List<EmployeeStatus>();

            foreach (EmployeeModel employeeModel in employeeResponse)
            {
                IContent content;

                if (currentEmployees.TryGetValue(employeeModel.Id, out content))
                {
                    if (content.GetValue<string>("rawData") == JsonConvert.SerializeObject(employeeModel))
                    {
                        currentEmployees.Remove(employeeModel.Id);

                        statusList.Add(new EmployeeStatus
                        {
                            EmployeeId = employeeModel.Id,
                            Status = "NotModified"
                        });
                    }
                    else
                    {
                        currentEmployees.Remove(employeeModel.Id);

                        UpdateEmployee(statusList, employeeModel, content);

                        statusList.Add(new EmployeeStatus
                        {
                            EmployeeId = employeeModel.Id,
                            Status = "Updated"
                        });

                    }
                }
                else
                {
                    content = ContentService.CreateContent(employeeModel.EmployeeName, UmbracoConstants.Content.Employees, "employee");

                    currentEmployees.Remove(employeeModel.Id);

                    InsertNewEmployee(statusList, employeeModel, content);

                    statusList.Add(new EmployeeStatus
                    {
                        EmployeeId = employeeModel.Id,
                        Status = "New"
                    });
                }
            }

            if (currentEmployees.Any())
            {
                foreach (IContent content in currentEmployees.Values)
                {
                    DeleteEmployee(statusList, content);
                }
            }

            return statusList;
        }

        private void InsertNewEmployee(List<EmployeeStatus> statusList, EmployeeModel employeeModel, IContent content)
        {
            content.SetValue("employeeId", employeeModel.Id);
            content.SetValue("employeeName", employeeModel.EmployeeName);
            content.SetValue("salary", employeeModel.EmployeeSalary);
            content.SetValue("age", employeeModel.EmployeeAge);
            content.SetValue("rawData", JsonConvert.SerializeObject(employeeModel));

            if (!string.IsNullOrEmpty(employeeModel.ProfileImage))
            {
                string image = DownloadImage(employeeModel);

                content.SetValue("image", image);
            }

            ContentService.SaveAndPublishWithStatus(content);
        }

        private void UpdateEmployee(List<EmployeeStatus> statusList, EmployeeModel employeeModel, IContent content)
        {
            content.SetValue("employeeId", employeeModel.Id);
            content.SetValue("employeeName", employeeModel.EmployeeName);
            content.SetValue("salary", employeeModel.EmployeeSalary);
            content.SetValue("age", employeeModel.EmployeeAge);
            content.SetValue("rawData", JsonConvert.SerializeObject(employeeModel));

            if (employeeModel.ProfileImage != content.GetValue<string>("profileImage"))
            {
                string image = DownloadImage(employeeModel);
                content.SetValue("image", image);
            }

            ContentService.SaveAndPublishWithStatus(content);
        }

        private void DeleteEmployee(List<EmployeeStatus> statusList, IContent content)
        {
            ContentService.Delete(content);

            statusList.Add(new EmployeeStatus
            {
                EmployeeId = content.GetValue<int>("employeeId"),
                Status = "Deleted"
            });
        }

        public List<EmployeeModel> RetrieveEmployeeList(string url)
        {
            using (WebClient client = new WebClient())
            {
                string content = client.DownloadString(url);

                return JsonConvert.DeserializeObject<List<EmployeeModel>>(content);
            }
        }

        private string DownloadImage(EmployeeModel employeeModel)
        {
            List<string> temp = new List<string>();

            IMedia media;

            string fileName = Path.GetFileName(employeeModel.ProfileImage);
            string savePath = IOHelper.MapPath(imageDownloadPath + fileName);

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(employeeModel.ProfileImage, savePath);
            }

            using (FileStream fs = new FileStream(savePath, FileMode.Open))
            {
                media = MediaService.CreateMedia(Path.GetFileNameWithoutExtension(employeeModel.ProfileImage), UmbracoConstants.Media.Employees, "Image");
                media.SetValue("umbracoFile", fileName, fs);

                MediaService.Save(media);

                return "umb://media/" + media.Key.ToString("N");
            }
        }
    }
}