using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Domain.Entities.Base
{
    public class ApplicationUser : IdentityUser
    {
        public string? Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsRegister { get; set; } = false;


        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}
