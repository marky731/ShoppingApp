using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OrderCount { get; set; }
        public int ReviewCount { get; set; }
        public int FavoriteCount { get; set; }
        public IEnumerable<Order> RecentOrders { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }
    }

    public class AddressViewModel
    {
        public int AddressId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Label (e.g., Home, Office)")]
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
    }
}
