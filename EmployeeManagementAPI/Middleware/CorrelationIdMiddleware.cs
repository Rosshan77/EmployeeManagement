using Serilog.Context;

namespace EmployeeManagementAPI.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers.ContainsKey(CorrelationHeaderName)
                ? context.Request.Headers[CorrelationHeaderName].ToString()
                : Guid.NewGuid().ToString();

            context.Items[CorrelationHeaderName] = correlationId;

            context.Response.Headers[CorrelationHeaderName] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}