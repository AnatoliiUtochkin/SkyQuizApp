using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Models;

namespace SkyQuizApp.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TwoFactorCode> TwoFactorCodes { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Answer)
                .WithMany()
                .HasForeignKey(ua => ua.AnswerID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.TestSession)
                .WithMany()
                .HasForeignKey(ua => ua.TestSessionID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TwoFactorCode>()
                .HasOne(tf => tf.User)
                .WithMany()
                .HasForeignKey(tf => tf.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TestSession>()
                .HasOne(ts => ts.User)
                .WithMany()
                .HasForeignKey(ts => ts.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TestSession>()
                .HasOne(ts => ts.Test)
                .WithMany(t => t.TestSessions)
                .HasForeignKey(ts => ts.TestID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.TestSession)
                .WithOne(ts => ts.Result)
                .HasForeignKey<Result>(r => r.SessionID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Test>()
                .HasIndex(t => t.TestKey)
                .IsUnique();
        }
    }
}
