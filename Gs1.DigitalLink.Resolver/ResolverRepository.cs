using Microsoft.EntityFrameworkCore;
namespace Gs1.DigitalLink.Resolver;
public sealed class ResolverRepository(ResolverDbContext context)
{
    public Task<LinkDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.LinkDefinitions
            .AsNoTracking()
            .Include(item => item.Identifiers.OrderBy(identifier => identifier.Position))
            .Include(item => item.Targets)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    public Task<LinkDefinition?> GetByCanonicalPathAsync(string canonicalPath,
        CancellationToken cancellationToken = default) =>
        context.LinkDefinitions
            .AsNoTracking()
            .Include(item => item.Identifiers.OrderBy(identifier => identifier.Position))
            .Include(item => item.Targets)
            .SingleOrDefaultAsync(item => item.CanonicalPath == canonicalPath, cancellationToken);
    public async Task AddAsync(LinkDefinition definition, CancellationToken cancellationToken = default)
    {
        context.LinkDefinitions.Add(definition);
        await context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LinkDefinition? definition = await context.LinkDefinitions
            .Include(item => item.Identifiers)
            .Include(item => item.Targets)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (definition is null)
            return false;
        context.LinkIdentifiers.RemoveRange(definition.Identifiers);
        context.LinkTargets.RemoveRange(definition.Targets);
        context.LinkDefinitions.Remove(definition);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}