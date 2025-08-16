using System.Data.Common;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using AuthService.Data;
using AuthService.IService;
using AuthService.Models;
using AuthService.Service;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Contracts;
using System.IdentityModel.Tokens.Jwt;
var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers(); 

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseNpgsql(Environment.GetEnvironmentVariable("DEFAULT_CONNECTIONSTRING") ?? builder.Configuration.GetConnectionString("DefaultConnectionString"));

});

builder.Services.AddIdentity<User, Role>(option =>
{
    option.Password.RequireDigit = false;
    option.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton<JwtSettings>();

builder.Services.Configure<AppUrlOptions>(builder.Configuration.GetSection("AppUrls:VerificationCallbackBase"));



builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        var uri = Environment.GetEnvironmentVariable("Rabbit__Uri");
        if (string.IsNullOrEmpty(uri)) throw new Exception("Rabbit Uri is missing");
        cfg.Host(new Uri(uri));
        cfg.Message<EmailSendRequested>(m => m.SetEntityName("mail.commands"));
        cfg.Publish<EmailSendRequested>(p => p.ExchangeType = "topic");
    });
});
builder.Services.Configure<IdentityOptions>(option =>
{
    option.SignIn.RequireConfirmedEmail = true;
});
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwtSettings!.ValidIssuer,
        ValidAudience = jwtSettings.ValidAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.UTF8.GetBytes(jwtSettings!.SecretKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "Roles",
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
} );




var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthentication();


app.MapControllers(); 


app.UseHttpsRedirection();
app.Run();

