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

    [Fact]
    public async Task GetPost_ReturnsPost()
    {
        // Arrange
        var mockPost = new Post();
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
    }

    [Fact]
    public async Task GetPost_ReturnsNotFound()
    {
        // Arrange
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((Post)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithComments()
    {
        // Arrange
        var mockPost = new Post();
        var mockComments = new List<Comment> { new Comment(), new Comment() };
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockComments);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Equal(2, post.Comments.Count());
    }   

    [Fact]
    public async Task GetPostComments_ReturnsNotFound()
    {
        // Arrange
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((Post)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithNoComments()
    {
        // Arrange
        var mockPost = new Post();
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new List<Comment>());

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Empty(post.Comments);
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithCommentsForPost()
    {
        // Arrange
        var mockPost = new Post();
        var mockComments = new List<Comment> { new Comment(), new Comment() };
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockComments);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Equal(2, post.Comments.Count());
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithCommentsForPostWithNoComments()
    {
        // Arrange
        var mockPost = new Post();
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new List<Comment>());

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Empty(post.Comments);
    }

    [Fact]
    public async Task GetPostComments_ReturnsNotFoundForPost()
    {
        // Arrange
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((Post)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsNotFoundForComments()
    {
        // Arrange
        var mockPost = new Post();
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((IEnumerable<Comment>)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsNotFoundForPostAndComments()
    {
        // Arrange
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((Post)null);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((IEnumerable<Comment>)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithCommentsForPostWithComments()
    {
        // Arrange
        var mockPost = new Post();
        var mockComments = new List<Comment> { new Comment(), new Comment() };
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockComments);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);                
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Equal(2, post.Comments.Count());
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithCommentsForPostWithNoComments()
    {
        // Arrange
        var mockPost = new Post();
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new List<Comment>());

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);                
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Empty(post.Comments);
    }

    [Fact]
    public async Task GetPostComments_ReturnsNotFoundForPost()
    {
        // Arrange
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((Post)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);                
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsNotFoundForComments()
    {
        // Arrange
        var mockPost = new Post();
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((IEnumerable<Comment>)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);                
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsNotFoundForPostAndComments()
    {
        // Arrange
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((Post)null);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync((IEnumerable<Comment>)null);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);                
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ReturnsPostWithCommentsForPostWithComments()
    {
        // Arrange
        var mockPost = new Post();
        var mockComments = new List<Comment> { new Comment(), new Comment() };
        _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockPost);
        _mockDbConnection.Setup(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(mockComments);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _mockDbConnection.Object);                
            });
        })
        .CreateClient();

        // Act
        var response = await client.GetAsync("/posts/1/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadAsAsync<Post>();
        Assert.Equal(mockPost, post);
        Assert.Equal(2, post.Comments.Count());
    }

    // Repeat the above test method for each endpoint in your API, adjusting the Arrange, Act, and Assert steps as necessary.
}