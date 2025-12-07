using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.Domain
{
    public class ReviewResponse
    {
        [Key]
        public int ResponseId { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        public string SellerId { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Response")]
        public string ResponseText { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; }

        [ForeignKey("SellerId")]
        public virtual ApplicationUser Seller { get; set; }
    }
}
