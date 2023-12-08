using Dapper;
using Dotnetdudes.Web.Blog.Api.Models;
using System.Data;

namespace Dotnetdudes.Web.Blog.Api.Tests
{
    internal class DBSetup
    {
        public static void CreateDatabase(IDbConnection db)
        {
            // create posts table if it doesn't exist
            db.Execute("CREATE TABLE IF NOT EXISTS posts (id SERIAL PRIMARY KEY, title VARCHAR(255) NOT NULL, description VARCHAR(255) NOT NULL, body TEXT NOT NULL, author VARCHAR(255) NOT NULL, created TIMESTAMP NOT NULL, updated TIMESTAMP, published TIMESTAMP)");
            // create comments table if it doesn't exist
            db.Execute("CREATE TABLE IF NOT EXISTS comments (id SERIAL PRIMARY KEY, postid INTEGER NOT NULL, body TEXT NOT NULL, author VARCHAR(255) NOT NULL, email VARCHAR(255) NOT NULL, created TIMESTAMP NOT NULL, updated TIMESTAMP, published TIMESTAMP)");
            // create index on postid column in comments table if it doesn't exist
            db.Execute("CREATE INDEX IF NOT EXISTS comments_postid_idx ON comments (postid)");
            // create index on email column in comments table if it doesn't exist
            db.Execute("CREATE INDEX IF NOT EXISTS comments_email_idx ON comments (email)");
            // create foreign key on postid column in comments table if it doesn't exist
            db.Execute("ALTER TABLE comments DROP CONSTRAINT IF EXISTS comments_postid_fkey");
            db.Execute("ALTER TABLE comments ADD CONSTRAINT comments_postid_fkey FOREIGN KEY (postid) REFERENCES posts (id) ON DELETE CASCADE");
        }

        public static void ClearDatabase(IDbConnection db)
        {

            db.Execute("DELETE FROM posts");
            db.Execute("TRUNCATE TABLE comments RESTART IDENTITY CASCADE");
            db.Execute("TRUNCATE TABLE posts RESTART IDENTITY CASCADE");
        }

        public static void SeedDatabase(IDbConnection db)
        {
            var posts = db.Query<Post>("SELECT * FROM posts");
            if (!posts.Any())
            {
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('First Post', 'This is my first post', 'This is the body of my first post', 'Dotnetdude', '2021-01-01')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Second Post', 'This is my second post', 'This is the body of my second post', 'Dotnetdude', '2021-01-02')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Third Post', 'This is my third post', 'This is the body of my third post', 'Dotnetdude', '2021-01-03')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Fourth Post', 'This is my fourth post', 'This is the body of my fourth post', 'Dotnetdude', '2021-01-04')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Fifth Post', 'This is my fifth post', 'This is the body of my fifth post', 'Dotnetdude', '2021-01-05')");
                db.Execute("INSERT INTO posts (title, description, body, author, created) VALUES ('Sixth Post', 'This is my sixth post', 'This is the body of my sixth post', 'Dotnetdude', '2021-01-06')");

                db.Execute("INSERT INTO comments (postid, body, author, email, created) VALUES (1, 'This is the first comment on the first post', 'Dotnetdude','john@doe.com', '2021-01-01')");
                db.Execute("INSERT INTO comments (postid, body, author, email, created) VALUES (1, 'This is the second comment on the first post', 'Dotnetdude','john@doe.com', '2021-01-01')");
                db.Execute("INSERT INTO comments (postid, body, author, email, created) VALUES (1, 'This is the third comment on the first post', 'Dotnetdude','john@doe.com', '2021-01-01')");
                db.Execute("INSERT INTO comments (postid, body, author, email, created) VALUES (2, 'This is the first comment on the second post', 'Dotnetdude','john@doe.com', '2021-01-01')");
            }
        }
    }
}
