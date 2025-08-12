using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
   public record ResetPasswordRequested(
    Guid CorrelationId,
    string UserId,
    string Email,
    string? Token,
    string? Otp,
    DateTime ExpiresAt,
    string Locale,
    string ResetUrl
);
}