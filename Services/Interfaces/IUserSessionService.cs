using SkyQuizApp.Models;

namespace SkyQuizApp.Services.Interfaces
{
    public interface IUserSessionService
    {
        User? CurrentUser { get; set; }
    }
}