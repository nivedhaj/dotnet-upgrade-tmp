using System;
using System.Collections.Generic;

namespace ct._Core.Repo.Resource
{
    /// <summary>
    ///     Image class. Not using Image as there is a .NET class by that name in System.Drawing namespace.
    /// </summary>
    [Serializable]
    internal class Img : Entity
    {
        public Img()
        {
            MetaData = new Dictionary<string, string>();
        }

        public CategoryEnum Category { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        public string MimeType { get; set; }
    }

    public enum CategoryEnum
    {
        Unknown = -1,
        Logo = 10,
        Profile = 20,
        DeviceInfo = 21
    }
}