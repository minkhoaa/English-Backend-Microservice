using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public record SendVerificationEmail(int UserId, string email, string subject, string verificationUrl);
}