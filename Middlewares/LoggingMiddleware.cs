using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using  System.Text;
namespace cw3_apbd.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext) 
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, @"Logs\requestsLog.txt");
            using (var fileStream = new FileStream(fileName, FileMode.Append)) 
            { // Другой вариант - это использование потока HTTP, после использования позицию которого необходимо будет поставить на ноль 
                string log = httpContext.Request.Method  + ";" +
                             httpContext.Request.Path + ";";

                string httpBodyString = "";
                using (StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024))
                {
                    httpBodyString = await reader.ReadToEndAsync();
                }

                
                log +=  httpBodyString + ";" + 
                        httpContext.Request.QueryString + "\r\n";
                
                byte[] buffer = Encoding.Default.GetBytes(log); // partOfLog);
                fileStream.Write(buffer, 0, buffer.Length);
              

            }

            await _next(httpContext);
            
        }


    }
}
