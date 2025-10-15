using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Services.Interfaces;

public class LogService : ILogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSessionService _userSession;

    public LogService(IServiceProvider serviceProvider, IUserSessionService userSession)
    {
        _serviceProvider = serviceProvider;
        _userSession = userSession;
    }

    public void Log(LogAction action, string? message = null)
    {
        int? userId = _userSession.CurrentUser?.UserID;
        Log(action, message, userId);
    }

    public void Log(LogAction action, string? message, int? userId)
    {
        Serilog.Log.Information("[{Action}] {Message} (UserID={UserID})", action, message ?? "-", userId ?? 0);

        if (userId.HasValue)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var logEntry = new SkyQuizApp.Models.Log
            {
                Action = action,
                UserID = userId.Value,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            db.Logs.Add(logEntry);
            db.SaveChanges();
        }
    }

    public void LogError(LogAction action, Exception ex, string? message = null)
    {
        int? userId = _userSession.CurrentUser?.UserID;
        Serilog.Log.Error(ex, "[{Action}] {Message} (UserID={UserID})", action, message ?? "-", userId ?? 0);

        if (userId.HasValue)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var logEntry = new SkyQuizApp.Models.Log
            {
                Action = action,
                UserID = userId.Value,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            db.Logs.Add(logEntry);
            db.SaveChanges();
        }
    }

    public void Log(LogAction action, string message, int userId)
    {
        Serilog.Log.Information("[{Action}] {Message} (UserID={UserID})", action, message ?? "-", userId);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var logEntry = new SkyQuizApp.Models.Log
        {
            Action = action,
            UserID = userId,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        db.Logs.Add(logEntry);
        db.SaveChanges();
    }
}