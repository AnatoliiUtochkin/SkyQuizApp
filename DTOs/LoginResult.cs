using SkyQuizApp.Enums;

namespace SkyQuizApp.DTOs
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public LoginFailureReason FailureReason { get; set; } = LoginFailureReason.None;

        public static LoginResult Success()
        {
            return new LoginResult { IsSuccess = true };
        }

        public static LoginResult Failure(LoginFailureReason reason)
        {
            return new LoginResult { IsSuccess = false, FailureReason = reason };
        }
    }
}