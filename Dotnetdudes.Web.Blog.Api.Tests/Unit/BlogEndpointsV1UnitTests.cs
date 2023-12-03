using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using FluentAssertions;
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
    public class BlogEndpointsV1UnitTests
    {

        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IDbConnection> _mockDbConnection;

        public BlogEndpointsV1UnitTests()
        {
            _factory = new WebApplicationFactory<Program>();
            _mockDbConnection = new Mock<IDbConnection>();
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
                .ReturnsAsync((Post?)null);
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
                });
            }).CreateClient();

            // act
            var response = await client.GetAsync("/api/posts/v1/1");

            // assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

    }
}
