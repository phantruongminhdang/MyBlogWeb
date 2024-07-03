using Application.Commons;
using Application.ModelViews.BlogViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IBlogService
    {
        public Task<Pagination<Blog>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false);
        public Task<Pagination<Blog>> GetAll(bool isAdmin = false);
        public Task<Pagination<Blog>?> GetByFilter(int pageIndex, int pageSize, FilterBlogModel filterBlogModel, bool isAdmin = false);
        public Task<Pagination<Blog>?> GetByUserId(int pageIndex, int pageSize, string userId, bool isAdmin = false);
        public Task<Pagination<Blog>?> GetByParentId(int pageIndex, int pageSize, Guid parentId, bool isAdmin = false);
        public Task<Blog?> GetById(Guid blogId, bool isAdmin = false);
        public Task AddAsync(BlogModel blogModel);
        public Task Update(Guid blogId, BlogModel blogModel);
        public Task Delete(Guid blogId);
        public Task<Pagination<Blog>> GetByBlogType(int pageIndex, int pageSize, Guid blogTypeId);
        public Task DisableBlog(Guid id);
    }
}
