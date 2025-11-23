namespace ShoppingApp.Core.Entities;

public class Address
{
    public int AddressId { get; set; }
    public int UserId { get; set; }
    public string AddressLabel { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;

    // Navigation
    public User User { get; set; } = null!;
}
