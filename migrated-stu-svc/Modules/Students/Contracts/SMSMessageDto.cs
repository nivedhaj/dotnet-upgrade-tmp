using System;
using System.Collections.Generic;

namespace Repo.Dto
{
    public class SmsMessageDto
    {
        public string Message { get; set; }
        public int Length { get { return string.IsNullOrEmpty(Message) ? 0 : Message.Length; } }
        public int Credits
        {
            get
            {
                return GetSmsCredits(Length);
            }
        }
        internal int GetSmsCredits(int sms_len)
        {
            var len = sms_len <= 160 ? (sms_len - 7) / 153.0m : sms_len / 153.0m;
            var credits = (int)Math.Ceiling(len);
            return credits <= 0 ? 1 : credits;
        }
    }
}