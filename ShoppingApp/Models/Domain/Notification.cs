using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShoppingApp.Models.Identity;

namespace ShoppingApp.Models.Domain
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [StringLength(255)]
        [Display(Name = "Link")]
        public string LinkUrl { get; set; }

        [Display(Name = "Read")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
