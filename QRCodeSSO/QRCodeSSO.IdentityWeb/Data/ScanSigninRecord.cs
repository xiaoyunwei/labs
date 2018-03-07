using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeSSO.IdentityWeb.Data
{
    public class ScanSigninRecord
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SignInCode { get; set; }

        [Required]
        [StringLength(250)]
        public string ReturnUrl { get; set; }

        public DateTime ExpirationTime { get; set; }

        [StringLength(50)]
        public string ScanOpenId { get; set; }

        [StringLength(50)]
        public string SignInUserId { get; set; }

        public DateTime? SignInTime { get; set; }
    }
}
