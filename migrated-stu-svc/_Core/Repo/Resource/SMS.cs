using ct._Core.Repo.Dto;
using System.Collections.Generic;

namespace ct._Core.Repo.Resource
{
    public class SMS : Entity
    {
        public SMSGatewayEnum Gateway { get; set; }
        public string GatewayResult { get; set; }
        public List<SMSRecipientDto> Recipients { get; set; }
        public string Message { get; set; }
        public string SMSRefId { get; set; }

        public SMS()
        {
            Recipients = new List<SMSRecipientDto>();
        }
    }
    public enum SMSGatewayEnum { Unknown = -1, Infini = 2 }
}
