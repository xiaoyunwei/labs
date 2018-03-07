using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeSSO.IdentityWeb.Models
{
    public class WeixinSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
    }
}
