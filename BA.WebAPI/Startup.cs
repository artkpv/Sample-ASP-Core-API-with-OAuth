using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using BA.WebAPI.Model;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;


namespace BA.WebAPI
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public const string TEST_USER_ID = "TESTUSERID";

        public static class Roles 
        {
            public const string Administrator = "administrator";
            public const string Manager = "manager";
            public const string User = "user";
        }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsDevelopment())
                services.AddDbContext<BikingDbContext>(opt =>
                {
                    opt.UseInMemoryDatabase("BikingDb");
                });
            else
            {
                services.AddDbContext<BikingDbContext>(opt =>
                {
                    opt.UseSqlite(
                        Configuration.GetConnectionString("BAAPIDbContextConnection"));

                });
            }

            ConfigureServicesAuth(services);

            services.AddHttpClient();
            services.AddTransient<IBikingEntryService, BikingEntryService>();
            services.AddTransient<IWeatherAPIAdapter, WeatherAPIAdapter>();
            services.AddTransient<IWeeklyReportService, WeeklyReportService>();
            services.AddControllers();

            //
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        private static void ConfigureServicesAuth(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services
                .AddAuthentication(
                    IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    // base-address of your identityserver
                    options.Authority = "https://localhost:5002";

                    // name of the API resource
                    options.ApiName = "BAWebAPI";
                    options.RequireHttpsMetadata = false;
                    options.JwtBackChannelHandler =
                        new HttpClientHandler {
                            ServerCertificateCustomValidationCallback = delegate { return true; } 
                        };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }


            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}
