// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using BA.IServer.Data;
using BA.IServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BA.IServer
{
    public class SeedData
    {
        private const string DefaultPassword = "Pass123$";

        public static void EnsureSeedData(string connectionString)
        {
            ServiceCollection services = PrepareServices(connectionString);

            SeedUsers(services);
        }

        private static ServiceCollection PrepareServices(string connectionString) 
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlite(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            return services;
        }

        private static void SeedUsers(ServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    context.Database.Migrate();

                    UserManager<ApplicationUser> userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    AddUser(userMgr, "alice", Startup.Roles.User );
                    AddUser(userMgr, "bob", Startup.Roles.User );
                    AddUser(userMgr, "administrator", Startup.Roles.Administrator );
                    AddUser(userMgr, "manager", Startup.Roles.Manager);
                }
            }
        }

        public static void AddUser(UserManager<ApplicationUser> userMgr, string userName, string roleName)
        {
            var user = userMgr.FindByNameAsync(userName).Result;
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName
                };
                var result = userMgr.CreateAsync(user, DefaultPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(user, new Claim[]{
                        new Claim(JwtClaimTypes.Role, roleName),
                        new Claim(JwtClaimTypes.Email, userName + "@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug($"'{userName}' created");
            }
            else
            {
                Log.Debug($"'{userName}' already exists");
            }
        }
    }
}
