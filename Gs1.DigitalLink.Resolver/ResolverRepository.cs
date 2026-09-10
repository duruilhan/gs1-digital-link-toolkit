using Microsoft.EntityFrameworkCore;
namespace Gs1.DigitalLink.Resolver;
public sealed class ResolverRepository(ResolverDbContext context)
{
    public Task<LinkDefinition?> GetByCanonicalPathAsync(string canonicalPath,
        CancellationToken cancellationToken = default) =>
        context.LinkDefinitions
            .AsNoTracking()
            .Include(item => item.Identifiers.OrderBy(identifier => identifier.Position))
            .Include(item => item.Targets)
            .SingleOrDefaultAsync(item => item.CanonicalPath == canonicalPath, cancellationToken);
}