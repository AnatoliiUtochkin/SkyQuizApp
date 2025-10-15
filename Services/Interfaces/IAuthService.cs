using SkyQuizApp.DTOs;
using SkyQuizApp.Models;

namespace SkyQuizApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> IsSuccessfulLogin(string email, string password);

        Task<User?> GetUserByEmailAsync(string email);
    }
}