using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeSSO.IdentityWeb.Models
{
    public class ScanUserInfo
    {
        public string UserId { get; set; }
        public string OpenId { get; set; }
        public DateTime ScanTime { get; set; }
        public string NickName { get; set; }
    }
}
