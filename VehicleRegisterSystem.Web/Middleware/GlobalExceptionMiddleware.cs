using System.Text.Json;

namespace VehicleRegisterSystem.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        { _next = next; _logger = logger; }

        public async Task Invoke(HttpContext ctx)
        {
            try { await _next(ctx); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "application/json";
                var res = JsonSerializer.Serialize(new { error = "Server error" });
                await ctx.Response.WriteAsync(res);
            }
        }
    }
}
