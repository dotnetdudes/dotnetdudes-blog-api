using Moq;
using Moq.Dapper;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Data;
using Dotnetdudes.Web.Blog.Api.Models;
using Dapper;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    public class BlogEndpointsV1Tests : IClassFixture<BlogApplication>
    {
        private BlogApplication _factory;

        public BlogEndpointsV1Tests(BlogApplication factory)
        {
            _factory = factory;
        }

       // [Fact]
        //public async void GetPosts_ShouldReturnOk()
        //{
        //    var expected = new Post[] { new Post() { Id = 1, Title = "First Post", Body = "Post body text", Author = "John Morton", Created = DateTime.MinValue, Description = "First post description" } };
        //    connection.SetupDapper(c => c.Query<Post>(It.IsAny<string>(), null, null, false, null, null))
        //    .Returns(expected);

        //    using var client = _factory.CreateClient();

        //    var response = await client.GetAsync("/posts/v1");
        //    var posts = await response.Content.ReadFromJsonAsync<Post[]>();
        //    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        //}

        [Fact]
        public async Task GetPostById_ReturnsPost()
        {
            // Arrange
            var _mockDbConnection = new Mock<IDbConnection>();
            var expectedPost = new Post { Id = 1, Title = "First Post" };
           

            //var client = _factory
            //    .WithWebHostBuilder(builder =>
            //    {
            //        builder.ConfigureServices(services =>
            //        {
            //            services.AddScoped<IDbConnection>(provider => _mockDbConnection.Object);
            //        });
            //    })
            //    .CreateClient();

            //// Act
            //var response = await client.GetAsync("/posts/1");

            //// Assert
            /// Console.WriteLine("ere");
            //response.EnsureSuccessStatusCode();
            //var post = await response.Content.ReadFromJsonAsync<Post>();
            //Assert.Equal(expectedPost.Id, post?.Id);
        }
    }
}
