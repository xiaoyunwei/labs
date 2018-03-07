using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeSSO.IdentityWeb.Data
{
    public class WeChatUser
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string UserId { get; set; }

        [StringLength(50)]
        public string OpenId { get; set; }

        [StringLength(100)]
        public string NickName { get; set; }
    }
}
