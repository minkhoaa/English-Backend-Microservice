using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int, UserClaim, UserRoles, UserLogin, RoleClaim, UserToken>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) : base(dbContext) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("user");
            builder.Entity<Role>().ToTable("role");
            builder.Entity<UserRoles>().ToTable("userrole");
            builder.Entity<UserClaim>().ToTable("userclaim");
            builder.Entity<UserToken>().ToTable("usertoken");
            builder.Entity<UserLogin>().ToTable("userlogin");

            builder.Entity<RoleClaim>().ToTable("roleclaim");



            builder.Entity<UserRoles>(option =>
            {
                option.HasKey(x => new { x.UserId, x.RoleId });
                option.HasOne(x => x.role).WithMany(x => x.userRoles).HasForeignKey(x => x.RoleId).IsRequired();
                option.HasOne(x => x.user).WithMany(x => x.userRoles).HasForeignKey(x => x.UserId).IsRequired();
            });


            builder.Entity<UserClaim>(option =>
            {
                option.HasOne(x => x.user).WithMany(x => x.userClaims).HasForeignKey(x => x.UserId).IsRequired();
            });
            builder.Entity<UserToken>(option =>
            {
                option.HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
                option.HasOne(x => x.user).WithMany(x => x.userTokens).HasForeignKey(x => x.UserId).IsRequired();
            });
            builder.Entity<UserLogin>(option =>
            {
                option.HasKey(x => new { x.LoginProvider, x.ProviderKey }); 
                option.HasOne(x => x.user).WithMany(x => x.userLogins).HasForeignKey(x => x.UserId).IsRequired();
            });
            builder.Entity<RoleClaim>().HasOne(x => x.role).WithMany(x => x.roleClaims).HasForeignKey(x => x.RoleId).IsRequired(); 

            
        }
    }
    }
