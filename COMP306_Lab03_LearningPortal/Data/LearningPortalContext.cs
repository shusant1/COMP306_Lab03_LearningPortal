
using Microsoft.EntityFrameworkCore;
using COMP306_Lab03_LearningPortal.Models;

namespace COMP306_Lab03_LearningPortal.Data
{
    public class LearningPortalContext : DbContext
    {
        public LearningPortalContext(
            DbContextOptions<LearningPortalContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<LearningResource> LearningResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryID);

            modelBuilder.Entity<LearningResource>()
                .HasKey(r => r.ResourceID);

            modelBuilder.Entity<LearningResource>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.AuthorID);

            modelBuilder.Entity<LearningResource>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(r => r.CategoryID);
        }
    }
}
