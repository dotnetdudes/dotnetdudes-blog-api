﻿using Dotnetdudes.Web.Blog.Api.Models;
using System.Data;
using Dapper;

namespace Dotnetdudes.Web.Blog.Api
{
    public class DbInitialiser
    {
        // create static method to initialise database
        public static void Initialise(WebApplication app)
        {
            using var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<IDbConnection>();
            // create posts table if it doesn't exist
            db.Execute("CREATE TABLE IF NOT EXISTS posts (id SERIAL PRIMARY KEY, title VARCHAR(255) NOT NULL, description VARCHAR(255) NOT NULL, body TEXT NOT NULL, author VARCHAR(255) NOT NULL, created TIMESTAMP NOT NULL, updated TIMESTAMP, published TIMESTAMP)");
            // create comments table if it doesn't exist
            db.Execute("CREATE TABLE IF NOT EXISTS comments (id SERIAL PRIMARY KEY, postid INTEGER NOT NULL, body TEXT NOT NULL, author VARCHAR(255) NOT NULL, email VARCHAR(255) NOT NULL, created TIMESTAMP NOT NULL, updated TIMESTAMP, published TIMESTAMP)");
            // seed database with posts
            var posts = db.Query<Post>("SELECT * FROM posts");
            if (!posts.Any())
            {
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('First Post', 'This is my first post', 'This is the body of my first post', 'Dotnetdude', '2021-01-01')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Second Post', 'This is my second post', 'This is the body of my second post', 'Dotnetdude', '2021-01-02')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Third Post', 'This is my third post', 'This is the body of my third post', 'Dotnetdude', '2021-01-03')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Fourth Post', 'This is my fourth post', 'This is the body of my fourth post', 'Dotnetdude', '2021-01-04')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Fifth Post', 'This is my fifth post', 'This is the body of my fifth post', 'Dotnetdude', '2021-01-05')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Sixth Post', 'This is my sixth post', 'This is the body of my sixth post', 'Dotnetdude', '2021-01-06')");
            }
        }
    }
}