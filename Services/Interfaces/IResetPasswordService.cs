namespace SkyQuizApp.Services.Interfaces
{
    public interface IResetPasswordService
    {
        Task<bool> SendResetPasswordEmail(string email);
    }
}