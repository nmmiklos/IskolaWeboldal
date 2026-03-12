using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VBJWeboldal.Data;
using VBJWeboldal.Models;

namespace VBJWeboldal.Filters
{
    // 1. Ezzel a címkével tudjuk kihagyni a reCAPTCHA-t és más titkos dolgokat!
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipLoggingAttribute : Attribute { }

    // 2. Maga a szűrő, ami minden oldalletöltésnél lefut
    public class ActivityLogFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Megnézzük, hogy a metóduson rajta van-e a [SkipLogging] címke
            var hasSkipAttribute = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(SkipLoggingAttribute));

            if (hasSkipAttribute)
            {
                await next(); // Ha igen, logolás nélkül továbbengedjük!
                return;
            }

            // Ha nincs kihagyva, elmentjük a logot
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = context.HttpContext.User;

            string? userId = null;
            string userDisplayName = "👤 Vendég (Guest)";

            // Ha be van jelentkezve, megszerezzük az adatait
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                userDisplayName = $"👑 {user.Identity.Name}"; // Ez általában az email címe
            }

            var request = context.HttpContext.Request;
            var actionName = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName ?? "Unknown";
            var controllerName = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName ?? "Unknown";

            var log = new ActivityLog
            {
                UserId = userId,
                UserFullName = userDisplayName,
                ActionType = $"{request.Method} {controllerName}/{actionName}",
                Url = request.Path + request.QueryString,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.Now
            };

            dbContext.ActivityLogs.Add(log);
            await dbContext.SaveChangesAsync();

            // Továbbengedjük a kérést
            await next();
        }
    }
}