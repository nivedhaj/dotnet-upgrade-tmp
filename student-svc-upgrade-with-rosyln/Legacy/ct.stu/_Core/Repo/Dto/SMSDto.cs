using System.Collections.Generic;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// The schema / shape of the class is designed keeping in mind the worker that handles the job. Elixir worker in this case.
    /// </summary>
    public class SMSJobDto
    {
        public string text { get; set; }
        public List<SMSRecipientDto> recipients { get; set; }
        public SMSJobDto()
        {
            recipients = new List<SMSRecipientDto>();
        }
    }
    public class SMSRecipientDto
    {
        public string name { get; set; }
        public string mobile { get; set; }

    }
}
