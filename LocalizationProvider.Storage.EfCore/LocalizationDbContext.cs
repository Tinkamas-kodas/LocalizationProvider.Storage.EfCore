using DbLocalizationProvider.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LocalizationProvider.Storage.EfCore
{
    public class LocalizationDbContext(DbContextOptions<LocalizationDbContext> options) : DbContext(options)
    {
        public virtual DbSet<LocalizationResource> LocalizationResources { get; set; }

        public virtual DbSet<LocalizationResourceTranslation> LocalizationResourceTranslations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<LocalizationResource>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Author).HasMaxLength(100);
                entity.Property(e => e.FromCode);
                entity.Property(e => e.IsHidden);
                entity.Property(e => e.IsModified);
                entity.Property(e => e.ModificationDate);
                entity.Property(e => e.Notes).HasMaxLength(3000);
                entity.Property(e => e.ResourceKey).HasMaxLength(1000);
            });

            modelBuilder.Entity<LocalizationResourceTranslation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ResourceId);

                entity.HasIndex(e => new { e.Language, e.ResourceId }).IsUnique();

                entity.Property(e => e.Language).HasMaxLength(10);
                entity.Property(e => e.ModificationDate);
                entity.Property(e => e.Value).HasMaxLength(3000);

                entity
                    .HasOne(d => d.LocalizationResource)
                    .WithMany(p => p.Translations)
                    .HasForeignKey(d => d.ResourceId);
            });

        }

    }
}
