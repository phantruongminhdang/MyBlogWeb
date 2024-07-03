using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Blog : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public int OrderNo { get; set; }
        public int Status { get; set; }
        [ForeignKey("BlogType")]
        public Guid BlogTypeId { get; set; }
        [ForeignKey("Blog")]
        public Guid? ParentBlogId { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual BlogType BlogType { get; set; }
        public virtual Blog? ParentBlog { get; set; }
        public virtual User User { get; set; }
    }
}
