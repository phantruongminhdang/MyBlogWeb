using Application.ModelViews.BlogViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Blog
{
    public class BlogModelValidator : AbstractValidator<BlogModel>
    {
        public BlogModelValidator()
        {
            RuleFor(x => x.BlogTypeId).NotEmpty().WithMessage("Danh mục không được để trống.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(100).WithMessage("Tên không quá 100 ký tự.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống.")
                .MaximumLength(2000).WithMessage("Mô tả không được quá 2000 ký tự.");
        }
    }
}
