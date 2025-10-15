namespace SkyQuizApp.Services.Interfaces
{
    public interface ITwoFactorService
    {
        Task<string> GenerateAndSendCodeAsync(int userId, string email);

        Task<bool> VerifyCodeAsync(int userId, string code);
    }
}