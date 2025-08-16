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
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IPublishEndpoint _bus;
        private readonly AppUrlOptions _appUrlOptions;
        public UserService(UserManager<User> userManager,
         RoleManager<Role> roleManager,
          ApplicationDbContext context,
           IOptions<JwtSettings> jwtSettings,
           IPublishEndpoint bus,
           IOptions<AppUrlOptions> options

           )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _bus = bus;
            _appUrlOptions = options.Value;

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
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
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
            if (!user.EmailConfirmed)
            {
                return new ApiResponse()
                {
                    IsSucceed = false,
                    Message = "Email is not verified"
                };
            }
            var role = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.Email!),
                    new(JwtRegisteredClaimNames.Name, user.FullName ?? user.UserName ?? user.Email!),
                    new("email_verified", user.EmailConfirmed ? "true" : "false"),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                };
            claims.AddRange(role.Select(a => new Claim("Roles", a)));
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
                    var createUserResult = await _userManager.CreateAsync(user, model.Password);
                    if (!createUserResult.Succeeded) return createUserResult;


                    var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
                    var verifyUrl = $"{_appUrlOptions.VerificationCallbackBase}?userId={user.Id}&token={tokenEncoded}";

                    var msg = new EmailSendRequested(
                    To: user.Email!,
                    Subject: "Verify your account",
                    Template: "verify-email",
                    Variables: new Dictionary<string, object>
                    {
                        ["verifyUrl"] = verifyUrl,
                        ["fullName"] = user.FullName ?? ""
                    }
                );


                    await transaction.CommitAsync();
                    await _bus.Publish(msg, ctx =>
                    {
                        ctx.SetRoutingKey("email.send");
                    });
                    return IdentityResult.Success;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return IdentityResult.Failed(new IdentityError { Code = "Server Error", Description = $"Unexpected error:{ex.Message} " });
                }
                
            }
        }

        public async Task<ApiResponse> VerifyEmail(int UserId, string token)
        {
            var user = await _userManager.FindByIdAsync(UserId.ToString());
            if (user == null) return new ApiResponse() { IsSucceed = false, Message = "User is not existed" };
            var rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, rawToken);
            if (!result.Succeeded) return new ApiResponse() { IsSucceed = false, Message = "Invalid or expire token " };

            if (!await _roleManager.RoleExistsAsync(DefaultRole.User))
            {
                var role = await _roleManager.CreateAsync(new Role { Name = DefaultRole.User });
            }
            if (!await _userManager.IsInRoleAsync(user, DefaultRole.User))
            {
                var userrole = await _userManager.AddToRoleAsync(user, DefaultRole.User);
            }
            return new ApiResponse() { IsSucceed = true, Message = "Email is verified" };

        } 
    }
}