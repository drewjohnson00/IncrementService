using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IncrementService.Models;
using Serilog;
using IncrementService.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;

namespace IncrementService
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
            ServiceDescriptor x = services.First(a => a.ServiceType.Name == "IWebHostEnvironment");
            bool isDevelopment = ((IWebHostEnvironment) x.ImplementationInstance).IsDevelopment();

            services.AddScoped(typeof(IIncrementData), typeof(IncrementModel));
            services.AddDbContext<IncrementContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("IncrementService"));
            });
            //services.AddAuthentication();

        //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            //{
            //    options.Cookie.HttpOnly = true; // cookie is not accessible with javascript, only the browser when serving requests
            //    options.Cookie.SameSite = SameSiteMode.Lax;
            //    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            //});
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.MinimumSameSitePolicy = SameSiteMode.Strict;
            //    options.HttpOnly = HttpOnlyPolicy.None;
            //    options.Secure = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            //});
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseSerilogRequestLogging();

            if (env.IsDevelopment())
            {
                logger.LogInformation("-- Development Mode Enabled --");

                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseCookiePolicy();

            //app.UseAuthentication();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
