using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.API.Models.DTOs;

public class AddressDto
{
    public int AddressId { get; set; }
    public string AddressLabel { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
}

public class CreateAddressRequest
{
    [Required]
    [StringLength(100)]
    public string AddressLabel { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}

public class UpdateAddressRequest
{
    [Required]
    [StringLength(100)]
    public string AddressLabel { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}
