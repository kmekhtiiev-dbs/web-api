using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).HasMaxLength(200).IsRequired();
            entity.Property(b => b.Author).HasMaxLength(120).IsRequired();
            entity.Property(b => b.Isbn).HasMaxLength(32).IsRequired();
            entity.HasIndex(b => b.Isbn).IsUnique();
        });
    }
}
