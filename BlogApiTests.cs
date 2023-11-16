using Xunit;
using Moq;
using Dapper;
using System.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BlogApiTests
{
    private readonly WebApplicationFactory<Dotnetdudes.Web.Blog.Api.Program> _factory;
    private readonly Mock<IDbConnection> _mockDbConnection;

    public BlogApiTests()
    {
        _factory = new WebApplicationFactory<Dotnetdudes.Web.Blog.Api.Program>();
        _mockDbConnection = new Mock<IDbConnection>();
    }

    [Fact]
    public async Task GetPosts_ReturnsAllPosts()
    {
        // Arrange
        var mockPosts = new List<Post> { new Post(), new Post() };
        _mockDbConnection.Setup(db => db.QueryAsync<Post>(It.IsAny<string>())).ReturnsAsync(mockPosts);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts");

        // Assert
        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadAsAsync<IEnumerable<Post>>();
        Assert.Equal(2, posts.Count());
    }

    // Repeat the above test method for each endpoint in your API, adjusting the Arrange, Act, and Assert steps as necessary.
}