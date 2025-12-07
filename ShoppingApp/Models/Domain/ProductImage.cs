using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingApp.Models.Domain
{
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        // Navigation
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
