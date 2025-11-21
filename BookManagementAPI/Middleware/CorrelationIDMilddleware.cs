using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace BookManagementAPI.Middleware;


public class CorrelationIDMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIDMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIDMiddleware> logger)
    {
        string correlationId = Guid.NewGuid().ToString();

        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues existingCorrelationId) && !StringValues.IsNullOrEmpty(existingCorrelationId))
        {
            correlationId = existingCorrelationId.ToString();
        }
        else
        {
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationId
               }))
        {
            await _next(context);
        }
    }
}