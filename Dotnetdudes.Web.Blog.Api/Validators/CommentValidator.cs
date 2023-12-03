using Dotnetdudes.Web.Blog.Api.Models;
using FluentValidation;

namespace Dotnetdudes.Web.Blog.Api.Validators {
    public class CommentValidator : AbstractValidator<Comment>
    {
        public CommentValidator()
        {
            RuleFor(x => x.Body).NotEmpty().WithMessage("Comment is required");
            RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        }
    }
}