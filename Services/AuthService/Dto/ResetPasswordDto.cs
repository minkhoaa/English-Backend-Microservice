using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Dto
{
    public class ResetPasswordDto
    {   
        public Guid ResetId { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = default!;
        public string TokenHash { get; set; } = default!;
        public string OtpHash { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
    }
}