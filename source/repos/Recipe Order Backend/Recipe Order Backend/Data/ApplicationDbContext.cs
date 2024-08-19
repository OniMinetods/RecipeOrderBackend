using RecipeOrder.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeOrder.Models;
using System.Text.Json;

namespace RecipeOrder.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser>
    {
        public DbSet<Recipe> Recipes { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Recipe>()
                .Property(i => i.Reviews)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = true }),
                    v => JsonSerializer.Deserialize<List<RecipeReviews>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
        }
    }
    public class ApplicationUserDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public ApplicationUserDbContext(DbContextOptions<ApplicationUserDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserID);
            });

            modelBuilder.Entity<Order>()
            .Property(o => o.OrderID)
            .ValueGeneratedOnAdd();
        }
    }

}
