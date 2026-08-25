using System.Collections.Generic;

namespace ct._Core.Repo.Resource
{
    public class App
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// List of UX routes that is managed by the App
        /// </summary>
        public List<string> Routes { get; set; }
        /// <summary>
        /// Requests can be originated from specific IP only. Defaults to false.
        /// </summary>
        public bool IPBound { get; set; }
        public AppStatusEnum Status { get; set; }
        public AppTypeEnum AppType { get; set; }
        public App()
        {
            AppType = AppTypeEnum.User;
            Routes = new List<string>();
        }
    }

    public enum AppStatusEnum
    {
        Unknown = -1, Active = 1, Disabled = 99
    }
    /// <summary>
    /// Type of app that is making request. User => Using a user interface, System => Programmatic test / dsl / worker process etc.
    /// </summary>
    public enum AppTypeEnum
    {
        User = 1, System = 2
    }
}
