using Moq;
using Moq.Dapper;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Data;
using Dotnetdudes.Web.Blog.Api.Models;
using Dapper;
using System.Net.Http.Json;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    public class BlogEnpointsV1Tests
    {
        // generate tests for BlogEndpointsV1.cs
        [Fact]
        public void GetPosts_ShouldReturnAllPosts()
        {
            var connection = new Mock<IDbConnection>();
            var expected = new Post[] { new Post() { Id = 1, Title = "First Post", Body = "Post body text", Author = "John Morton", Created = DateTime.MinValue, Description = "First post description" } };
            connection.SetupDapper(c => c.Query<Post>(It.IsAny<string>(), null, null, false, null, null))
                .Returns(expected);

            /* await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            var response = await client.GetAsync("/posts/v1");
            var posts = await response.Content.ReadFromJsonAsync < Post[]>();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode */

            var actual = connection.Object.Query<Post>("", null, null, true, null, null).ToArray<Post>();

            Assert.Equal(expected.Length, actual.Length);
        }

        /* [Fact]
         public async Task GetPostById_ReturnsPost()
         {
             // Arrange
             var connection = new Mock<IDbConnection>();
             var expectedPost = new Post { Id = 1, Title = "First Post", Body = "Post body text", Author = "John Morton", Created = DateTime.MinValue, Description = "First post description" };
             _mockDbConnection
                 .Setup(
             db =>
                 db.QueryFirstOrDefaultAsync<Post>(
                     It.IsAny<string>(),
                     It.IsAny<object>(),
                     It.IsAny<IDbTransaction>(),
                     It.IsAny<int?>(),
                     It.IsAny<CommandType?>()
                 )
         )
         .ReturnsAsync(expectedPost);

             var client = _factory
                 .WithWebHostBuilder(builder =>
                 {
                     builder.ConfigureServices(services =>
                     {
                         services.AddSingleton(_mockDbConnection.Object);
                     });
                 })
                 .CreateClient();

             // Act
             var response = await client.GetAsync("/posts/1");

             // Assert
             response.EnsureSuccessStatusCode();
             var post = await response.Content.ReadAsAsync<Post>();
             Assert.Equal(expectedPost.Id, post.Id);
         } */

        [Fact]
        public void GetPostById_ReturnsPost()
        {
            var connection = new Mock<IDbConnection>();
            var expected = new Post { Id = 1, Title = "First Post", Body = "Post body text", Author = "John Morton", Created = DateTime.MinValue, Description = "First post description" };
            connection.SetupDapper(c => c.QueryFirstOrDefault<Post>(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .Returns(expected);

            var actual = connection.Object.QueryFirstOrDefault<Post>("", null, null, null, null);

            Assert.Equal(expected.Id, actual?.Id);
        }

       /* [Fact]
        public void GetPostById_ReturnsNotFound()
        {
            //
        } */

        // [Fact]
        // public async Task TestyTest()
        // {
        //     await using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        //     using var client = application.CreateClient();

        //     var response = await client.GetAsync("/posts/v1");
        //     var posts = await response.Content.ReadFromJsonAsync<Post[]>();
        //     Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        // }

        [Fact]
        public async Task GetPosts_ReturnsAllPosts()
        {
            // Arrange
            var expectedPosts = new List<Post>
            {
                new Post { Id = 1, Title = "First Post" },
                new Post { Id = 2, Title = "Second Post" }
            };
            _mockDbConnection.Setup(db => db.QueryAsync<Post>(It.IsAny<string>()))
                .ReturnsAsync(expectedPosts);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts");

            // Assert
            response.EnsureSuccessStatusCode();
            var posts = await response.Content.ReadAsAsync<IEnumerable<Post>>();
            Assert.Equal(expectedPosts.Count, posts.Count());
        }

        [Fact]
        public async Task GetPostById_ReturnsPost()
        {
            // Arrange
            var expectedPost = new Post { Id = 1, Title = "First Post" };
            _mockDbConnection.Setup(db => db.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(expectedPost);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var post = await response.Content.ReadAsAsync<Post>();
            Assert.Equal(expectedPost.Id, post.Id);
        }

    }
}
