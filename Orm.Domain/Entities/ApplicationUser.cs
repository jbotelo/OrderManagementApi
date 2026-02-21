using Microsoft.AspNetCore.Identity;

namespace Orm.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
