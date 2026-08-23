using Microsoft.AspNetCore.Identity;

namespace ECommerce.DAL.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
