using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.IntegrationTests;

public class HealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_responde_200()
    {
        var response = await factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
