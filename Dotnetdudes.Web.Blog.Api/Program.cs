using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using Dotnetdudes.Web.Blog.Api.Routes;
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

// todoV1 endpoints
app.MapGroup("/posts/v1")
    .MapBlogEndpointsV1()
    .WithTags("Blog V1 Endpoints");

app.Run();

public partial class Program { }
