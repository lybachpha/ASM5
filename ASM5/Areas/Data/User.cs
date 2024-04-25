using Microsoft.AspNetCore.Identity;

namespace ASM5.Areas.Data
{
    public class User : IdentityUser
    {
        public string? UserRole { get; set; }
    }
}
