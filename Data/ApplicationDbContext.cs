using Microsoft.EntityFrameworkCore;
using RegionCodeCollector.Models;

namespace RegionCodeCollector.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Region> Regions => Set<Region>();
        public DbSet<RegionCode> RegionCodes => Set<RegionCode>();
        public DbSet<UserSeenCode> UserSeenCodes => Set<UserSeenCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // regions
            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.HasMany(e => e.Codes)
                    .WithOne(e => e.Region)
                    .HasForeignKey(e => e.RegionId);
            });

            // region_codes
            modelBuilder.Entity<RegionCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Code)
                    .IsUnique();

                entity.HasOne(e => e.Region)
                    .WithMany(e => e.Codes)
                    .HasForeignKey(e => e.RegionId);
            });

            // user_seen_codes
            modelBuilder.Entity<UserSeenCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.UserId, e.RegionCodeId })
                    .IsUnique();

                entity.Property(e => e.SeenAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.SeenCodes)
                    .HasForeignKey(e => e.UserId);

                entity.HasOne(e => e.RegionCode)
                    .WithMany(e => e.UserSeenCodes)
                    .HasForeignKey(e => e.RegionCodeId);
            });
        }
    }
}