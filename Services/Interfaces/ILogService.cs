using SkyQuizApp.Enums;

namespace SkyQuizApp.Services.Interfaces
{
    public interface ILogService
    {
        void Log(LogAction action, string? message = null);

        void Log(LogAction action, string message, int userId);

        void LogError(LogAction action, Exception ex, string? message = null);
    }
}