using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructures.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructures.Repositories
{
    public class BlogRepository : GenericRepository<Blog>, IBlogRepository
    {
        protected DbSet<Blog> _dbSet;
        public BlogRepository(AppDbContext context, ICurrentTimeService timeService, IClaimsService claimsService) : base(context, timeService, claimsService)
        {
            _dbSet = context.Set<Blog>();
        }
    }
}
