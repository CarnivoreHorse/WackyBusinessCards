using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WackyBusinessCards.Constants;
using WackyBusinessCards.Models;

namespace WackyBusinessCards.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusinessCard> BusinessCards { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the BusinessCard entity
        modelBuilder.Entity<BusinessCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Company).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.BackgroundColor).IsRequired().HasMaxLength(7);
            entity.Property(e => e.TextColor).IsRequired().HasMaxLength(7);
            entity.Property(e => e.FontFamily).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BorderStyle).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BorderColor).IsRequired().HasMaxLength(7);
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.SpecialEffect).IsRequired().HasMaxLength(20);

            // Configure the relationship with ApplicationUser
            entity.HasOne(e => e.User)
                .WithMany(u => u.BusinessCards)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ApplicationUser entity
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            // SQLite doesn't support non-constant defaults, so we'll handle this in code
            entity.Property(e => e.CreatedAt);
        });

        // Seed roles
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "admin-role-id",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "9cf14c2c-19ca-4f69-a702-c0d2c25d6c51"
            },
            new IdentityRole
            {
                Id = "user-role-id",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "3d9c9f2f-dd8a-4c8a-b1ad-09548a0c3bab"
            }
        );

        // Create users
        var demoUserId = "demo-user-id";
        var adminUserId = "admin-user-id";

        // Demo regular user
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = demoUserId,
                UserName = "demo@example.com",
                NormalizedUserName = "DEMO@EXAMPLE.COM",
                Email = "demo@example.com",
                NormalizedEmail = "DEMO@EXAMPLE.COM",
                EmailConfirmed = true,
                // Pre-computed hash for password "Demo@123"
                PasswordHash = "AQAAAAIAAYagAAAAELBBjRDGnFPZKbQUQY1jGKAydC/Qs8AMeUg1KQmGaQp9oF5iHDjYZGFPmxl3BNXHHw==",
                SecurityStamp = "VVPCRDAS3MJWQD5CSW2GWPRADBXEZINA",
                ConcurrencyStamp = "c8554266-b401-4519-9aeb-a9283053fc58",
                FirstName = "Demo",
                LastName = "User",
                CreatedAt = new DateTime(2025, 5, 1) // Fixed date for seeding
            },
            // Admin user
            new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                // Pre-computed hash for password "Admin@123"
                PasswordHash = "AQAAAAIAAYagAAAAEEzrqscVVKw+cVunoGW+MKF+lK3eSW/KEF11V5wxLXdULX7jYQGjHD+L2yeAP5xvzw==",
                SecurityStamp = "HNXMWMGKNVIXLSPVKFWGVCDYTWWC5XNZ",
                ConcurrencyStamp = "e7f1d3e9-6bd0-4dff-8b4a-3a9b6e0a5b5c",
                FirstName = "System",
                LastName = "Administrator",
                CreatedAt = new DateTime(2025, 5, 1) // Fixed date for seeding
            }
        );

        // Assign roles to users
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            // Demo user gets User role
            new IdentityUserRole<string>
            {
                UserId = demoUserId,
                RoleId = "user-role-id"
            },
            // Admin user gets Admin role
            new IdentityUserRole<string>
            {
                UserId = adminUserId,
                RoleId = "admin-role-id"
            }
        );

        // Seed initial data
        modelBuilder.Entity<BusinessCard>().HasData(
            new BusinessCard
            {
                Id = 1,
                Name = "Willy Wonka",
                Title = "Chocolate Extraordinaire",
                Company = "Wonka Industries",
                Email = "willy@wonkachocolate.com",
                Phone = "555-CHOCOLATE",
                Website = "www.wonkachocolate.com",
                Address = "1 Chocolate Factory Way, Loompaland",
                BackgroundColor = "#9b59b6",
                TextColor = "#ffffff",
                FontFamily = "'Comic Sans MS', cursive",
                BorderStyle = "dashed",
                BorderColor = "#f1c40f",
                BorderWidth = 5,
                BorderRadius = 15,
                ImageUrl = "/images/chocolate.png",
                SpecialEffect = "rotate",
                UserId = demoUserId
            },
            new BusinessCard
            {
                Id = 2,
                Name = "Captain Jack Sparrow",
                Title = "Pirate Captain",
                Company = "Black Pearl Enterprises",
                Email = "captain@blackpearl.sea",
                Phone = "ARRRR-MATEY",
                Website = "www.pirateslife.com",
                Address = "Somewhere in the Caribbean",
                BackgroundColor = "#34495e",
                TextColor = "#f1c40f",
                FontFamily = "'Pirata One', cursive",
                BorderStyle = "double",
                BorderColor = "#c0392b",
                BorderWidth = 7,
                BorderRadius = 0,
                ImageUrl = "/images/pirate.png",
                SpecialEffect = "shadow",
                UserId = demoUserId
            },
            new BusinessCard
            {
                Id = 3,
                Name = "Luna Lovegood",
                Title = "Magizoologist",
                Company = "The Quibbler",
                Email = "luna@quibbler.wiz",
                Phone = "PATRONUS-123",
                Website = "www.thequibbler.wiz",
                Address = "Ottery St. Catchpole",
                BackgroundColor = "#3498db",
                TextColor = "#ffffff",
                FontFamily = "'Indie Flower', cursive",
                BorderStyle = "dotted",
                BorderColor = "#e74c3c",
                BorderWidth = 3,
                BorderRadius = 30,
                ImageUrl = "/images/magic.png",
                SpecialEffect = "sparkle",
                UserId = demoUserId
            }
        );
    }
}
