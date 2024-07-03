using Application.Commons;
using Application.Interfaces.Services;
using Application.ModelViews.BlogViewModels;
using Application.Utils;
using Application.Validations.Blog;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Services
{
    public class BlogService : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseService _fireBaseService;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claims;

        public BlogService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService, IClaimsService claims)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
            _claims = claims;
        }

        public async Task AddAsync(BlogModel blogModel)
        {
            var userId = _claims.GetCurrentUserId.ToString();
            var user = await GetUserAsync(userId);
            if (blogModel == null)
               throw new ArgumentNullException(nameof(blogModel), "Chưa điền đủ thông tin!");
            var validation = new BlogModelValidator();
            var validationResult = await validation.ValidateAsync(blogModel);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            if (blogModel.Image == null)
                throw new Exception("Vui lòng thêm hình ảnh");
            var blogType = await _unitOfWork.BlogTypeRepository.GetByIdAsync(blogModel.BlogTypeId);
            if (blogModel == null)
                throw new Exception("Không tìm thấy danh mục!");
            var blog = _mapper.Map<Blog>(blogModel);
            blog.Status = 1;
            blog.OrderNo = await generateOrderNo();
            blog.UserId = user.Id;
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.BlogRepository.AddAsync(blog);
                if (blogModel.Image != null)
                {
                    string newImageName = blog.Id + "_i";
                    string folderName = $"Blog/{blog.Id}/Image";
                    string imageExtension = Path.GetExtension(blogModel.Image.FileName);
                    //Kiểm tra xem có phải là file ảnh không.
                    string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    const long maxFileSize = 20 * 1024 * 1024;
                    if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || blogModel.Image.Length > maxFileSize)
                    {
                        throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                    }
                    var url = await _fireBaseService.UploadFileToFirebaseStorage(blogModel.Image, newImageName, folderName);
                    if (url == null)
                        throw new Exception("Lỗi khi đăng ảnh lên Firebase!");

                    blog.ImageUrl = url;

                    await _unitOfWork.BlogRepository.AddAsync(blog);
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task Delete(Guid blogId)
        {
            var result = await _unitOfWork.BlogRepository.GetByIdAsync(blogId);
            if(result == null)
            {
                throw new Exception("Không tìm thấy!");
            }
            try
            {
                _unitOfWork.BlogRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa Product. Vui lòng thử lại!");
            }
        }

        public async Task DisableBlog(Guid id)
        {
            var result = await _unitOfWork.BlogRepository.GetByIdAsync(id);
            if (result == null)
            {
                throw new Exception("Không tìm thấy!");
            }
            if (result.Status == 1)
                result.Status = 2;
            else
                result.Status = 1;
            _unitOfWork.BlogRepository.Update(result);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<Pagination<Blog>> GetAll(bool isAdmin = false)
        {
            Pagination<Blog> blogs;
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            if (isAdmin)
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Status == 1,
                isDisableTracking: true, includes: includes);
            }

            return blogs;
        }

        public async Task<Pagination<Blog>> GetByBlogType(int pageIndex, int pageSize, Guid blogTypeId)
        {
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            var blogs = await _unitOfWork.BlogRepository
                .GetAsync(
                pageIndex: pageIndex, 
                pageSize: pageSize, 
                expression: x => !x.IsDeleted && x.Status == 1 && x.BlogTypeId == blogTypeId,
                isDisableTracking: true, includes: includes
                );
            return blogs;
        }

        public async Task<Pagination<Blog>?> GetByFilter(int pageIndex, int pageSize, FilterBlogModel filterBlogModel, bool isAdmin = false)
        {
            if (filterBlogModel.Keyword != null && filterBlogModel.Keyword.Length > 50)
            {
                throw new Exception("Từ khóa phải dưới 50 kí tự");
            }
            var filter = new List<Expression<Func<Blog, bool>>>();
            filter.Add(x => !x.IsDeleted);
            if (!isAdmin)
            {
                filter.Add(x => x.Status == 1);
            }
            if (filterBlogModel.Keyword != null)
            {
                filter.Add(x => x.Title.ToLower().Contains(filterBlogModel.Keyword.ToLower()));
            }
            if (filterBlogModel.UserId != null && filterBlogModel.UserId != "")
            {
                var user = await GetUserAsync(filterBlogModel.UserId);
                filter.Add(x => x.UserId == user.Id);
            }
            if (filterBlogModel.BlogTypeId != null && filterBlogModel.BlogTypeId != "")
            {
                filter.Add(x => x.BlogTypeId == Guid.Parse(filterBlogModel.BlogTypeId));
            }
            var finalFilter = filter.Aggregate((current, next) => current.AndAlso(next));
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            var blogs = await _unitOfWork.BlogRepository
                .GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: finalFilter,
                isDisableTracking: true, includes: includes);
            return blogs;
        }

        public async Task<Pagination<Blog>?> GetByUserId(int pageIndex, int pageSize, string userId, bool isAdmin = false)
        {
            if (userId == null)
            {
                throw new Exception("Hãy chọn người dùng!");
            }
            var user = await GetUserAsync(userId);
            Pagination<Blog> blogs;
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            if (isAdmin)
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.UserId == user.Id,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Status == 1 && x.UserId == user.Id,
                isDisableTracking: true, includes: includes);
            }
            return blogs;
        }

        public async Task<Pagination<Blog>?> GetByParentId(int pageIndex, int pageSize, Guid parentId, bool isAdmin = false)
        {
            if (parentId == null)
            {
                throw new Exception("Chưa nhập Blog!");
            }
            Pagination<Blog> blogs;
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            if (isAdmin)
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ParentBlogId == parentId,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Status == 1 && x.ParentBlogId == parentId,
                isDisableTracking: true, includes: includes);
            }
            return blogs;
        }
        public async Task<Blog?> GetById(Guid blogId, bool isAdmin = false)
        {
            Pagination<Blog> blogs;
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            if (isAdmin)
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Id == blogId,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Status == 1 && x.Id == blogId,
                isDisableTracking: true, includes: includes);
            }
            return blogs.Items[0];
        }

        public async Task<Pagination<Blog>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false)
        {
            Pagination<Blog> blogs;
            List<Expression<Func<Blog, object>>> includes = new List<Expression<Func<Blog, object>>>{
                                 x => x.BlogType,
                                    };
            if (isAdmin)
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                blogs = await _unitOfWork.BlogRepository
                    .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Status == 1,
                isDisableTracking: true, includes: includes);
            }
            return blogs;
        }

        public async Task Update(Guid blogId, BlogModel blogModel)
        {
            var result = await _unitOfWork.BlogRepository.GetByIdAsync(blogId);
            if (result == null)
                throw new Exception("Không tìm thấy Product!");
            if (blogModel == null)
                throw new ArgumentNullException(nameof(blogModel), "Vui lòng điền đầy đủ thông tin!");
            var validationRules = new BlogModelValidator();
            var validationResult = await validationRules.ValidateAsync(blogModel);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            if (blogModel.Image == null)
                throw new Exception("Vui lòng thêm hình ảnh");
            var blog = _mapper.Map<Blog>(blogModel);
            blog.Id = blogId;
            blog.UserId = result.UserId;
            blog.Status = 1;
            blog.OrderNo = await generateOrderNo();
            try
            {
                _unitOfWork.BeginTransaction();
                if (blogModel.Image != null)
                {
                    string newImageName = blog.Id + "_i";
                    string folderName = $"Blog/{blog.Id}/Image";
                    string imageExtension = Path.GetExtension(blogModel.Image.FileName);
                    //Kiểm tra xem có phải là file ảnh không.
                    string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    const long maxFileSize = 20 * 1024 * 1024;
                    if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || blogModel.Image.Length > maxFileSize)
                    {
                        throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                    }
                    var url = await _fireBaseService.UploadFileToFirebaseStorage(blogModel.Image, newImageName, folderName);
                    if (url == null)
                        throw new Exception("Lỗi khi đăng ảnh lên Firebase!");

                    blog.ImageUrl = url;

                    _unitOfWork.BlogRepository.Update(blog);
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<int> generateOrderNo()
        {
            int newOrderNo = 0;
            var lastCountBlog = await _unitOfWork.BlogRepository.CountAsync();
            if (lastCountBlog > 0)
            {
                newOrderNo = lastCountBlog++;
                return newOrderNo;
            }
            else
            {
                newOrderNo = 1;
                return newOrderNo;
            }
        }

        private async Task<User> GetUserAsync(string? userId)
        {
            var user = await _unitOfWork.UserRepository.GetAllQueryable()
                .FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(userId.ToLower()));
            if (user == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return user;
        }
    }
}
