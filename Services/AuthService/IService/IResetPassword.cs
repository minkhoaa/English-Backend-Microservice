using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Dto;

namespace AuthService.IService
{
    public interface IResetPassword
    {
        Task SaveAsync(ResetPasswordDto req);
        Task<ResetPasswordDto?> GetByUserIdAsync(int userId);
        Task<ResetPasswordDto?> GetByResetIdAsync(Guid resetId);
        Task MarkUsedAsync(Guid resetId);
    }
}