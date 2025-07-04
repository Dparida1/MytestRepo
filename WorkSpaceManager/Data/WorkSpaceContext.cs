using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Models;

namespace WorkSpaceManager.Data
{
    public class WorkSpaceContext : DbContext
    {
        public WorkSpaceContext(DbContextOptions<WorkSpaceContext> options) : base(options)
        {
        }

        public DbSet<WorkSpaceEntity> WorkSpaces { get; set; }
        public DbSet<WorkSpaceRequestEntity> WorkSpaceRequests { get; set; }
        public DbSet<BundleEntity> Bundles { get; set; }
        public DbSet<DirectoryEntity> Directories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<WorkSpaceEntity>()
                .HasOne(w => w.WorkSpaceRequest)
                .WithMany(r => r.WorkSpaces)
                .HasForeignKey(w => w.WorkSpaceRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for better query performance
            modelBuilder.Entity<WorkSpaceEntity>()
                .HasIndex(w => w.WorkSpaceId)
                .IsUnique();

            modelBuilder.Entity<WorkSpaceEntity>()
                .HasIndex(w => w.State);

            modelBuilder.Entity<WorkSpaceEntity>()
                .HasIndex(w => w.Region);

            modelBuilder.Entity<WorkSpaceEntity>()
                .HasIndex(w => w.UserName);

            modelBuilder.Entity<WorkSpaceEntity>()
                .HasIndex(w => w.CreatedAt);

            modelBuilder.Entity<WorkSpaceRequestEntity>()
                .HasIndex(r => r.RequestStatus);

            modelBuilder.Entity<WorkSpaceRequestEntity>()
                .HasIndex(r => r.RequestedAt);

            modelBuilder.Entity<WorkSpaceRequestEntity>()
                .HasIndex(r => r.Region);

            modelBuilder.Entity<BundleEntity>()
                .HasIndex(b => b.BundleId)
                .IsUnique();

            modelBuilder.Entity<BundleEntity>()
                .HasIndex(b => b.Region);

            modelBuilder.Entity<DirectoryEntity>()
                .HasIndex(d => d.DirectoryId)
                .IsUnique();

            modelBuilder.Entity<DirectoryEntity>()
                .HasIndex(d => d.Region);

            // Configure decimal precision for cost estimates if needed
            modelBuilder.Entity<WorkSpaceEntity>()
                .Property(w => w.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<WorkSpaceRequestEntity>()
                .Property(r => r.RequestedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<BundleEntity>()
                .Property(b => b.LastUpdated)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<DirectoryEntity>()
                .Property(d => d.LastUpdated)
                .HasDefaultValueSql("GETUTCDATE()");

            // Seed data for AWS regions
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // You can add seed data here if needed
            // For example, common AWS regions, default bundles, etc.
        }
    }
}