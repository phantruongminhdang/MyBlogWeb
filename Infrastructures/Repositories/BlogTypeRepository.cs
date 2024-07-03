using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructures.Repositories.Base;

namespace Infrastructures.Repositories
{
    public class BlogTypeRepository : GenericRepository<BlogType>, IBlogTypeRepository
    {
        public BlogTypeRepository(AppDbContext context, ICurrentTimeService timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {
        }
    }
}
