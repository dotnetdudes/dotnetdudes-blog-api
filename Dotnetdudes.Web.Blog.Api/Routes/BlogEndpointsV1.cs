using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
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
            group.MapGet("/comments", async (IDbConnection db) =>
            {
                // get all posts from database
                var posts = await db.QueryAsync<Post>("SELECT * FROM posts");
                // get all comments from database
                var comments = await db.QueryAsync<Comment>("SELECT * FROM comments");
                // loop through each post
                foreach (var post in posts)
                {
                    // get comments for post
                    var postComments = comments.Where(c => c.PostId == post.Id).ToArray();
                    // set comments property on post
                    post.Comments = postComments;
                }
                return TypedResults.Json(posts);
            });

            // add route for getting a single post
            group.MapGet("/{id}", async Task<Results<Ok<Post>, NotFound>> (IDbConnection db, int id) =>
            {
                // get post from database
                var post = await db.QueryFirstOrDefaultAsync<Post>("SELECT * FROM posts WHERE id = @id", new { id });
                return post is null ? TypedResults.NotFound() : TypedResults.Ok(post);
            });

            // add route for getting a single post with comments
            group.MapGet("/{id}/comments", async Task<Results<Ok<Post>, NotFound>> (IDbConnection db, int id) =>
            {
                // get post from database
                var post = await db.QueryFirstOrDefaultAsync<Post>("SELECT * FROM posts WHERE id = @id", new { id });
                // get comments from database
                var comments = await db.QueryAsync<Comment>("SELECT * FROM comments WHERE postid = @id", new { id });
                // check if post is null
                if (post is null)
                {
                    return TypedResults.NotFound();
                }
                // set comments property on post
                post.Comments = comments.ToArray();
                return post is null ? TypedResults.NotFound() : TypedResults.Ok(post);
            });

            // add route for creating a new post
            group.MapPost("/", async (IDbConnection db, Post post) =>
            {
                // insert post into database
                post.Id = await db.QuerySingleAsync<int>("INSERT INTO posts (title, description, body, author, created) VALUES (@Title, @Description, @Body, @Author, @Created) RETURNING id", post);
                return TypedResults.Created($"/posts/{post.Id}", post);
            });

            // add route for updating a post
            group.MapPut("/{id}", async Task<Results<Ok<Post>, NotFound>> (IDbConnection db, int id, Post post) =>
            {
                // set the updated time
                post.Updated = DateTime.Now;
                // update post in database
                var result = await db.ExecuteAsync("UPDATE posts SET title = @Title, description = @Description, body = @Body, author = @Author, updated = @Updated, published = @Published WHERE id = @Id", new { id, post.Title, post.Description, post.Body, post.Author, post.Updated, post.Published });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(post);
            });

            // add route for deleting a post
            group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (IDbConnection db, int id) =>
            {
                // delete post from database
                var result = await db.ExecuteAsync("DELETE FROM posts WHERE id = @id", new { id });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            });

            // add route for getting a single comment
            group.MapGet("/{id}/comments/{commentId}", async Task<Results<Ok<Comment>, NotFound>> (IDbConnection db, int id, int commentId) =>
            {
                // get comment from database
                var comment = await db.QueryFirstOrDefaultAsync<Comment>("SELECT * FROM comments WHERE id = @commentId", new { commentId });
                return comment is null ? TypedResults.NotFound() : TypedResults.Ok(comment);
            });

            // add route for creating a new comment
            group.MapPost("/{id}/comments", async Task<Results<Created<Comment>, NotFound>> (IDbConnection db, int id, Comment comment) =>
            {
                // insert comment into database
                var result = await db.ExecuteAsync("INSERT INTO comments (postid, body, author, email, created) VALUES (@PostId, @Body, @Author, @Email, @Created)", new { comment.PostId, comment.Body, comment.Author, comment.Email, comment.Created });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Created($"/{id}/comments/{comment.Id}", comment);
            });

            // add route for updating a comment
            group.MapPut("/{id}/comments/{commentId}", async Task<Results<Ok<Comment>, NotFound>>  (IDbConnection db, int id, int commentId, Comment comment) =>
            {
                // update comment in database
                var result = await db.ExecuteAsync("UPDATE comments SET body = @Body, author = @Author, email = @Email, updated = @Updated, published = @Published WHERE id = @CommentId", new { id, commentId, comment.Body, comment.Author, comment.Email, comment.Updated, comment.Published });
                if (result == 0)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(comment);
            });

            // add route for deleting a comment
            group.MapDelete("/{id}/comments/{commentId}", async Task<Results<NoContent, NotFound>> (IDbConnection db, int id, int commentId) =>
            {
                // delete comment from database
                var result = await db.ExecuteAsync("DELETE FROM comments WHERE id = @commentId", new { commentId });
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
