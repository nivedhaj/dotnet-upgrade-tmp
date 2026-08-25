using ct._Core.Repo.Resource;
using System.Collections.Generic;

namespace Repo.Resource
{
    public class Email : Entity
    {
        public List<string> Recipients { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public List<Attachment> Attachments { get; set; }

        public Email()
        {
            Recipients = new List<string>();
        }
    }
}