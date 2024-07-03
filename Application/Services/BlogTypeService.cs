using Application.Commons;
using Application.Interfaces.Services;
using Application.ModelViews.BlogTypeViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BlogTypeService : IBlogTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BlogTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddBlogType(BlogTypeModel blogTypeModel)
        {
            await isExistedBlogTypeName(blogTypeModel.Name);
            var blogType = _mapper.Map<BlogType>(blogTypeModel);
            try
            {
                await _unitOfWork.BlogTypeRepository.AddAsync(blogType);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }

        public async Task DeleteBlogType(Guid id)
        {
            var result = await _unitOfWork.BlogTypeRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy!");
            var blogs = await _unitOfWork.BlogRepository
                .GetAllQueryable().Where(x => x.BlogTypeId == id && !x.IsDeleted).ToListAsync();
            if (blogs.Count > 0)
            {
                throw new Exception("Còn tồn tại blog thuộc về phân loại này, không thể xóa!");
            }
            try
            {
                _unitOfWork.BlogTypeRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa phan loai. Vui lòng thử lại!");
            }
        }

        public async Task<Pagination<BlogType>> GetBlogTypes()
        {
            var blogTypes = await _unitOfWork.BlogTypeRepository
                .GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return blogTypes;
        }

        public async Task<BlogType?> GetBlogTypeById(Guid id)
        {
            var blogType = await _unitOfWork.BlogTypeRepository
                .GetByIdAsync(id);
            return blogType;
        }

        public async Task UpdateBlogType(Guid id, BlogTypeModel blogTypeModel)
        {
            await isExistedBlogTypeName(blogTypeModel.Name);

            var blogType = _mapper.Map<BlogType>(blogTypeModel);
            blogType.Id = id;
            var result = await _unitOfWork.BlogTypeRepository.GetByIdAsync(blogType.Id);
            if (result == null)
                throw new Exception("Không tìm thấy phân loại!");
            try
            {
                _unitOfWork.BlogTypeRepository.Update(blogType);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }

        private async Task isExistedBlogTypeName(string name)
        {
            var blogTypes = await _unitOfWork.BlogTypeRepository.GetAllQueryable().ToListAsync();

            var matchingBlogTypes = blogTypes
                .Where(c => c.Name.ToLower() == name)
                .ToList();
            if (matchingBlogTypes.Count > 0)
                throw new Exception("Phân loại này đã tồn tại!");
        }
    }
}
