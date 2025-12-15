# Bugs To Fix - ShoppingApp

## Filter Issues

### 1. Seller Orders - Filters Not Working
**Priority:** Medium
**Status:** Open
**Location:**
- View: `Areas/Seller/Views/Orders/Index.cshtml` (lines 14, 28-29)
- Controller: `Areas/Seller/Controllers/OrdersController.cs` (line 19)

**Description:**
The Seller Orders page displays filter inputs for `search`, `fromDate`, and `toDate`, but the controller only accepts `status` and `page` parameters. These filters appear in the UI but do nothing when used.

**View has:**
```html
<input type="text" name="search" ... />
<input type="date" name="fromDate" ... />
<input type="date" name="toDate" ... />
```

**Controller accepts:**
```csharp
public ActionResult Index(OrderStatus? status, int page = 1)
```

**Fix Required:**
Update the controller to accept and process `search`, `fromDate`, and `toDate` parameters:
```csharp
public ActionResult Index(string search, OrderStatus? status, DateTime? fromDate, DateTime? toDate, int page = 1)
{
    // Add filtering logic for:
    // - search: filter by order number or customer name/email
    // - fromDate: filter orders >= fromDate
    // - toDate: filter orders <= toDate
}
```

---

## Verified Working Filters

| Controller | Area | Filters | Status |
|------------|------|---------|--------|
| ProductsController | Main | SearchTerm, CategoryId, MinPrice, MaxPrice, Brand, SortBy | OK |
| OrdersController | Admin | search, status, shopId | OK |
| UsersController | Admin | search, role, status | OK |
| ProductsController | Seller | search, categoryId, status | OK |
| ReviewsController | Seller | rating | OK |
| OrdersController | Customer | status | OK |
| ReviewsController | Customer | status | OK |

---

## Notes

- All pagination links correctly preserve filter parameters
- Main ProductsController has the most comprehensive filtering with hierarchical category support
- Admin UsersController does role filtering in-memory (less efficient but works)

---

*Last Updated: December 15, 2025*
