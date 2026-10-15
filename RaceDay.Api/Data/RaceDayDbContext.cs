using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<AppUser> Users { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Enrolment> Enrolments { get; set; } = null!;
    public DbSet<Result> Results { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasKey(u => u.UserId);

        // One Role -> many Users
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // One Organiser (AppUser) -> many Events
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Event -> many Categories
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // One Participant -> many Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(en => en.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Event -> many Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Event)
            .WithMany(e => e.Enrolments)
            .HasForeignKey(en => en.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Category -> many Enrolments
        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(en => en.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // A Participant may only enrol once per Event/Category combination
        modelBuilder.Entity<Enrolment>()
            .HasIndex(en => new { en.ParticipantId, en.EventId })
            .IsUnique();

        // One Enrolment -> zero or one Result
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentId)
            .IsUnique();

        // Seed the two fixed roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, Name = "Organiser" },
            new Role { RoleId = 2, Name = "Participant" }
        );

        modelBuilder.Entity<Category>()
            .Property(c => c.DistanceKm)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Category>()
            .HasIndex(c => new { c.EventId, c.Name })
            .IsUnique(); 

    }
}
