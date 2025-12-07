using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingApp.Models.Domain
{
    public class ProductSpecification
    {
        [Key]
        public int SpecificationId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Specification Name")]
        public string SpecName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Specification Value")]
        public string SpecValue { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
