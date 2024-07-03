using Application.Commons;
using Application.ModelViews.BlogTypeViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IBlogTypeService
    {
        Task<Pagination<BlogType>> GetBlogTypes();
        Task<BlogType?> GetBlogTypeById(Guid id);
        Task AddBlogType(BlogTypeModel blogTypeModel);
        Task UpdateBlogType(Guid id, BlogTypeModel blogTypeModel);
        Task DeleteBlogType(Guid id);
    }
}
