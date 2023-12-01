using Moq;
using Moq.Dapper;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Data;
using Dotnetdudes.Web.Blog.Api.Models;
using Dapper;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    public class BlogEndpointsV1Tests : IClassFixture<BlogApplication>
    {
        private BlogApplication _factory;

        public BlogEndpointsV1Tests(BlogApplication factory)
        {
            _factory = factory;
        }

        [Fact]
        public async void GetPosts_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async void GetPosts_ReturnsPosts()
        {
            // Arrange
            var client = _factory.CreateClient();
            var expectedPosts = await System.Text.Json.JsonSerializer.DeserializeAsync<Post[]>(
                System.IO.File.OpenRead("posts.json"));

            // Act
            var response = await client.GetAsync("/posts/v1");
            var actualPosts = await response.Content.ReadFromJsonAsync<Post[]>();

            // Assert
            Assert.Equal(expectedPosts, actualPosts);
        }
    }
}
