using Microsoft.EntityFrameworkCore;
using QuotesApi.Models;

namespace QuotesApi.Data;

public class QuotesDbContext : DbContext
{
    public QuotesDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Collection> Collections => Set<Collection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(q => q.Id);

            entity.Property(q => q.Author)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(q => q.IsDeleted)
                .IsRequired();

            entity.HasQueryFilter(q => !q.IsDeleted);
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(c => c.OwnerId)
                .IsRequired();

            entity.OwnsMany(c => c.Items, item =>
            {
                item.WithOwner()
                    .HasForeignKey("CollectionId");

                item.HasKey("CollectionId", "QuoteId");

                item.Property(i => i.QuoteId)
                    .ValueGeneratedNever()
                    .IsRequired();

                item.Property(i => i.AddedAt)
                    .IsRequired();
            });
        });
    }
}