using Microsoft.EntityFrameworkCore;
using VlabShop.Web.Models;

namespace VlabShop.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Price)
                .HasColumnType("decimal(18,2)");

            entity.HasOne(product => product.Category)
                .WithMany()
                .HasForeignKey(product => product.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
