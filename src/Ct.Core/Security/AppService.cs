using ct._Core.Repo.Resource;
using ct._Core.Util;
using System.Collections.Generic;

namespace ct._Core.Security
{
    /// <summary>
    /// App Service
    /// </summary>
    public class AppService
    {
        Dictionary<string, App> app_dict;
        /// <summary>
        /// Ctor
        /// </summary>
        public AppService()
        {
            app_dict = new Dictionary<string, App>()
            {
                {"fb2a0b08-f3ba-4fc5-b0c8-6a9f260fb861", new App() { Id = "fb2a0b08-f3ba-4fc5-b0c8-6a9f260fb861", Name = "CTWEB", Status = AppStatusEnum.Active }},
                {"a5a3ae48-c281-4c83-b428-c139344a7ed6", new App() { Id = "a5a3ae48-c281-4c83-b428-c139344a7ed6", Name = "CampusTrack.API.Tests", Status = AppStatusEnum.Active }},
                {"a782fbe9-23cf-4a7b-821c-cac5ac4c151c", new App() { Id = "a782fbe9-23cf-4a7b-821c-cac5ac4c151c", Name = "CTNG", Status = AppStatusEnum.Active } },
                {"00ed8512-2b45-4853-a962-d2423ef61fb1", new App() { Id = "00ed8512-2b45-4853-a962-d2423ef61fb1", Name = "CTNG4", Status = AppStatusEnum.Active } },
                {"e2fea31f-78b0-418e-99bc-81c91733f6d8", new App() { Id = "e2fea31f-78b0-418e-99bc-81c91733f6d8", Name = "CTMobi", Status = AppStatusEnum.Active }},
                {"d1d4ac73-952e-441c-86f5-06159c70a936", new App() { Id = "d1d4ac73-952e-441c-86f5-06159c70a936", Name = "CTDash", Status = AppStatusEnum.Active }},
                {_Constants.APP_ID_ETL, new App()
                    {
                        Id = _Constants.APP_ID_ETL,
                        Name = "CampusTrack.Etl",
                        Status = AppStatusEnum.Active,
                        AppType = AppTypeEnum.System,
                        Description = "Used for ETL, Reporting & Analytics",
                        IPBound = true
                    }
                },
                {"23631b84-a0f9-4e44-be1a-9e1b81dac036", new App() { Id = "23631b84-a0f9-4e44-be1a-9e1b81dac036", Name = "Swagger", Status = AppStatusEnum.Active } },
                {_Constants.APP_ID_JOB, new App() {
                        Id = _Constants.APP_ID_JOB,
                        Name = "Job",
                        Status = AppStatusEnum.Active,
                        AppType = AppTypeEnum.System,
                        Description = "Used by Job(s) and Workflow(s)",
                        IPBound = true
                    }
                },
            };

        }
        /// <summary>
        /// App From Access Code
        /// </summary>
        /// <param name="accessCode"></param>
        /// <returns></returns>
        public App GetAppFromAccessCode(string accessCode)
        {
            if (string.IsNullOrEmpty(accessCode)) return null;
            return app_dict.ContainsKey(accessCode) ? app_dict[accessCode] : null;
        }
    }
}
