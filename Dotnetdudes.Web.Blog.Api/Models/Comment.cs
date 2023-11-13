using System.Security.Cryptography;
using System.Text;

namespace Dotnetdudes.Web.Blog.Api.Models
{
    public class Comment
    {
        // properties of a comment
        public int Id { get; set; }

        // foreign key to post
        public int PostId { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; } // nullable datetime
        public DateTime? Published { get; set; } // nullable datetime

        // constructor to create a new comment
        public Comment () { }
        public Comment(string body, string author, string email)
        {
            Body = body;
            Author = author;
            Email = email;
            Created = DateTime.Now;
        }

        // get gravatar function to get the gravatar image url
        public string GetGravatar()
        {
            // create md5 hash of email address
            var md5Hash = MD5.Create();
            var hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(Email));

            // create stringbuilder to hold the bytes
            var stringBuilder = new StringBuilder();

            // loop through each byte of the hashed data and format each one as a hexadecimal string
            foreach (var t in hash)
            {
                stringBuilder.Append(t.ToString("x2"));
            }

            // return the hexadecimal string
            return $"https://www.gravatar.com/avatar/{stringBuilder}";
        }

    }
}
