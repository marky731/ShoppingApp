using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models.DTOs;
using ShoppingApp.Core.Entities;
using ShoppingApp.Core.Interfaces.Repositories;
using ShoppingApp.Infrastructure.Data;

namespace ShoppingApp.API.Controllers.Addresses;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public AddressesController(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<List<AddressDto>>> GetAddresses()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId)
            .Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressLabel = a.AddressLabel,
                StreetAddress = a.StreetAddress,
                City = a.City,
                PostalCode = a.PostalCode,
                Country = a.Country
            })
            .ToListAsync();

        return Ok(addresses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AddressDto>> GetAddress(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _context.Addresses
            .Where(a => a.AddressId == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (address == null)
        {
            return NotFound(new { message = "Address not found" });
        }

        return Ok(new AddressDto
        {
            AddressId = address.AddressId,
            AddressLabel = address.AddressLabel,
            StreetAddress = address.StreetAddress,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country
        });
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = new Address
        {
            UserId = userId.Value,
            AddressLabel = request.AddressLabel,
            StreetAddress = request.StreetAddress,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        await _unitOfWork.Addresses.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAddress), new { id = address.AddressId }, new AddressDto
        {
            AddressId = address.AddressId,
            AddressLabel = address.AddressLabel,
            StreetAddress = address.StreetAddress,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AddressDto>> UpdateAddress(int id, [FromBody] UpdateAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _context.Addresses
            .Where(a => a.AddressId == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (address == null)
        {
            return NotFound(new { message = "Address not found" });
        }

        address.AddressLabel = request.AddressLabel;
        address.StreetAddress = request.StreetAddress;
        address.City = request.City;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country;

        _unitOfWork.Addresses.Update(address);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new AddressDto
        {
            AddressId = address.AddressId,
            AddressLabel = address.AddressLabel,
            StreetAddress = address.StreetAddress,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAddress(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var address = await _context.Addresses
            .Where(a => a.AddressId == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (address == null)
        {
            return NotFound(new { message = "Address not found" });
        }

        _unitOfWork.Addresses.Remove(address);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Address deleted successfully" });
    }
}
