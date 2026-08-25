using System.Collections.Generic;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// The schema / shape of the class is designed keeping in mind the worker that handles the job. Elixir worker in this case.
    /// </summary>
    public class EmailJobDto
    {
        public string subject { get; set; }
        public string from { get; set; }
        public List<string> to { get; set; }
        public string data { get; set; }
        public string html { get; set; }
        public string text { get; set; }
        public string sender_id { get; set; }
        public string inst_id { get; set; }
        public string inst_short_name { get; set; }
        public List<EmailAttachmentDto> attachments { get; set; }
        public string topic { get; set; }
        public EmailJobDto()
        {
            attachments = new List<EmailAttachmentDto>();
        }
    }
    public class EmailAttachmentDto
    {
        public string file_path { get; set; }
        public string mime_type { get; set; }
        public string mime_sub_type { get; set; }
        public string data { get; set; }
    }
}
