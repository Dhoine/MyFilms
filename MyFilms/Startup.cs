using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyFilms.Data;
using MyFilms.Models;
using MyFilms.Services;

namespace MyFilms
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.LoginPath = "/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
            });
            services.AddAuthentication().AddTwitter(twitterOptions =>
            {
                twitterOptions.ConsumerKey = Configuration["TwitterID"];
                twitterOptions.ConsumerSecret = Configuration["TwitterSecret"];
                twitterOptions.Events = new Microsoft.AspNetCore.Authentication.Twitter.TwitterEvents
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/Account/Login");
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["FacebookID"];
                facebookOptions.AppSecret = Configuration["FacebookSecret"];
                facebookOptions.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/Account/Login");
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddAuthentication().AddVK(vkOptions =>
            {
                vkOptions.ClientId = Configuration["VkID"];
                vkOptions.ClientSecret = Configuration["VkSecret"];
                vkOptions.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/Account/Login");
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
                vkOptions.Scope.Add("email");

                // Add fields https://vk.com/dev/objects/user
                vkOptions.Fields.Add("uid");
                vkOptions.Fields.Add("first_name");
                vkOptions.Fields.Add("last_name");

                // In this case email will return in OAuthTokenResponse, 
                // but all scope values will be merged with user response
                // so we can claim it as field
                vkOptions.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                vkOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                vkOptions.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
                vkOptions.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
            });
            services.AddSingleton<IHelper, Helper>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
