using Dotnetdudes.Web.Blog.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Web.Blog.Api.Validators
{

    public class PostValidator : AbstractValidator<Post>
    {
        public PostValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
            RuleFor(x => x.Body).NotEmpty().WithMessage("Content is required");
            RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required");
        }
    }
}