using Microsoft.EntityFrameworkCore;
using HyperHiveBackend.Models;

namespace HyperHiveBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Learner> Learners { get; set; }
        public DbSet<Mentor> Mentors { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Learner entity
            modelBuilder.Entity<Learner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                
                // Configure JSON column for AI profile data
                entity.Property(e => e.AIProfileData)
                    .HasColumnType("json");
            });

            // Configure Mentor entity
            modelBuilder.Entity<Mentor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                
                // Configure JSON column for AI profile data
                entity.Property(e => e.AIProfileData)
                    .HasColumnType("json");
            });

            // Configure Manager entity
            modelBuilder.Entity<Manager>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Team).HasMaxLength(100);
                
                // Configure JSON column for AI profile data
                entity.Property(e => e.AIProfileData)
                    .HasColumnType("json");
            });

            // Configure Quiz entity
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.QuizType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Difficulty).HasMaxLength(50);
                
                // Configure JSON column for quiz data
                entity.Property(e => e.QuizData)
                    .IsRequired()
                    .HasColumnType("json");
                
                // Configure relationship with Learner
                entity.HasOne(e => e.Learner)
                    .WithMany()
                    .HasForeignKey(e => e.LearnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure QuizAttempt entity
            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure JSON column for learner answers
                entity.Property(e => e.LearnerAnswers)
                    .IsRequired()
                    .HasColumnType("json");
                
                entity.Property(e => e.Percentage)
                    .HasPrecision(5, 2);
                
                // Configure relationship with Quiz
                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.QuizAttempts)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Configure relationship with Learner
                entity.HasOne(e => e.Learner)
                    .WithMany()
                    .HasForeignKey(e => e.LearnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

