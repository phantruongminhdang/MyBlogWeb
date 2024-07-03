using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ModelViews.BlogViewModels
{
    public class BlogModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public Guid BlogTypeId { get; set; }
        public Guid? ParentBlogId { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; } = default!;
    }
}
