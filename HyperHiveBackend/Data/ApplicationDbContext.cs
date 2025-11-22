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
        }
    }
}

