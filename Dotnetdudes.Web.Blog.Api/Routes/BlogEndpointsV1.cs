using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;

namespace Dotnetdudes.Web.Blog.Api.Routes
{
    public static class BlogEndpointsV1
    {
        public static RouteGroupBuilder MapBlogEndpointsV1(this RouteGroupBuilder group)
        {
            // add route for getting all posts
            group.MapGet("/", async (IDbConnection db) =>
            {
                // get all posts from database
                var posts = await db.QueryAsync<Post>("SELECT * FROM posts");
                return TypedResults.Json(posts);
            });

            // add route for getting all posts with comments
            group.MapGet("/posts/comments", async (IDbConnection db) =>
            {
                // get all posts from database
                var posts = await db.QueryAsync<Post>("SELECT * FROM posts");
                // get all comments from database
                var comments = await db.QueryAsync<Comment>("SELECT * FROM comments");
                // loop through each post
                foreach (var post in posts)
                {
                    // get comments for post
                    List<Comment> postComments = comments.Where(c => c.PostId == post.Id).ToList<Comment>();
                    // set comments property on post
                    post.Comments = postComments;
                }
                return TypedResults.Json(posts);
            });

            // add route for getting all comments
            group.MapGet("/comments", async (IDbConnection db) =>
            {
                // get all comments from database
                var comments = await db.QueryAsync<Comment>("SELECT * FROM comments");
                return TypedResults.Json(comments);
            });


            // add route for getting a single post
            group.MapGet("/{id}", async Task<Results<Ok<Post>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // get post from database
                var post = await db.QueryFirstOrDefaultAsync<Post>("SELECT * FROM posts WHERE id = @number", new { number });
                return post is null ? TypedResults.NotFound() : TypedResults.Ok(post);
            });

            // get comments for post
            group.MapGet("/{id}/comments", async Task<Results<Ok<List<Comment>>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // get comments from database
                var comments = await db.QueryAsync<Comment>("SELECT * FROM comments WHERE postid = @number", new { number });
                return TypedResults.Ok(comments.ToList<Comment>());
            });

            // get single post with comments
            group.MapGet("/{id}/post/comments", async Task<Results<Ok<Post>, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // get post from database
                var post = await db.QueryFirstOrDefaultAsync<Post>("SELECT * FROM posts WHERE id = @number", new { number });
                if (post is null)
                {
                    return TypedResults.NotFound();
                }
                // get comments from database
                var comments = await db.QueryAsync<Comment>("SELECT * FROM comments WHERE postid = @number", new { number });
                // set comments property on post
                post.Comments = comments.ToList<Comment>();
                return TypedResults.Ok(post);
            });

            // add route for creating a new post
            group.MapPost("/", async Task<Results<Created<Post>, NotFound, ValidationProblem>> (IValidator<Post> validator, IDbConnection db, Post post) =>
            {
                // validate post
                var validationResult = await validator.ValidateAsync(post);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert post into database
                post.Id = await db.QuerySingleAsync<int>("INSERT INTO posts (title, description, body, author, created) VALUES (@Title, @Description, @Body, @Author, @Created) RETURNING id", post);
                return TypedResults.Created($"/posts/{post.Id}", post);
            });

            // add route for updating a post
            group.MapPut("/{id}", async Task<Results<Ok<Post>, NotFound, ValidationProblem, BadRequest>> (IValidator<Post> validator, IDbConnection db, string id, Post post) =>
            {
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // set the updated time
                post.Updated = DateTime.Now;
                // validate post
                var validationResult = await validator.ValidateAsync(post);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update post in database
                var result = await db.ExecuteAsync("UPDATE posts SET title = @Title, description = @Description, body = @Body, author = @Author, updated = @Updated WHERE id = @number", new { number, post.Title, post.Description, post.Body, post.Author, post.Updated });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(post);
            });

            // add route for deleting a post
            group.MapDelete("/{id}", async Task<Results<NoContent, NotFound, BadRequest>> (IDbConnection db, string id) =>
            {
                bool success = int.TryParse(id, out int number);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                if (number < 1)
                {
                    return TypedResults.BadRequest();
                }
                // delete post from database
                var result = await db.ExecuteAsync("DELETE FROM posts WHERE id = @number", new { number });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            });

            // add route for getting a single comment
            group.MapGet("/comments/{commentId}", async Task<Results<Ok<Comment>, NotFound, BadRequest>> (IDbConnection db, string commentId) =>
            {
                bool success = int.TryParse(commentId, out int commentNumber);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // get comment from database
                var comment = await db.QueryFirstOrDefaultAsync<Comment>("SELECT * FROM comments WHERE id = @commentNumber", new { commentNumber });
                return comment is null ? TypedResults.NotFound() : TypedResults.Ok(comment);
            });

            // add route for creating a new comment
            group.MapPost("/{id}/comments", async Task<Results<Created<Comment>, NotFound, ValidationProblem, BadRequest>> (IValidator<Comment> validator, IDbConnection db, string id, Comment comment) =>
            {
                bool success = int.TryParse(id, out int postId);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // validate comment
                var validationResult = await validator.ValidateAsync(comment);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // insert comment into database
                var result = await db.ExecuteAsync("INSERT INTO comments (postid, body, author, email, created) VALUES (@PostId, @Body, @Author, @Email, @Created)", new { postId, comment.Body, comment.Author, comment.Email, comment.Created });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Created($"/{id}/comments/{comment.Id}", comment);
            });

            // add route for updating a comment
            group.MapPut("/comments/{commentId}", async Task<Results<Ok<Comment>, NotFound, BadRequest, ValidationProblem>> (IValidator<Comment> validator, IDbConnection db, string commentId, Comment comment) =>
            {
                bool success = int.TryParse(commentId, out int commentNumber);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // set the updated time
                comment.Updated = DateTime.Now;
                // validate comment
                var validationResult = await validator.ValidateAsync(comment);
                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
                // update comment in database
                var result = await db.ExecuteAsync("UPDATE comments SET body = @Body, author = @Author, email = @Email, updated = @Updated, published = @Published WHERE id = @CommentNumber", new { comment.Body, comment.Author, comment.Email, comment.Updated, comment.Published, commentNumber });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(comment);
            });

            // add route for deleting a comment
            group.MapDelete("/comments/{commentId}", async Task<Results<NoContent, NotFound, BadRequest>> (IDbConnection db, string commentId) =>
            {
                bool success = int.TryParse(commentId, out int commentNumber);
                if (!success)
                {
                    return TypedResults.BadRequest();
                }
                // delete comment from database
                var result = await db.ExecuteAsync("DELETE FROM comments WHERE id = @commentNumber", new { commentNumber });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            });
            return group;
        }
    }
}
