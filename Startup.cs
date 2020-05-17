using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using cw3_apbd.DAL;
using cw3_apbd.Controllers;
using cw3_apbd.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using cw3_apbd.Middlewares;
using Microsoft.AspNetCore.Authentication;
using cw3_apbd.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using cw3_apbd.Models_2;
namespace cw3_apbd
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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer( options => 
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        // ValidAudience = "Students",
                        ValidAudience = "Employee",
                        ValidIssuer = "CORP",
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                
                });
            /*
            services.AddAuthentication("AuthenticationBasic")
                    .AddScheme<AuthenticationSchemeOptions, BasicAuthorizationHandler>
                    ("AuthenticationBasic", null);
            */
             services.AddTransient<IStudentsDbService, ServerDbService>();
             services.AddTransient<IDbService, MockDbService>();

             
            services.AddScoped<IDbServicesCwieczenie10, EfDbServicesCwieczenie10>();
            services.AddDbContext<s19314Context>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentsDbService studentsDbService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<LoggingMiddleware>();
            
            
            /* Проверка на содержания в запросе индекса. Есть подозрение, что один из моих middlwar'ow 
             * просто не передает Body
             */ 

            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Index") )
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Nie podałeś indeksu");
                    return;
                }


                /*
                string httpBodyString = "";
            
                using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024))
                {
                    httpBodyString = await reader.ReadToEndAsync();
                }
                */
                string studentIndex = context.Request.Headers["Index"].ToString();
                if (!studentsDbService.isExistStudies(studentIndex))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Nie istnieje takiego studenta");

                    return;
                }
                // context.Request.Body.Position = 0; //(0);
                await next();
            });
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
