using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Dapper;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dotnetdudes.Web.Blog.Api.Tests.Unit
{
    public class BlogEndpointsV1UnitTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {

        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IDbConnection> _mockDbConnection;

        public BlogEndpointsV1UnitTests()
        {
            _factory = new WebApplicationFactory<Program>();
            _mockDbConnection = new Mock<IDbConnection>();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        [Fact]
        public async Task GetPosts_ReturnsOk()
        {
            // arrange
            var posts = new[]
            {
                new Post { Id = 1, Title = "Post 1", Body = "Content 1", Description="description", Author="Dotnetdude" },
                new Post { Id = 2, Title = "Post 2", Body = "Content 2", Description="description", Author="Dotnetdude" },
                new Post { Id = 3, Title = "Post 3", Body = "Content 3", Description="description", Author="Dotnetdude" },
            };
            _mockDbConnection.SetupDapperAsync(c => c.QueryAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(posts);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.GetAsync("/posts/v1");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responsePosts = await response.Content.ReadFromJsonAsync<Post[]>();
            responsePosts.Should().BeEquivalentTo(posts);
        }

        [Fact]
        public async Task GetPosts_ReturnsOk_Empty()
        {
            // arrange
            var posts = new Post[0];
            _mockDbConnection.SetupDapperAsync(c => c.QueryAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(posts);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.GetAsync("/posts/v1");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responsePosts = await response.Content.ReadFromJsonAsync<Post[]>();
            responsePosts.Should().BeEquivalentTo(posts);
        }

        [Fact]
        public async Task GetPost_ReturnsOk()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Description = "description", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(post);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.GetAsync("/posts/v1/1");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responsePost = await response.Content.ReadFromJsonAsync<Post>();
            responsePost.Should().BeEquivalentTo(post);
        }

        [Fact]
        public async Task GetPost_ReturnsNotFound()
        {
            // arrange
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(default(Post));
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.GetAsync("/posts/v1/1");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPost_ReturnsBadRequest()
        {
            // arrange
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(default(Post));
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.GetAsync("/posts/v1/abc");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostPost_ReturnsCreated()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Description = "Post description", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(1);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.PostAsJsonAsync("/posts/v1", post);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responsePost = await response.Content.ReadFromJsonAsync<Post>();
            responsePost.Should().BeEquivalentTo(post);
        }

        [Fact]
        public async Task PostPost_ReturnsValidationProblem()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(1);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.PostAsJsonAsync("/posts/v1", post);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseProblem?.Errors.Should().ContainKey("Description");
        }

        [Fact]
        public async Task PutPost_ReturnsOk()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Description = "Post description", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(1);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.PutAsJsonAsync("/posts/v1/1", post);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responsePost = await response.Content.ReadFromJsonAsync<Post>();
            Assert.Equal(post.Title, responsePost?.Title);
            Assert.Equal(post.Description, responsePost?.Description);
            Assert.Equal(post.Body, responsePost?.Body);
            Assert.Equal(post.Author, responsePost?.Author);
        }

        [Fact]
        public async Task PutPost_ReturnsNotFound()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Description = "Post description", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(0);
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(default(Post));
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.PutAsJsonAsync("/posts/v1/1", post);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PutPost_ReturnsValidationProblem()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(1);
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(post);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.PutAsJsonAsync("/posts/v1/1", post);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseProblem?.Errors.Should().ContainKey("Description");
        }

        [Fact]
        public async Task PutPost_ReturnsBadRequest()
        {
            // arrange
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Author = "Dotnetdude" };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(1);
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(post);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);

                });
            }).CreateClient();

            // act
            var response = await client.PutAsJsonAsync("/posts/v1/abc", post);

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeletePost_ReturnsNoContent()
        {
            // arrange
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(1);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });

            }).CreateClient();

            // act
            var response = await client.DeleteAsync("/posts/v1/1");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeletePost_ReturnsNotFound()
        {
            // arrange
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(0);
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(default(Post));
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);

                });
            }).CreateClient();

            // act
            var response = await client.DeleteAsync("/posts/v1/100");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeletePost_ReturnsBadRequest()
        {
            // arrange
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(0);
            _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(default(Post));
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);

                });
            }).CreateClient();

            // act
            var response = await client.DeleteAsync("/posts/v1/abc");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCommentById_ReturnsComment()
        {
            // Arrange
            var expectedComment = new Comment { Id = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now };
            _mockDbConnection.SetupDapperAsync(db => db.QueryFirstOrDefaultAsync<Comment>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(expectedComment);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/comments/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var comment = await response.Content.ReadFromJsonAsync<Comment>();
            Assert.Equal(expectedComment.Id, comment?.Id);
        }

        [Fact]
        public async Task GetCommentById_ReturnsNotFound()
        {
            // Arrange
            _mockDbConnection.SetupDapperAsync(db => db.QueryFirstOrDefaultAsync<Comment>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(default(Comment));

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/comments/1");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCommentById_ReturnsBadRequest()
        {
            // Arrange
            _mockDbConnection.SetupDapperAsync(db => db.QueryFirstOrDefaultAsync<Comment>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(default(Comment));

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/comments/abc");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetComments_ReturnsComments()
        {
            // Arrange
            using FileStream stream = File.OpenRead("./Data/comments.json");
            var expectedComments = await JsonSerializer.DeserializeAsync<List<Comment>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            // query async
            _mockDbConnection.SetupDapperAsync(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(expectedComments!);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/1/comments");

            // Assert
            response.EnsureSuccessStatusCode();
            var comments = await response.Content.ReadFromJsonAsync<Comment[]>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.Equal(expectedComments?.Count, comments?.ToList().Count);
        }

        [Fact]
        public async Task GetComments_ReturnsEmptyComments()
        {
            // Arrange
            var expectedComments = new List<Comment>();
            // query async
            _mockDbConnection.SetupDapperAsync(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(expectedComments!);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/1/comments");

            // Assert
            response.EnsureSuccessStatusCode();
            var comments = await response.Content.ReadFromJsonAsync<Comment[]>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.Equal(expectedComments.Count, comments?.ToList().Count);
        }

        // test for create comment "/{id}/comments" post
        [Fact]
        public async Task PostComment_CreatesComment()
        {
            // Arrange
            var expectedComment = new Comment { PostId = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now, Email = "test@dudes.com" };
           _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/1/comments", expectedComment);

            // Assert
            response.EnsureSuccessStatusCode();
            var comment = await response.Content.ReadFromJsonAsync<Comment>();
            Assert.Equal(expectedComment.Id, comment?.Id);
        }

        // create comment returns validation error
        [Fact]
        public async Task PostComment_ReturnsValidationError()
        {
            // Arrange
            var expectedComment = new Comment { PostId = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/1/comments", expectedComment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseProblem?.Errors.Should().ContainKey("Email");
        }

        // create comment returns bad request
        [Fact]
        public async Task PostComment_RetunsBadRequest()
        {
            // Arrange
            var expectedComment = new Comment { PostId = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now, Email = "test@dudes.com" };
           _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/abc/comments", expectedComment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // test for update comment "/{id}/comments/{id}" put
        [Fact]
        public async Task PutComment_UpdatesComment()
        {
            // Arrange
            var expectedComment = new Comment { Id = 1, PostId = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now, Email = "test@dudes.com" };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(1);
                
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/comments/1", expectedComment);

            // Assert
            response.EnsureSuccessStatusCode();
            var comment = await response.Content.ReadFromJsonAsync<Comment>();
            Assert.Equal(expectedComment.Id, comment?.Id);
        }

        // update comment returns validation error
        [Fact]
        public async Task PutComment_ReturnsValidationError()
        {
            // Arrange
            var expectedComment = new Comment { Id = 1, PostId = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/comments/1", expectedComment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseProblem?.Errors.Should().ContainKey("Email");
        }

        // update comment returns bad request
        [Fact]
        public async Task PutComment_RetunsBadRequest()
        {
            // Arrange
            var expectedComment = new Comment { Id = 1, PostId = 1, Body = "First Comment", Author = "Dotnetdude", Created = DateTime.Now, Email = "test@dudes.com" };
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder => 
            {
                builder.ConfigureServices(services => 
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/comments/abc", expectedComment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // test for delete comment "/{id}/comments/{id}" delete
        [Fact]
        public async Task DeleteComment_DeletesComment()
        {
            // Arrange
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/comments/1");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // delete comment returns bad request
        [Fact]
        public async Task DeleteComment_RetunsBadRequest()
        {
            // Arrange
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(1);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/comments/abc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // delete comment returns not found
        [Fact]
        public async Task DeleteComment_RetunsNotFound()
        {
            // Arrange
            _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                 .ReturnsAsync(0);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/comments/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // test for get comments by post id "/{id}/comments" get
        [Fact]
        public async Task GetCommentsByPostId_ReturnsComments()
        {
            // Arrange
            using FileStream stream = File.OpenRead("./Data/comments.json");
            var expectedComments = await JsonSerializer.DeserializeAsync<List<Comment>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            // query async
            _mockDbConnection.SetupDapperAsync(db => db.QueryAsync<Comment>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .ReturnsAsync(expectedComments!);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockDbConnection.Object);
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/1/comments");

            // Assert
            response.EnsureSuccessStatusCode();
            var comments = await response.Content.ReadFromJsonAsync<Comment[]>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.Equal(expectedComments?.Count, comments?.ToList().Count);
        }
    }
}
