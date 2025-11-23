# Optimization Notes

## Why These Changes Were Made

### Use Cases Fixes

**Mislabeling & Duplicates**
- UC 15 was titled "Logout" but described review functionality - this would cause confusion during development and testing

**Numbering Issues**
- Inconsistent step numbers (5,6,7 instead of 1,2,3) make documentation hard to follow and reference

**Missing Use Cases**
- **Track/Cancel Order & Returns**: Essential e-commerce features customers expect; without these, support burden increases
- **Apply to Become Seller**: Needed to complete the seller onboarding flow mentioned in admin use cases
- **Manage Discounts**: Referenced in checkout flow but had no seller-side management
- **Analytics/Dashboard**: Sellers and admins need visibility into performance metrics for business decisions

---

### Database Schema Enhancements

**Added Timestamps (`created_at`, `updated_at`)**
- Enable audit trails and debugging
- Support features like "recently added to cart" or cart abandonment emails
- Track when records were modified for data integrity

**Products: `slug`, `average_rating`, `total_reviews`**
- `slug`: SEO-friendly URLs improve search rankings and user experience
- `average_rating`/`total_reviews`: Denormalized for performance - avoids expensive JOINs on every product listing

**Shops: `banner_image_url`, `average_rating`, `total_sales`**
- Complete shop profile customization
- Quick access to shop metrics without aggregation queries

**Reviews: `title`**
- Use case mentioned review titles but schema didn't support it

**Discounts: `minimum_order_amount`, `usage_limit`, `per_user_limit`**
- Standard discount controls to prevent abuse
- Enable marketing strategies (e.g., "20% off orders over $50")

---

### New Tables Rationale

**ProductQuestions & QuestionAnswers**
- UC 20 (Manage Customer Interactions) references seller responses to questions, but no tables existed

**Payments**
- Original schema only stored `payment_id` string - insufficient for refunds, payment status tracking, and financial reporting

**Returns & ReturnItems**
- Critical for e-commerce operations
- Supports the new "Request Return/Refund" use case
- Enables tracking partial returns (specific items from an order)

**OrderStatusHistory**
- Audit trail for order status changes
- Customer support can see full timeline
- Accountability for who changed what and when

**DiscountUsage**
- Tracks per-user discount usage to enforce `per_user_limit`
- Enables analytics on discount effectiveness

---
