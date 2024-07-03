using Application.Commons;
using Application.ModelViews.UserViewModels;
using Domain.Entities;
using Domain.Entities.Base;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        public Task<List<string>> ChangePasswordAsync(ChangePasswordModel model, string userId);
        public Task<IList<string>> UpdateUserAsync(UserUpdateModel model, string userId);
        public Task<ApplicationUser> GetUserByIdAsync(string userId);
        public Task<UserViewModel> GetUserById(string userId);

        public Task<Pagination<UserViewModel>> GetListUserAsync(int pageIndex = 0, int pageSize = 20);

        public Task<Pagination<UserViewForUserRoleModel>> GetListUserForUserRoleAsync(int pageIndex = 0, int pageSize = 20);
        public Task<string> LockOrUnlockUser(string userId);
        public Task<IList<string>> CreateUserAccount(UserCreateModel model);
        public Task<List<string>> GetListRoleAsync();
        public Task Delete(string role, ApplicationUser user);

    }
}
