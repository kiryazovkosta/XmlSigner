using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DigitalSignaturesClient.Middleware
{
    public class LogPostRequestsMiddleware
    {
        private readonly RequestDelegate _next;

        public LogPostRequestsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method.Equals("POST", StringComparison.CurrentCultureIgnoreCase))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                string body = await reader.ReadToEndAsync();

                context.Request.Body.Position = 0;
                Log.Information($"HttpPost: {body}");
            }
            

            await _next(context);
        }


    }
}
