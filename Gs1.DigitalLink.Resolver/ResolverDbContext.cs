using Microsoft.EntityFrameworkCore;
namespace Gs1.DigitalLink.Resolver;
public sealed class ResolverDbContext(DbContextOptions<ResolverDbContext> options) : DbContext(options)
{
    public DbSet<LinkDefinition> LinkDefinitions => Set<LinkDefinition>();
    public DbSet<LinkIdentifier> LinkIdentifiers => Set<LinkIdentifier>();
    public DbSet<LinkTarget> LinkTargets => Set<LinkTarget>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LinkDefinition>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.CanonicalPath).HasMaxLength(2_048).IsRequired();
            entity.HasIndex(item => item.CanonicalPath).IsUnique();
        });
        modelBuilder.Entity<LinkIdentifier>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ApplicationIdentifier).HasMaxLength(4).IsRequired();
            entity.Property(item => item.Value).HasMaxLength(90).IsRequired();
            entity.HasIndex(item => new { item.LinkDefinitionId, item.Position }).IsUnique();
            entity.HasIndex(item => new { item.ApplicationIdentifier, item.Value });
            entity.HasOne(item => item.LinkDefinition).WithMany(item => item.Identifiers)
                .HasForeignKey(item => item.LinkDefinitionId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<LinkTarget>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.LinkType).HasMaxLength(64).IsRequired();
            entity.Property(item => item.Url).HasMaxLength(2_048).IsRequired();
            entity.Property(item => item.Language).HasMaxLength(16);
            entity.Property(item => item.MediaType).HasMaxLength(128);
            entity.HasIndex(item => new { item.LinkDefinitionId, item.LinkType, item.Language }).IsUnique();
            entity.HasIndex(item => item.LinkDefinitionId).HasFilter("\"IsDefault\" = TRUE").IsUnique();
            entity.HasOne(item => item.LinkDefinition).WithMany(item => item.Targets)
                .HasForeignKey(item => item.LinkDefinitionId).OnDelete(DeleteBehavior.Cascade);
        });
        ResolverSeedData.Configure(modelBuilder);
    }
}