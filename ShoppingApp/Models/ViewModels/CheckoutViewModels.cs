using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ShoppingApp.Models.Domain;

namespace ShoppingApp.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

        [Required(ErrorMessage = "Please select a shipping address")]
        [Display(Name = "Shipping Address")]
        public int SelectedAddressId { get; set; }

        [Display(Name = "Discount Code")]
        public string DiscountCode { get; set; }

        public Discount AppliedDiscount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderConfirmationViewModel
    {
        public Order Order { get; set; }
        public string Message { get; set; }
    }

    public class ApplyDiscountViewModel
    {
        [Required]
        [Display(Name = "Discount Code")]
        public string DiscountCode { get; set; }
    }
}
