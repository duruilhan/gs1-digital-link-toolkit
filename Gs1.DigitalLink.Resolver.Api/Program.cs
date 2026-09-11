using Gs1.DigitalLink;
using Gs1.DigitalLink.Resolver;
using Gs1.DigitalLink.Resolver.Api;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ResolverDbContext>((services, options) =>
{
    string connectionString = services.GetRequiredService<IConfiguration>()[ResolverDatabase.ConnectionStringEnvironmentVariable]
        ?? throw new InvalidOperationException($"Set {ResolverDatabase.ConnectionStringEnvironmentVariable} before starting the API.");
    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<ResolverRepository>();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("GS1 Digital Link Resolver API"));

var definitions = app.MapGroup("/definitions").WithTags("Definitions");

definitions.MapPost("/", async (CreateDefinitionRequest request, ResolverRepository repository, CancellationToken cancellationToken) =>
{
    if (request.Elements is null || request.Targets is null || request.Elements.Count == 0 || request.Targets.Count == 0)
        return Results.BadRequest(new { error = "At least one AI/value element and one target are required." });

    foreach (ElementRequest element in request.Elements)
    {
        if (!ApplicationIdentifierCatalog.TryGet(element.ApplicationIdentifier, out _) ||
            !ApplicationIdentifierValidator.IsValid(element.ApplicationIdentifier, element.Value))
            return Results.BadRequest(new { error = $"AI {element.ApplicationIdentifier} has an unknown code or invalid value." });
    }

    string digitalLink;
    try
    {
        digitalLink = Gs1DigitalLinkBuilder.Build(request.Elements.Select(item =>
            new Gs1Element(item.ApplicationIdentifier, item.Value)));
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }

    var uri = new Uri(digitalLink);
    string canonicalPath = uri.AbsolutePath;
    if (await repository.GetByCanonicalPathAsync(canonicalPath, cancellationToken) is not null)
        return Results.Conflict(new { error = $"A definition already exists for path {canonicalPath}." });

    IReadOnlyList<Gs1Element> canonicalElements = Gs1DigitalLinkParser.Parse(digitalLink);
    var definition = new LinkDefinition
    {
        Id = Guid.NewGuid(),
        CanonicalPath = canonicalPath,
        Identifiers = canonicalElements
            .Where(item => ApplicationIdentifierCatalog.TryGet(item.ApplicationIdentifier, out var metadata) &&
                metadata!.Role != ApplicationIdentifierRole.DataAttribute)
            .Select((item, index) => new LinkIdentifier
            {
                Id = Guid.NewGuid(),
                Position = index,
                ApplicationIdentifier = item.ApplicationIdentifier,
                Value = item.Value
            }).ToList(),
        Targets = request.Targets.Select(item => new LinkTarget
        {
            Id = Guid.NewGuid(),
            LinkType = item.LinkType,
            Url = item.Url,
            Language = item.Language,
            MediaType = item.MediaType,
            IsDefault = item.IsDefault
        }).ToList()
    };

    try
    {
        await repository.AddAsync(definition, cancellationToken);
    }
    catch (DbUpdateException)
    {
        return Results.Conflict(new { error = "The definition or one of its targets conflicts with an existing record." });
    }

    return Results.Created($"/definitions/{definition.Id}", definition.ToResponse());
})
.WithSummary("Create a resolver definition")
.WithDescription("Builds the canonical path with Gs1DigitalLinkBuilder. Data attributes are accepted but do not change definition identity. Example body: {\"elements\":[{\"applicationIdentifier\":\"01\",\"value\":\"08690504080008\"},{\"applicationIdentifier\":\"21\",\"value\":\"SERIAL9\"}],\"targets\":[{\"linkType\":\"gs1:pip\",\"url\":\"https://example.com/tr/products/08690504080008/serials/SERIAL9\",\"language\":\"tr\",\"mediaType\":\"text/html\",\"isDefault\":true}]}")
.Produces<DefinitionResponse>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict);

definitions.MapGet("/{id:guid}", async (Guid id, ResolverRepository repository, CancellationToken cancellationToken) =>
    await repository.GetByIdAsync(id, cancellationToken) is { } definition
        ? Results.Ok(definition.ToResponse())
        : Results.NotFound())
    .WithSummary("Get a definition by identifier")
    .Produces<DefinitionResponse>()
    .Produces(StatusCodes.Status404NotFound);

definitions.MapGet("/", async (string path, ResolverRepository repository, CancellationToken cancellationToken) =>
{
    if (!path.StartsWith('/') || !Gs1DigitalLinkParser.TryParse("https://id.gs1.org" + path, out _))
        return Results.BadRequest(new { error = "The path is not a valid GS1 Digital Link path." });
    return await repository.GetByCanonicalPathAsync(path, cancellationToken) is { } definition
        ? Results.Ok(definition.ToResponse())
        : Results.NotFound();
})
.WithSummary("Get a definition by canonical Digital Link path")
.Produces<DefinitionResponse>()
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

definitions.MapDelete("/{id:guid}", async (Guid id, ResolverRepository repository, CancellationToken cancellationToken) =>
    await repository.DeleteAsync(id, cancellationToken) ? Results.NoContent() : Results.NotFound())
    .WithSummary("Delete a definition and its targets")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

public partial class Program;

internal static class ResponseMappings
{
    internal static DefinitionResponse ToResponse(this LinkDefinition definition) => new(
        definition.Id,
        definition.CanonicalPath,
        definition.Identifiers.OrderBy(item => item.Position)
            .Select(item => new ElementResponse(item.ApplicationIdentifier, item.Value)).ToArray(),
        definition.Targets.Select(item => new TargetResponse(item.LinkType, item.Url, item.Language, item.MediaType, item.IsDefault)).ToArray());
}

public sealed record ElementResponse(string ApplicationIdentifier, string Value);
public sealed record TargetResponse(string LinkType, string Url, string? Language, string? MediaType, bool IsDefault);
public sealed record DefinitionResponse(Guid Id, string CanonicalPath, IReadOnlyList<ElementResponse> Elements, IReadOnlyList<TargetResponse> Targets);
