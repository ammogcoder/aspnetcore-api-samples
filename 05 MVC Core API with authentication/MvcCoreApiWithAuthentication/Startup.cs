﻿using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvcCoreApiWithAuthentication.Models;

namespace MvcCoreApiWithAuthentication
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var readPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).
                RequireAuthenticatedUser().
                RequireClaim("scope", "read").Build();

            var writePolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).
                RequireAuthenticatedUser().
                RequireClaim("scope", "write").Build();

            services.AddSingleton<IContactRepository, InMemoryContactRepository>();
            services.AddMvcCore().AddDataAnnotations().
                AddJsonFormatters();

            // set up embedded identity server
            services.AddIdentityServer().
                AddTestClients().
                AddTestResources().
                AddDeveloperSigningCredential();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, o =>
                {
                    o.Authority = "http://localhost:5000/openid";
                    o.RequireHttpsMetadata = false;
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Map("/openid", id => {
                // use embedded identity server to issue tokens
                id.UseIdentityServer();
            });

            app.Map("/api", api => {
                // consume the JWT tokens in the API
                api.UseAuthentication();
                api.UseMvc();
            });
        }
    }
}
