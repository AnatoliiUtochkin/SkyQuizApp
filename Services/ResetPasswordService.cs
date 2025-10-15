using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.Services
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ResetPasswordService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> SendResetPasswordEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                PasswordHasher passwordHasher = new PasswordHasher();
                Random random = new Random();
                string newPassword = random.Next(1000000, 9999999).ToString();
                string hashedPassword = passwordHasher.HashPassword(newPassword);

                user.PasswordHash = hashedPassword;
                await _context.SaveChangesAsync();

                string subject = "Скидання паролю — SkyQuiz";
                string body = $@"
                    <div style='font-family:Segoe UI, sans-serif; color:#333; padding:20px;'>
                        <h2>Скидання паролю</h2>
                        <p>Ваш новий тимчасовий пароль:</p>
                        <div style='background:#f0f0f0; padding:10px 15px; border-radius:5px; font-size:18px; font-weight:bold; width:max-content;'>
                            {newPassword}
                        </div>
                        <p>Рекомендуємо увійти та змінити пароль у своєму профілі.</p>
                        <hr />
                        <p style='color:#a00;'><strong>Увага:</strong> якщо ви не запитували скидання паролю — негайно зверніться до системного адміністратора.</p>
                    </div>";

                await _emailService.SendEmailAsync(email, subject, body);
                return true;
            }

            return false;
        }
    }
}