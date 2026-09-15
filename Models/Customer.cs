using System.ComponentModel.DataAnnotations;

namespace InventoryApi.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
