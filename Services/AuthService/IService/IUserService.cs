using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Dto;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthService.IService
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(RequestRegister model);
        Task<ApiResponse> LoginAsync(RequestLogin model);

        Task<ApiResponse> VerifyEmail(int UserId, string token);

    }
}