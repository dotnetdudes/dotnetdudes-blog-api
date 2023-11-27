using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// add authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001";
        options.Audience = "Dotnetdudes.Web.Blog.Api";
    });
// add postgressql database connection using Dapper
builder.Services.AddScoped<IDbConnection>(provider =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ensure database is created
using var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();
db.Execute("CREATE TABLE IF NOT EXISTS posts (id SERIAL PRIMARY KEY, title VARCHAR(255) NOT NULL, description VARCHAR(255) NOT NULL, body TEXT NOT NULL, author VARCHAR(255) NOT NULL, created TIMESTAMP NOT NULL, updated TIMESTAMP, published TIMESTAMP)");
db.Execute("CREATE TABLE IF NOT EXISTS comments (id SERIAL PRIMARY KEY, postid INTEGER NOT NULL, body TEXT NOT NULL, author VARCHAR(255) NOT NULL, email VARCHAR(255) NOT NULL, created TIMESTAMP NOT NULL, updated TIMESTAMP, published TIMESTAMP)");
// seed database with posts
if (app.Environment.IsDevelopment()) { 
    var posts = db.Query<Post>("SELECT * FROM posts");
    if (!posts.Any())
    {
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('First Post', 'This is my first post', 'This is the body of my first post', 'Dotnetdude', '2021-01-01')");
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Second Post', 'This is my second post', 'This is the body of my second post', 'Dotnetdude', '2021-01-02')");
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Third Post', 'This is my third post', 'This is the body of my third post', 'Dotnetdude', '2021-01-03')");
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Fourth Post', 'This is my fourth post', 'This is the body of my fourth post', 'Dotnetdude', '2021-01-04')");
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Fifth Post', 'This is my fifth post', 'This is the body of my fifth post', 'Dotnetdude', '2021-01-05')");
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Sixth Post', 'This is my sixth post', 'This is the body of my sixth post', 'Dotnetdude', '2021-01-06')");
        db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Seventh Post', 'This is my seventh post', 'This is the body of my seventh post', 'Dotnetdude', '2021-01-07')");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// add route for getting all posts
app.MapGet("/posts", async (IDbConnection db) =>
{
    // get all posts from database
    var posts = await db.QueryAsync<Post>("SELECT * FROM posts");
    return posts;
});

// add route for getting all posts with comments
app.MapGet("/posts/comments", async (IDbConnection db) =>
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
    return posts;
});

// add route for getting a single post
app.MapGet("/posts/{id}", async (IDbConnection db, int id) =>
{
    // get post from database
    var post = await db.QueryFirstOrDefaultAsync<Post>("SELECT * FROM posts WHERE id = @id", new { id });
    return post is null ? Results.NotFound() : TypedResults.Ok(post);
});

// add route for getting a single post with comments
app.MapGet("/posts/{id}/comments", async (IDbConnection db, int id) =>
{
    // get post from database
    var post = await db.QueryFirstOrDefaultAsync<Post>("SELECT * FROM posts WHERE id = @id", new { id });
    // get comments from database
    var comments = await db.QueryAsync<Comment>("SELECT * FROM comments WHERE postid = @id", new { id });
    // check if post is null
    if (post is null)
    {
        return Results.NotFound();
    }
    // set comments property on post
    post.Comments = comments.ToArray();
    return post is null ? Results.NotFound() : TypedResults.Ok(post);
});

// add route for creating a new post
app.MapPost("/posts", async (IDbConnection db, Post post) =>
{
    // insert post into database
    post.Id = await db.QuerySingleAsync<int>("INSERT INTO posts (title, description, body, author, created) VALUES (@Title, @Description, @Body, @Author, @Created) RETURNING id", post);
    return Results.Created($"/posts/{post.Id}", post);
});

// add route for updating a post
app.MapPut("/posts/{id}", async (IDbConnection db, int id, Post post) =>
{
    // set the updated time
    post.Updated = DateTime.Now;
    // update post in database
    var result = await db.ExecuteAsync("UPDATE posts SET title = @Title, description = @Description, body = @Body, author = @Author, updated = @Updated, published = @Published WHERE id = @Id", new { id, post.Title, post.Description, post.Body, post.Author, post.Updated, post.Published });
    return TypedResults.Ok(post);
});

// add route for deleting a post
app.MapDelete("/posts/{id}", async (IDbConnection db, int id) =>
{
    // delete post from database
    var result = await db.ExecuteAsync("DELETE FROM posts WHERE id = @id", new { id });
    return Results.NoContent();
});

// add route for creating a new comment
app.MapPost("/posts/{id}/comments", async (IDbConnection db, int id, Comment comment) =>
{
    // insert comment into database
    var result = await db.ExecuteAsync("INSERT INTO comments (postid, body, author, email, created) VALUES (@PostId, @Body, @Author, @Email, @Created)", new { id, comment.Body, comment.Author, comment.Email, comment.Created });
    return Results.Created($"/posts/{id}/comments/{comment.Id}", comment);
});

// add route for updating a comment
app.MapPut("/posts/{id}/comments/{commentId}", async (IDbConnection db, int id, int commentId, Comment comment) =>
{
    // update comment in database
    var result = await db.ExecuteAsync("UPDATE comments SET body = @Body, author = @Author, email = @Email, updated = @Updated, published = @Published WHERE id = @CommentId", new { id, commentId, comment.Body, comment.Author, comment.Email, comment.Updated, comment.Published });
    return TypedResults.Ok(comment);
});

// add route for deleting a comment
app.MapDelete("/posts/{id}/comments/{commentId}", async (IDbConnection db, int id, int commentId) =>
{
    // delete comment from database
    var result = await db.ExecuteAsync("DELETE FROM comments WHERE id = @commentId", new { commentId });
    return Results.NoContent();
});

app.Run();

public partial class Program { }

