using Serilog;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.Services
{
    public class LoggerConfig : ILoggerConfig
    {
        public void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}