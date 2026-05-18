using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;

namespace Logistics.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; }

        public string Region { get; set; }

        public ICollection<Contract>? Contracts { get; set; }
    }
}
