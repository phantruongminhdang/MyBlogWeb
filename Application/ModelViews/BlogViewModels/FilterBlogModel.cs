using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ModelViews.BlogViewModels
{
    public class FilterBlogModel
    {
        public string? Keyword { get; set; }

        public string? UserId { get; set; }
        public string? BlogTypeId { get; set; }
    }
}
