using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

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
                new Post { Id = 1, Title = "Post 1", Body = "Content 1", Author="Dotnetdude" },
                new Post { Id = 2, Title = "Post 2", Body = "Content 2", Author="Dotnetdude" },
                new Post { Id = 3, Title = "Post 3", Body = "Content 3", Author="Dotnetdude" },
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
            var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Author = "Dotnetdude" };
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

        // [Fact]
        // public async Task PutPost_ReturnsOk()
        // {
        //     // fix this test

        //     // arrange
        //     var post = new Post { Id = 1, Title = "Post 1", Body = "Content 1", Description = "Post description", Author = "Dotnetdude" };
        //     _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
        //         .ReturnsAsync(1);
        //     _mockDbConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<Post>(It.IsAny<string>(), null, null, null, null))
        //         .ReturnsAsync(post);
        //     var client = _factory.WithWebHostBuilder(builder =>
        //     {
        //         builder.ConfigureServices(services =>
        //         {
        //             services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
        //         });
        //     }).CreateClient();

        //     // act
        //     var response = await client.PutAsJsonAsync("/posts/v1/1", post);

        //     // assert
        //     response.StatusCode.Should().Be(HttpStatusCode.OK);
        //     var responsePost = await response.Content.ReadFromJsonAsync<Post>();
        //     responsePost.Should().BeEquivalentTo(post);
        // }

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

    }
}
