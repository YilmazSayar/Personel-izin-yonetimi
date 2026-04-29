using Microsoft.EntityFrameworkCore;
using personelizin_backend.Models;

namespace personelizin_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        /// <summary>İzin talepleri (Leaves) — veritabanı tablosu: PermissionRequest</summary>
        public DbSet<PermissionRequest> Leaves { get; set; }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UserUnit> UserUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<UserUnit>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.UnitId });
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Unit).WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Cascade);
            });

            // İzin talepleri tek tabloda: PermissionRequest (migration ile uyumlu)
            modelBuilder.Entity<PermissionRequest>(entity =>
            {
                entity.ToTable("PermissionRequest");
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            });
        }
    }
}