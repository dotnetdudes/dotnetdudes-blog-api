using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Dotnetdudes.Web.Blog.Api.Models;
using Dapper;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    public class BlogEndpointsV1Tests : IClassFixture<BlogApplication>, IDisposable
    {
        private BlogApplication _factory;

        public BlogEndpointsV1Tests(BlogApplication factory)
        {
            _factory = factory;
            CreateDatabase();
        }

        public void Dispose()
        {
            // reset database after each test
            ClearDatabase();
            SeedDatabase();
            
        }

        private void CreateDatabase()
        {
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();
            DBSetup.CreateDatabase(db);
        }

        private void SeedDatabase()
        {
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();
            DBSetup.SeedDatabase(db);
        }

        private void ClearDatabase()
        {
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();
            DBSetup.ClearDatabase(db);
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
            using FileStream stream = File.OpenRead("./Data/posts.json");
            var expectedPosts = await JsonSerializer.DeserializeAsync<Post[]>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            // Act
            var response = await client.GetAsync("/posts/v1");
            var actualPosts = await response.Content.ReadFromJsonAsync<Post[]>();

            // Assert
            Assert.Equal(expectedPosts?.Length, actualPosts?.Length);
        }

        [Fact]
        public async void GetPost_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async void GetPost_ReturnsPost()
        {
            // Arrange
            var client = _factory.CreateClient();
            using FileStream stream = File.OpenRead("./Data/post.json");
            var expectedPost = await JsonSerializer.DeserializeAsync<Post>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            // Act
            var response = await client.GetAsync("/posts/v1/1");
            var actualPost = await response.Content.ReadFromJsonAsync<Post>();

            // Assert
            Assert.Equal(expectedPost?.Id, actualPost?.Id);
            Assert.Equal(expectedPost?.Title, actualPost?.Title);
            Assert.Equal(expectedPost?.Description, actualPost?.Description);
            Assert.Equal(expectedPost?.Body, actualPost?.Body);
            Assert.Equal(expectedPost?.Author, actualPost?.Author);
            Assert.Equal(expectedPost?.Created, actualPost?.Created);
            Assert.Equal(expectedPost?.Updated, actualPost?.Updated);
            Assert.Equal(expectedPost?.Published, actualPost?.Published);
        }

        [Fact]
        public async void GetPost_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/100");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void CreatePost_ReturnsCreated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var post = new Post
            {
                Title = "Seventh Post",
                Description = "This is my seventh post",
                Body = "This is the body of my seventh post",
                Author = "Dotnetdude"
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1", post);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async void CreatePost_ReturnsPost()
        {
            // Arrange
            var client = _factory.CreateClient();
            var post = new Post
            {
                Title = "Seventh Post",
                Description = "This is my seventh post",
                Body = "This is the body of my seventh post",
                Author = "Dotnetdude"
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1", post);
            var actualPost = await response.Content.ReadFromJsonAsync<Post>();

            // Assert
            Assert.Equal(post.Title, actualPost?.Title);
            Assert.Equal(post.Description, actualPost?.Description);
            Assert.Equal(post.Body, actualPost?.Body);
            Assert.Equal(post.Author, actualPost?.Author);
        }

        // create post returns validationproblem
        [Fact]
        public async void CreatePost_ReturnsValidationProblem()
        {
            // Arrange
            var client = _factory.CreateClient();
            var post = new Post
            {
                Title = "",
                Description = "",
                Body = "",
                Author = ""
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1", post);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void UpdatePost_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var post = new Post
            {
                Title = "Updated Post",
                Description = "This is my updated post",
                Body = "This is the body of my updated post",
                Author = "Dotnetdude"
            };

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/1", post);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async void UpdatePost_ReturnsPost()
        {
            // Arrange
            var client = _factory.CreateClient();
            var post = new Post
            {
                Title = "Updated Post",
                Description = "This is my updated post",
                Body = "This is the body of my updated post",
                Author = "Dotnetdude"
            };

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/1", post);
            var actualPost = await response.Content.ReadFromJsonAsync<Post>();

            // Assert
            Assert.Equal(post.Title, actualPost?.Title);
            Assert.Equal(post.Description, actualPost?.Description);
            Assert.Equal(post.Body, actualPost?.Body);
            Assert.Equal(post.Author, actualPost?.Author);
        }

        [Fact]
        public async void UpdatePost_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var post = new Post
            {
                Title = "Updated Post",
                Description = "This is my updated post",
                Body = "This is the body of my updated post",
                Author = "Dotnetdude"
            };

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/100", post);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void DeletePost_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/1");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async void DeletePost_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/100");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void GetComments_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/1/comments");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async void GetComments_ReturnsComments()
        {
            // Arrange
            var client = _factory.CreateClient();
            using FileStream stream = File.OpenRead("./Data/comments.json");
            var expectedComments = await JsonSerializer.DeserializeAsync<List<Comment>>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            // Act
            var response = await client.GetAsync("/posts/v1/1/comments");
            var actualComments = await response.Content.ReadFromJsonAsync<List<Comment>>();

            // Assert
            Assert.Equal(expectedComments?.Count, actualComments?.Count);
        }

        [Fact]
        public async void GetComment_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/comments/1");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async void GetComment_ReturnsComment()
        {
            // Arrange
            var client = _factory.CreateClient();
            using FileStream stream = File.OpenRead("./Data/comment.json");
            var expectedComment = await JsonSerializer.DeserializeAsync<Comment>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            // Act
            var response = await client.GetAsync("/posts/v1/comments/1");
            var actualComment = await response.Content.ReadFromJsonAsync<Comment>();

            // Assert
            Assert.Equal(expectedComment?.Id, actualComment?.Id);
            Assert.Equal(expectedComment?.PostId, actualComment?.PostId);
            Assert.Equal(expectedComment?.Body, actualComment?.Body);
            Assert.Equal(expectedComment?.Author, actualComment?.Author);
            Assert.Equal(expectedComment?.Email, actualComment?.Email);
        }

        [Fact]
        public async void GetComment_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/comments/100");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        } 

        // test for "/{id}/post/comments" endpoint -get single post with comments
        [Fact]
        public async void GetPostWithComments_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/posts/v1/1/post/comments");

            // Assert
            response.EnsureSuccessStatusCode();
        }



        [Fact]
        public async void CreateComment_ReturnsCreated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                PostId = 1,
                Body = "This is an added comment on the first post",
                Author = "Dotnetdude",
                Email = "john@doe.com",
                Created = DateTime.UtcNow
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/1/comments", comment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        // create comment returns validationproblem
        [Fact]
        public async void CreateComment_ReturnsValidationProblem()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                PostId = 1,
                Body = "",
                Author = "",
                Email = "",
                Created = DateTime.UtcNow
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/1/comments", comment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async void CreateComment_ReturnsComment()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                PostId = 1,
                Body = "This is an added comment on the first post",
                Author = "Dotnetdude",
                Email = "john@doe.com"
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/1/comments", comment);
            var actualComment = await response.Content.ReadFromJsonAsync<Comment>();

            // Assert
            Assert.Equal(comment.PostId, actualComment?.PostId);
            Assert.Equal(comment.Body, actualComment?.Body);
            Assert.Equal(comment.Author, actualComment?.Author);
            Assert.Equal(comment.Email, actualComment?.Email);
        }

        [Fact]
        public async void CreateComment_ReturnsServerError()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                PostId = 100,
                Body = "This is an added comment on the first post",
                Author = "Dotnetdude",
                Email = "john@doe.com",
                Created = DateTime.UtcNow
            };

            // Act
            var response = await client.PostAsJsonAsync("/posts/v1/100/comments", comment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async void UpdateComment_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                Body = "This is an updated comment on the first post",
                Author = "Dotnetdude",
                Email = "john@doe.com"
            };

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/comments/1", comment);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async void UpdateComment_ReturnsComment()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                Body = "This is an updated comment on the first post",
                Author = "Dotnetdude",
                Email = "john@doe.com"
            };

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/comments/1", comment);
            var actualComment = await response.Content.ReadFromJsonAsync<Comment>();

            // Assert
            Assert.Equal(comment.Body, actualComment?.Body);
            Assert.Equal(comment.Author, actualComment?.Author);
            Assert.Equal(comment.Email, actualComment?.Email);
        }

        [Fact]
        public async void UpdateComment_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var comment = new Comment
            {
                Body = "This is an updated comment on the first post",
                Author = "Dotnetdude",
                Email = "john@doe.com"
            };

            // Act
            var response = await client.PutAsJsonAsync("/posts/v1/comments/100", comment);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async void DeleteComment_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/comments/1");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async void DeleteComment_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.DeleteAsync("/posts/v1/comments/100");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
