using Microsoft.VisualBasic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dotnetdudes.Web.Blog.Api.Models
{
    // class to represent a blog post
    public class Post
    {
        // generate properties of blog post
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; } // url friendly version of title
        public string Description { get; set; }

        // property for a list of tags
        public string[] Tags { get; set; } = Array.Empty<string>();

        // list of categories
        public string[] Categories { get; set; } = Array.Empty<string>();

        //property for a list of comments
        public Comment[] Comments { get; set; } = Array.Empty<Comment>();

        // content of blog post
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; } // nullable datetime

        public DateTime? Published { get; set; } // nullable datetime

        #pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Post() { }

        // constructor to create a new post
        public Post(string title, string description, string body, string author)
        {
            Title = title;
            Description = description;
            Body = body;
            Author = author;
            Created = DateTime.Now;
            Slug = CreateSlug(title);

        }

        public static string CreateSlug(string title)
        {
            title = Regex.Replace(title.ToLower(), @"[^a-z0-9\s-]", "") ?? string.Empty;
            title = RemoveDiacritics(title);
            title = RemoveReservedUrlCharacters(title);

            return title.ToLowerInvariant();
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string RemoveReservedUrlCharacters(string text)
        {
            var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

            foreach (var chr in reservedCharacters)
            {
                text = text.Replace(chr, string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            return text;
        }
    }
}
