using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models
{
    public class EditRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> AllRoles { get; set; } = new List<string>();
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}