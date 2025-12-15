using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.Domain
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Label")]
        public string AddressLabel { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        // Soft delete - hidden from user but preserved for order history
        public bool IsActive { get; set; } = true;

        // Navigation
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
