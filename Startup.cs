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
             services.AddTransient<IStudentsDbService, ServerDbService>();
             services.AddTransient<IDbService, MockDbService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentsDbService studentsDbService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
                // context.Request.Body.Position(0);
                await next();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
