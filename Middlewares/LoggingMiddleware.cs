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


        /*
         streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
             
             
         */
         /*
        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            // var request

            string fileName = Path.Combine(Environment.CurrentDirectory, @"Logs\requestsLog.txt");
            using (var fileStream = new FileStream(fileName, FileMode.Append))
            { // Другой вариант - это использование потока HTTP, после использования позицию которого необходимо будет поставить на ноль 
                string log = httpContext.Request.Method + ";" +
                             httpContext.Request.Path + ";";

                string httpBodyString = "";

                // using (var memoryStream = new MemoryStream()) {
                    // httpContext.Request.Body.CopyTo(memoryStream);
                    using (StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024))
                    {
                        httpBodyString = await reader.ReadToEndAsync();
                    // reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    httpContext.Request.Body.Position = 0;
                }

                // httpContext.Request.Body = new StreamReader(httpBodyString); // Encoding.UTF8.GetBytes(httpBodyString));
                //}
                /*
                using (var saveStreamWiriter = new StreamWriter(httpContext.Request.Body))
                {
                    saveStreamWiriter.Write(httpBodyString);
                }
                *_/

                // httpContext.Request.Body.Position = 0;

                log += httpBodyString + ";" +
                        httpContext.Request.QueryString + "\r\n";

                byte[] buffer = Encoding.Default.GetBytes(log); // partOfLog);
                fileStream.Write(buffer, 0, buffer.Length);


            }
            // httpContext.Request.Body.Position = 0;
                
            await _next(httpContext);
            // await _next(new HttpContext(new HttpRequest(), httpContext.Response)));
        }

        */
        public async Task InvokeAsync(HttpContext httpContext)
        {
            // var request
            httpContext.Request.EnableBuffering();
            string fileName = Path.Combine(Environment.CurrentDirectory, @"Logs\requestsLog.txt");
            using (var fileStream = new FileStream(fileName, FileMode.Append))
            { // Другой вариант - это использование потока HTTP, после использования позицию которого необходимо будет поставить на ноль 
                string log = httpContext.Request.Method + ";" +
                             httpContext.Request.Path + ";";

                string httpBodyString = "";
                //using (
                StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024); // )
                //{
                    httpBodyString = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                //}
                
                // httpContext.Request.Body.Position = 0;

                log += httpBodyString + ";" +
                        httpContext.Request.QueryString + "\r\n";

                byte[] buffer = Encoding.Default.GetBytes(log); // partOfLog);
                fileStream.Write(buffer, 0, buffer.Length);


            }
            // httpContext.Request.Body.Position = 0;
            await _next(httpContext);

        }

    }
}
