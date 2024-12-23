using Microsoft.AspNetCore.Identity;

namespace restaurant_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
