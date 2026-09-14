using System.Net;
using System.Net.Http.Json;
using Gs1.DigitalLink.Resolver;
using Gs1.DigitalLink.Resolver.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Gs1.DigitalLink.Resolver.Api.Tests;
public sealed class ResolverApiTests
{
    [Fact]
    public async Task Resolve_DefaultTarget_ReturnsTemporaryRedirectAndLinkHeaders()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = CreateNonRedirectingClient(factory);
        HttpResponseMessage response = await client.GetAsync("/01/08690504080008");
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://example.com/products/08690504080008", response.Headers.Location!.ToString());
        Assert.Equal(3, response.Headers.GetValues("Link").Count());
        Assert.Contains(response.Headers.GetValues("Link"), value =>
            value.Contains("rel=\"https://gs1.org/voc/defaultLink\"") && value.Contains("type=\"text/html\""));
    }
    [Theory]
    [InlineData("tr", "https://example.com/tr/products/08690504080008")]
    [InlineData("en", "https://example.com/en/products/08690504080008")]
    public async Task Resolve_LinkTypeAndLanguage_SelectMatchingTarget(string language, string expectedLocation)
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = CreateNonRedirectingClient(factory);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/01/08690504080008?linkType=gs1:pip");
        request.Headers.AcceptLanguage.ParseAdd(language);
        HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location!.ToString());
    }
    [Fact]
    public async Task Resolve_UnsupportedLanguage_FallsBackToDefaultTarget()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = CreateNonRedirectingClient(factory);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/01/08690504080008?linkType=gs1:pip");
        request.Headers.AcceptLanguage.ParseAdd("de");
        HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://example.com/products/08690504080008", response.Headers.Location!.ToString());
    }
    [Fact]
    public async Task Resolve_All_ReturnsEveryTargetWithoutRedirecting()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/01/08690504080008?linkType=all");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, (await response.Content.ReadFromJsonAsync<DefinitionResponse>())!.Targets.Count);
        Assert.Null(response.Headers.Location);
    }
    [Fact]
    public async Task Resolve_QualifiedDefinitionWithSingleTarget_ReturnsTemporaryRedirect()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = CreateNonRedirectingClient(factory);
        HttpResponseMessage response = await client.GetAsync("/01/08690504080008/10/LOT123");
        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://example.com/tr/products/08690504080008/lots/LOT123", response.Headers.Location!.ToString());
    }
    [Fact]
    public async Task Resolve_MissingQualifiedDefinition_DoesNotFallBackToLessQualifiedRecord()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = CreateNonRedirectingClient(factory);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/01/08690504080008/21/YOKBOYLE9")).StatusCode);
    }
    [Fact]
    public async Task Resolve_MalformedOddSegmentPath_ReturnsBadRequest()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = CreateNonRedirectingClient(factory);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/01/8690504080008")).StatusCode);
    }
    [Fact]
    public async Task Post_CreatesCanonicalPathAndReturnsLocation()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        var request = CreateRequest(
            [new("01", "08690504080008"), new("21", "SERIAL9")],
            "https://example.com/tr/products/08690504080008/serials/SERIAL9");
        HttpResponseMessage response = await client.PostAsJsonAsync("/definitions", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        DefinitionResponse? body = await response.Content.ReadFromJsonAsync<DefinitionResponse>();
        Assert.Equal("/01/08690504080008/21/SERIAL9", body!.CanonicalPath);
    }
    [Fact]
    public async Task Post_CanonicalizesQualifiers_AndDuplicateCanonicalPathReturnsConflict()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        var reversed = CreateRequest(
            [new("01", "08690504080008"), new("21", "SERIAL9"), new("10", "LOT123")],
            "https://example.com/qualified");
        var canonical = CreateRequest(
            [new("01", "08690504080008"), new("10", "LOT123"), new("21", "SERIAL9")],
            "https://example.com/qualified");
        HttpResponseMessage created = await client.PostAsJsonAsync("/definitions", reversed);
        HttpResponseMessage conflict = await client.PostAsJsonAsync("/definitions", canonical);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        DefinitionResponse? body = await created.Content.ReadFromJsonAsync<DefinitionResponse>();
        Assert.Equal("/01/08690504080008/10/LOT123/21/SERIAL9", body!.CanonicalPath);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
    }
    [Fact]
    public async Task Post_DataAttributeDoesNotChangeDefinitionIdentity()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        var withAttribute = CreateRequest(
            [new("01", "08690504080008"), new("21", "SERIAL9"), new("17", "261231")],
            "https://example.com/attribute");
        var withoutAttribute = CreateRequest(
            [new("01", "08690504080008"), new("21", "SERIAL9")],
            "https://example.com/attribute");
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/definitions", withAttribute)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/definitions", withoutAttribute)).StatusCode);
    }
    [Fact]
    public async Task Post_InvalidValueAndInvalidQualifierCombinationReturnBadRequestWithDetails()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        var invalidCheckDigit = CreateRequest([new("01", "08690504080009")], "https://example.com/invalid");
        var glnWithLot = CreateRequest([new("414", "8690123456789"), new("10", "LOT123")], "https://example.com/invalid");
        HttpResponseMessage invalidValueResponse = await client.PostAsJsonAsync("/definitions", invalidCheckDigit);
        HttpResponseMessage invalidCombinationResponse = await client.PostAsJsonAsync("/definitions", glnWithLot);
        Assert.Equal(HttpStatusCode.BadRequest, invalidValueResponse.StatusCode);
        Assert.Contains("AI 01", await invalidValueResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest, invalidCombinationResponse.StatusCode);
        Assert.NotEmpty(await invalidCombinationResponse.Content.ReadAsStringAsync());
    }
    [Fact]
    public async Task GetByPath_DistinguishesBadRequestNotFoundAndSuccess()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        HttpResponseMessage success = await client.GetAsync("/definitions?path=/01/08690504080008");
        HttpResponseMessage invalid = await client.GetAsync("/definitions?path=01/08690504080008");
        HttpResponseMessage missing = await client.GetAsync("/definitions?path=/01/08690504080015");
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        Assert.Equal(3, (await success.Content.ReadFromJsonAsync<DefinitionResponse>())!.Targets.Count);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }
    [Fact]
    public async Task Delete_RemovesDefinitionAndItsTargets()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        HttpResponseMessage created = await client.PostAsJsonAsync("/definitions", CreateRequest(
            [new("01", "08690504080008"), new("21", "DELETE9")], "https://example.com/delete"));
        DefinitionResponse body = (await created.Content.ReadFromJsonAsync<DefinitionResponse>())!;
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/definitions/{body.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/definitions/{body.Id}")).StatusCode);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        ResolverDbContext context = scope.ServiceProvider.GetRequiredService<ResolverDbContext>();
        Assert.False(await context.LinkTargets.AnyAsync(target => target.LinkDefinitionId == body.Id));
    }
    [Fact]
    public async Task OpenApiAndScalarUiAreAvailable()
    {
        await using var factory = new ResolverApiFactory();
        using HttpClient client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/scalar/v1")).StatusCode);
    }
    private static CreateDefinitionRequest CreateRequest(IReadOnlyList<ElementRequest> elements, string url) =>
        new(elements, [new TargetRequest("gs1:pip", url, "tr", "text/html", true)]);
    private static HttpClient CreateNonRedirectingClient(ResolverApiFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
internal sealed class ResolverApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ResolverDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ResolverDbContext>>();
            services.RemoveAll<ResolverDbContext>();
            services.AddDbContext<ResolverDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ResolverDbContext>().Database.EnsureCreated();
        });
    }
}