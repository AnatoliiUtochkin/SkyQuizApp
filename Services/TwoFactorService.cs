using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.Services.Interfaces
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public TwoFactorService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> GenerateAndSendCodeAsync(int userId, string email)
        {
            var oldCodes = await _context.TwoFactorCodes
                .Where(c => c.UserID == userId)
                .ToListAsync();

            _context.TwoFactorCodes.RemoveRange(oldCodes);
            await _context.SaveChangesAsync();

            var code = GenerateCode();

            var twoFactorCode = new TwoFactorCode
            {
                UserID = userId,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            _context.TwoFactorCodes.Add(twoFactorCode);
            await _context.SaveChangesAsync();

            var htmlContent = $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <div style='background-color: #f4f4f9; padding: 20px; border-radius: 8px;'>
                <h2 style='color: #4CAF50;'>Ваш код для двофакторної автентифікації</h2>
                <p style='font-size: 16px;'>Доброго дня!</p>
                <p style='font-size: 16px;'>Ваш код підтвердження для входу в SkyQuizApp:</p>
                <div style='background-color: #fff; padding: 15px; border: 1px solid #ddd; border-radius: 5px; text-align: center; font-size: 24px; font-weight: bold; color: #333;'>
                    {code}
                </div>
                <p style='font-size: 14px; color: #555;'>Цей код дійсний протягом 10 хвилин.</p>
                <p style='font-size: 14px;'>Якщо ви не запитували код для входу, просто проігноруйте цей лист.</p>
                <p style='font-size: 14px; color: #777;'>З найкращими побажаннями,<br/>Команда SkyQuizApp</p>
            </div>
        </body>
        </html>";

            await _emailService.SendEmailAsync(email, "Ваш код підтвердження для входу", htmlContent);

            return code;
        }

        public async Task<bool> VerifyCodeAsync(int userId, string code)
        {
            var record = await _context.TwoFactorCodes
                .FirstOrDefaultAsync(c => c.UserID == userId && c.Code == code);

            if (record == null)
                return false;

            if (record.ExpiresAt < DateTime.UtcNow)
                return false;

            _context.TwoFactorCodes.Remove(record);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateCode()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }
    }
}