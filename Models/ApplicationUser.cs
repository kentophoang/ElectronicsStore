using Microsoft.AspNetCore.Identity;

namespace ElectronicsStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add any custom properties for your user
        public string? FullName { get; set; }
        public string? ShippingAddress { get; set; }
    }
}