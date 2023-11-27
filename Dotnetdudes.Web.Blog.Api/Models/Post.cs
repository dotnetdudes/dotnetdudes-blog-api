using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string Title { get; set; } = String.Empty;
        public string Slug { get { return CreateSlug(Title); } } // url friendly version of title
        [Required]
        public string Description { get; set; } = String.Empty;

        // property for a list of tags
        public string[] Tags { get; set; } = Array.Empty<string>();

        // list of categories
        public string[] Categories { get; set; } = Array.Empty<string>();

        //property for a list of comments
        public Comment[] Comments { get; set; } = Array.Empty<Comment>();

        // content of blog post
        [Required]
        public string Body { get; set; } = String.Empty;
        [Required]
        public string Author { get; set; } = String.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Updated { get; set; } // nullable datetime

        public DateTime? Published { get; set; } // nullable datetime

        public Post() { }

        // constructor to create a new post
        public Post(string title, string description, string body, string author)
        {
            Title = title;
            Description = description;
            Body = body;
            Author = author;

        }

        public static string CreateSlug(string title)
        {
            title = Regex.Replace(title.ToLower(), @"[^a-z0-9\s-]", "") ?? string.Empty;
            title = title.Replace(" ", "-");
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
