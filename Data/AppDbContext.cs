using Microsoft.EntityFrameworkCore;
using StudentEventAPI.Models;

namespace StudentEventAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) 
        {
        }

        // Entity collections with descriptive naming
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Registration> Registrations { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Gathering entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Venue).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.Date).HasDatabaseName("IX_Gatherings_Date");
                entity.HasIndex(e => e.Name).HasDatabaseName("IX_Gatherings_Name");
            });

            // Configure Learner entity
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.FullName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Email).IsRequired().HasMaxLength(255);
                entity.Property(s => s.StudentIdentifier).HasMaxLength(20);
                entity.Property(s => s.Department).HasMaxLength(100);
                entity.Property(s => s.PhoneNumber).HasMaxLength(15);
                entity.Property(s => s.EnrollmentDate).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(s => s.Email).IsUnique().HasDatabaseName("IX_Learners_Email");
                entity.HasIndex(s => s.StudentIdentifier).HasDatabaseName("IX_Learners_StudentId");
            });

            // Configure Registration entity with composite relationships
            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(r => r.RegistrationId);
                
                entity.HasOne(r => r.Event)
                    .WithMany(e => e.Registrations)
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Student)
                    .WithMany(s => s.Registrations)
                    .HasForeignKey(r => r.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.RegistrationTimestamp).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(r => r.SpecialRequirements).HasMaxLength(500);
                entity.Property(r => r.Notes).HasMaxLength(200);
                
                // Unique constraint to prevent duplicate registrations
                entity.HasIndex(r => new { r.EventId, r.StudentId })
                    .IsUnique()
                    .HasDatabaseName("IX_Registrations_Event_Student");
            });

            // Configure Feedback entity
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(f => f.Id);
                
                entity.HasOne(f => f.Event)
                    .WithMany(e => e.Feedbacks)
                    .HasForeignKey(f => f.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Student)
                    .WithMany()
                    .HasForeignKey(f => f.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(f => f.Comment).HasMaxLength(1000);
                entity.Property(f => f.Suggestions).HasMaxLength(500);
                entity.Property(f => f.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Unique constraint to prevent multiple feedback from same student for same event
                entity.HasIndex(f => new { f.EventId, f.StudentId })
                    .IsUnique()
                    .HasDatabaseName("IX_Feedbacks_Event_Student");
            });

            // Seed data configuration
            SeedInitialData(modelBuilder);
        }

        private static void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Seed some initial event categories and sample data if needed
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    Id = 1,
                    Name = "Welcome Orientation 2025",
                    Description = "New student orientation program",
                    Venue = "Main Auditorium",
                    Date = new DateTime(2025, 9, 1, 10, 0, 0),
                    MaxCapacity = 500,
                    Category = EventCategory.Academic,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Event && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Event eventEntity)
                {
                    eventEntity.ModifiedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

