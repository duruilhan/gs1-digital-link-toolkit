using Gs1.DigitalLink.Resolver;
string[] seededPaths =
[
    "/01/08690504080008",
    "/01/08690504080008/10/LOT123",
    "/414/8690123456789"
];
try
{
    await ResolverDatabase.MigrateAsync();
    Console.WriteLine("Migration applied successfully.");
    await using ResolverDbContext context = ResolverDatabase.CreateFromEnvironment();
    var repository = new ResolverRepository(context);
    foreach (string path in seededPaths)
    {
        LinkDefinition? definition = await repository.GetByCanonicalPathAsync(path);
        if (definition is null)
        {
            throw new InvalidOperationException($"Seeded definition was not found: {path}");
        }
        Console.WriteLine($"{definition.CanonicalPath} -> {definition.Targets.Count} target(s)");
        foreach (LinkTarget target in definition.Targets.OrderBy(item => item.LinkType).ThenBy(item => item.Language))
        {
            string language = target.Language ?? "-";
            Console.WriteLine($"  {target.LinkType}, language={language}, default={target.IsDefault}: {target.Url}");
        }
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Resolver database setup failed: {exception.Message}");
    return 1;
}
return 0;