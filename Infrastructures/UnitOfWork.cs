using Application;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructures
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserRepository _userRepository;
        private readonly IBlogRepository _blogRepository;
        private readonly IBlogTypeRepository _blogTypeRepository;
        private IDbContextTransaction _transaction;

        public UnitOfWork(AppDbContext dbContext, IUserRepository userRepository,
            IBlogRepository productRepository, IBlogTypeRepository categoryRepository
         )
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _blogRepository = productRepository;
            _blogTypeRepository = categoryRepository;
        }

        public IUserRepository UserRepository => _userRepository;

        public IBlogRepository BlogRepository => _blogRepository;

        public IBlogTypeRepository BlogTypeRepository => _blogTypeRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
        public void BeginTransaction()
        {
            _transaction = _dbContext.Database.BeginTransaction();
        }
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
        }
        public void RollbackTransaction()
        {
            _transaction.Rollback();
        }
        public void ClearTrack()
        {
            _dbContext.ChangeTracker.Clear();
        }
    }
}
