using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;

namespace AuthService.IService
{
    public interface IEmailService
    {
                public Task<ApiResponse> SendAsync(string toEmail, string subject, string html);

    }
}