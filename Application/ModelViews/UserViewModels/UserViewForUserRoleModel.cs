using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ModelViews.UserViewModels
{
    public class UserViewForUserRoleModel
    {
        public string Id { get; set; }
        public string? Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
