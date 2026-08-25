using System.Collections.Generic;

namespace ct._Core.Repo.Dto
{
    public class NotificationJobDto
    {
        public string title { get; set; } //referred to as headings by onesignal
        public string content { get; set; } //notification content
        public List<string> player_ids { get; set; } //User(s) to whom the notification is to be sent
        public NotificationJobDto()
        {
            player_ids = new List<string>();
        }
    }
}
