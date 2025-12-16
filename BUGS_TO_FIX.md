# Bugs To Fix - ShoppingApp

## Fixed Issues

### 1. Seller Orders - Filters Not Working (FIXED)
**Priority:** Medium
**Status:** Resolved
**Location:**
- View: `Areas/Seller/Views/Orders/Index.cshtml`
- Controller: `Areas/Seller/Controllers/OrdersController.cs`

**Issue:**
The Seller Orders page displayed filter inputs for `search`, `fromDate`, and `toDate`, but the controller only accepted `status` and `page` parameters.

**Fix Applied:**
Updated the controller `Index` action to accept and process all filter parameters:
```csharp
public ActionResult Index(string search, OrderStatus? status, DateTime? fromDate, DateTime? toDate, int page = 1)
```

Added filtering logic for:
- `search`: Filters by order number, customer name, or email
- `fromDate`: Filters orders on or after the specified date
- `toDate`: Filters orders on or before the specified date (inclusive)

Added ViewBag values to preserve filter state in the form.

---

## Verified Working Filters

| Controller | Area | Filters | Status |
|------------|------|---------|--------|
| ProductsController | Main | SearchTerm, CategoryId, MinPrice, MaxPrice, Brand, SortBy | OK |
| OrdersController | Admin | search, status, shopId | OK |
| UsersController | Admin | search, role, status | OK |
| ProductsController | Seller | search, categoryId, status | OK |
| OrdersController | Seller | search, status, fromDate, toDate | OK |
| ReviewsController | Seller | rating | OK |
| OrdersController | Customer | status | OK |
| ReviewsController | Customer | status | OK |

---

## Notes

- All pagination links correctly preserve filter parameters
- Main ProductsController has the most comprehensive filtering with hierarchical category support
- Admin UsersController does role filtering in-memory (less efficient but works)

---

*Last Updated: December 16, 2025*
