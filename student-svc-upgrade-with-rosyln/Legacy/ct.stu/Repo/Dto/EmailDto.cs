using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repo.Dto
{
    public class email_attachment
    {
        public string file_path { get; set; }
        public string mime_type { get; set; }
        public string mime_sub_type { get; set; }
        public string data { get; set; }
    }
}
