using SkyQuizApp.Models;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.Services
{
    public class UserSessionService : IUserSessionService
    {
        public User? CurrentUser { get; set; }
    }
}