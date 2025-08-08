using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dto;
using AuthService.IService;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;
        public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager; 
        }
        public async Task<IdentityResult> RegisterAsync(RequestRegister model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null) return IdentityResult.Failed(new IdentityError { Code = "DuplicatedEmail", Description = "Email has already been used" });

            if (model.Password != model.ConfirmPassword) return IdentityResult.Failed(new IdentityError { Code = "PasswordDismatch", Description = "Password and confirm password do not match" });
            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = new User()
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        FullName = model.FullName
                    };
                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded) return  createUserResult;

                    if (!await _roleManager.RoleExistsAsync(DefaultRole.User))
                    {
                        var role = await _roleManager.CreateAsync(new Role { Name = DefaultRole.User });
                        if (!role.Succeeded) return role;
                    }
                    if (!await _userManager.IsInRoleAsync(user,DefaultRole.User))
                    {
                        var userrole = await _userManager.AddToRoleAsync(user, DefaultRole.User);
                        if (!userrole.Succeeded) return userrole;
                    }

                    await transaction.CommitAsync();
                    return createUserResult;

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return IdentityResult.Failed(new IdentityError { Code = "Server Error", Description = $"Unexpected error:{ex.Message} " });
                }
                
            }
        }


    }
}