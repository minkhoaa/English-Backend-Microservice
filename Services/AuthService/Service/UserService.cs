using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dto;
using AuthService.IService;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        private readonly JwtSettings _jwtSettings;
        public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value; 
        }

        public async Task<ApiResponse> LoginAsync(RequestLogin model)
        {
            if (model.Email == null || model.Password == null)
            {
                return new ApiResponse() { IsSucceed = false, Message = "Failed", Data = IdentityResult.Failed(new IdentityError
                {
                    Code = "Email or password missing",
                    Description = "Please enter your email and passwword"
                })
                };
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || await _userManager.CheckPasswordAsync(user, model.Password))
                {
                return new ApiResponse()
                {
                    IsSucceed = false,
                    Message = "Failed",
                    Data = IdentityResult.Failed(new IdentityError
                    {
                        Code = "IncorrectInformation",
                        Description = "Email or password is incorrect"
                    })
                };
                }
            var claims = await _userManager.GetClaimsAsync(user);
            var secretKey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var creds = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                    issuer: _jwtSettings.ValidIssuer,
                    audience: _jwtSettings.ValidAudience,
                    claims: claims,
                    signingCredentials: creds,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireTime)

            );
            return new ApiResponse() {
                IsSucceed = true,
                Message = "Login successfully",
                Data = new JwtSecurityTokenHandler().WriteToken(token).ToString()
            }; 
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
                    if (!createUserResult.Succeeded) return createUserResult;

                    if (!await _roleManager.RoleExistsAsync(DefaultRole.User))
                    {
                        var role = await _roleManager.CreateAsync(new Role { Name = DefaultRole.User });
                        if (!role.Succeeded) return role;
                    }
                    if (!await _userManager.IsInRoleAsync(user, DefaultRole.User))
                    {
                        var userrole = await _userManager.AddToRoleAsync(user, DefaultRole.User);
                        if (!userrole.Succeeded) return userrole;
                    }

                    var claims = new List<Claim>()
                    {
                        new(ClaimTypes.Name, user.FullName),
                        new(ClaimTypes.Email, user.Email),
                        new(ClaimTypes.Role, DefaultRole.User),
                    };
                    foreach (var claim in claims)
                    {
                        await _userManager.AddClaimAsync(user, claim);
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