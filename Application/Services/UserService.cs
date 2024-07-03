using Application.Commons;
using Application.Interfaces.Services;
using Application.ModelViews.UserViewModels;
using Application.Validations.User;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTimeService _currentTimeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClaimsService _claims;
        private readonly IFirebaseService _fireBaseService;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTimeService currentTimeService,
            UserManager<ApplicationUser> userManager,
            IClaimsService claims,
            IFirebaseService fireBaseService,
            RoleManager<IdentityRole> roleManager
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTimeService = currentTimeService;
            _userManager = userManager;
            _claims = claims;
            _fireBaseService = fireBaseService;
            _roleManager = roleManager;
        }
        public async Task<List<string>> ChangePasswordAsync(ChangePasswordModel model, string userId)
        {
            if (!model.NewPassword.Equals(model.ConfirmPassword))
            {
                throw new Exception("Mật khẩu xác nhận không trùng khớp!");

            }
            if (model.NewPassword.Equals(model.OldPassword))
            {
                throw new Exception("Mật khẩu mới phải khác mật khẩu cũ!");
            }
            try
            {

                var user = await _userManager.FindByIdAsync(userId);
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return null;
                }
                else
                {
                    List<string> err = new List<string>();
                    foreach (var item in result.Errors)
                    {
                        err.Add(item.Description);
                    }
                    return err;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public async Task<IList<string>> CreateUserAccount(UserCreateModel model)
        {
            var validateResult = await isUserCreateModelValidationAsync(model);
            if (validateResult != null)
            {
                return validateResult;
            }
            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null)
            {
                throw new Exception("Địa chỉ email này đã được sử dụng!");
            }
            else
            {
                var temp = await _userManager.Users.Where(x => x.UserName.ToLower().Equals(model.UserName.ToLower())).FirstOrDefaultAsync();
                if (temp != null)
                    throw new Exception("Tên đăng nhập này đã được sử dụng!");
                try
                {
                    string url = null;
                    ApplicationUser newUser = new ApplicationUser()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        Fullname = model.Fullname,
                        PhoneNumber = model.PhoneNumber,
                        IsRegister = true
                    };
                    if (model.Avatar != null)
                    {
                        Random random = new Random();
                        string newImageName = newUser.Id + "_i" + model.Avatar.Name.Trim() + random.Next(1, 10000).ToString();
                        string folderName = $"user/{newUser.Id}/Image";
                        string imageExtension = Path.GetExtension(model.Avatar.FileName);
                        //Kiểm tra xem có phải là file ảnh không.
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        const long maxFileSize = 20 * 1024 * 1024;
                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || model.Avatar.Length > maxFileSize)
                        {
                            throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                        }
                        url = await _fireBaseService.UploadFileToFirebaseStorage(model.Avatar, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");
                    }
                    newUser.AvatarUrl = url;
                    var result = await _userManager.CreateAsync(newUser, "NewAccount1!");
                    if (result.Succeeded)
                    {
                        var tempUser = await _userManager.FindByIdAsync(newUser.Id);
                        try
                        {
                            await CreateAccountAsync(tempUser, model.Role);
                            return null;
                        }
                        catch (Exception ex)
                        {
                            await _userManager.DeleteAsync(tempUser);
                            throw new Exception("Đã xảy ra lỗi trong quá trình tạo tài khoản: " + ex.Message);
                        }
                    }
                    var errors = new List<string>();
                    errors.AddRange(result.Errors.Select(x => x.Description));
                    return errors;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task Delete(string role, ApplicationUser user)
        {
            switch (role)
            {
                case "User":

                    var customer = await _unitOfWork.UserRepository.GetAllAsync();
                    var temp = customer.Where(x => x.UserId.ToLower().Equals(user.Id.ToLower())).ToList();

                    _unitOfWork.UserRepository.HardDeleteRange(temp);
                    break;
            }
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<List<string>> GetListRoleAsync()
        {
            var roles = await _roleManager.Roles.Where(x => !x.Name.Equals("Manager")).Select(x => x.Name).ToListAsync();
            return roles;
        }

        public async Task<Pagination<UserViewModel>> GetListUserAsync(int pageIndex = 0, int pageSize = 20)
        {
            var listUser = await _userManager.GetUsersInRoleAsync("User");
            var listUsers = listUser.OrderBy(x => x.Email).ToList();
            var itemCount = listUsers.Count();
            var items = listUsers.Skip(pageIndex * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var result = new Pagination<ApplicationUser>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItemsCount = itemCount,
                Items = items,
            };
            var paginationList = _mapper.Map<Pagination<UserViewModel>>(result);
            foreach (var item in paginationList.Items)
            {
                var user = await _userManager.FindByIdAsync(item.Id);
                var isLockout = await _userManager.IsLockedOutAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                string role = "";
                if (roles != null && roles.Count > 0)
                {
                    role = roles[0];
                }
                item.IsLockout = isLockout;
                item.Role = role;
            }
            return paginationList;
        }

        public async Task<Pagination<UserViewForUserRoleModel>> GetListUserForUserRoleAsync(int pageIndex = 0, int pageSize = 20)
        {
            var listUser = await _userManager.GetUsersInRoleAsync("User");
            var listAllUsers = listUser.OrderBy(x => x.Email).ToList();
            foreach (var item in listAllUsers)
            {
                var user = await _userManager.FindByIdAsync(item.Id);
                var isLockout = await _userManager.IsLockedOutAsync(user);
                if (isLockout)
                {
                    listAllUsers.Remove(item);
                }
            }
            var itemCount = listAllUsers.Count();
            var items = listAllUsers.Skip(pageIndex * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var result = new Pagination<ApplicationUser>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItemsCount = itemCount,
                Items = items,
            };
            var paginationList = _mapper.Map<Pagination<UserViewForUserRoleModel>>(result);
            return paginationList;
        }

        public async Task<UserViewModel> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var result = _mapper.Map<UserViewModel>(user);

            var isLockout = await _userManager.IsLockedOutAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            string role = "";
            if (roles != null && roles.Count > 0)
            {
                role = roles[0];
            }
            result.IsLockout = isLockout;
            result.Role = role;
            return result;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");
            return user;
        }

        public async Task<string> LockOrUnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToLower());
            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");
            var isLockout = await _userManager.IsLockedOutAsync(user);
            if (!isLockout)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                return "Khóa tài khoản thành công!";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                return "Mở khóa tài khoản thành công!";

            }
        }

        public async Task<IList<string>> UpdateUserAsync(UserUpdateModel model, string userId)
        {
            var validateResult = await isUserUpdateModelValidationAsync(model);
            if (validateResult != null)
            {
                return validateResult;
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng!");
            }
            else
            {
                var temp = await _userManager.Users
                    .Where(x => !x.Id.ToLower().Equals(user.Id.ToLower()) && x.UserName.ToLower().Equals(model.Username.ToLower()))
                    .FirstOrDefaultAsync();
                if (temp != null)
                    throw new Exception("Tên đăng nhập này đã được sử dụng!");
                try
                {
                    string url = null;
                    if (model.Avatar != null)
                    {
                        Random random = new Random();
                        string newImageName = user.Id + "_i" + model.Avatar.Name.Trim() + random.Next(1, 10000).ToString();
                        string folderName = $"user/{user.Id}/Image";
                        string imageExtension = Path.GetExtension(model.Avatar.FileName);
                        //Kiểm tra xem có phải là file ảnh không.
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        const long maxFileSize = 20 * 1024 * 1024;
                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || model.Avatar.Length > maxFileSize)
                        {
                            throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                        }
                        url = await _fireBaseService.UploadFileToFirebaseStorage(model.Avatar, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");
                        user.AvatarUrl = url;
                    }
                    user.UserName = model.Username;
                    user.NormalizedUserName = model.Username.ToUpper();
                    user.Fullname = model.Fullname;
                    user.PhoneNumber = model.PhoneNumber;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return null;

                    var errors = new List<string>();
                    errors.AddRange(result.Errors.Select(x => x.Description));
                    return errors;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private async Task CreateAccountAsync(ApplicationUser user, string role)
        {
            switch (role)
            {
                case "User":
                    User customer = new User { UserId = user.Id };
                    await _unitOfWork.UserRepository.AddAsync(customer);
                    await _unitOfWork.SaveChangeAsync();
                    var cusResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!cusResult.Succeeded)
                    {
                        throw new Exception("Thêm vai trò bị lỗi");
                    }
                    break;
                default: throw new Exception("Vai trò không hợp lệ");
            }
        }

        private async Task<IList<string>> isUserCreateModelValidationAsync(UserCreateModel model)
        {
            var validator = new UserCreateModelValidator();
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            return null;
        }

        public async Task<IList<string>> isUserUpdateModelValidationAsync(UserUpdateModel model)
        {
            var validator = new UserModelValidator();
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            return null;
        }
    }
}
