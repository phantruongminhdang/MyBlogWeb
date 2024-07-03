using Application.Interfaces.Services;
using System.Security.Claims;

namespace MyBlogWeb.Services
{
    public class ClaimsService : IClaimsService
    {
        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            // todo implementation to get the current userId
            var Id = httpContextAccessor.HttpContext?.User?.FindFirstValue("userId");
            var isAdmin = httpContextAccessor.HttpContext?.User?.FindFirstValue("isAdmin");
            var isUser = httpContextAccessor.HttpContext?.User?.FindFirstValue("isUser");

            GetCurrentUserId = string.IsNullOrEmpty(Id) ? Guid.Empty : Guid.Parse(Id);
            if (string.IsNullOrEmpty(isAdmin))
                GetIsAdmin = false;
            else if(isAdmin.Equals("True")) GetIsAdmin = true;
            else GetIsAdmin = false;

            if (string.IsNullOrEmpty(isUser))
                GetIsUser = false;
            else if (isUser.Equals("True")) GetIsUser = true;
            else GetIsUser = false;
        }

        public Guid GetCurrentUserId { get; }
        public bool GetIsAdmin { get; }
        public bool GetIsUser { get; }
    }
}
