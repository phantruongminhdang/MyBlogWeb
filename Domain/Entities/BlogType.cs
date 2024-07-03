using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BlogType : BaseEntity
    {
        public required string Name { get; set; }
        [JsonIgnore]
        public IList<Blog>? Blogs { get; set; }
    }
}
