using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.DTOs;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IPasswordHasher passwordHasher, IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        public async Task<LoginResult> IsSuccessfulLogin(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    FailureReason = LoginFailureReason.UserNotFound
                };
            }

            if (user.IsBlocked)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    FailureReason = LoginFailureReason.UserBlocked
                };
            }

            bool isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHash, password);

            if (!isPasswordValid)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    FailureReason = LoginFailureReason.InvalidPassword
                };
            }

            return new LoginResult
            {
                IsSuccess = true,
                FailureReason = LoginFailureReason.None
            };
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}