**Actor: Guest**

**1\. Use Case: Browse Products**

* **Goal:** To allow the user to see products on the homepage or category pages.  
* **Inputs:**  
  * Clicking on the homepage link or a category link.  
  * Clicking on a page number (Pagination).  
* **Process:**  
  * The system queries the Products table from the database.  
  * If a category is selected, it filters by that categoryID.  
  * It sorts the products by a default order (e.g., date added, popularity).  
* **Outputs:**  
  * A grid view of products (Each product shows: image, title, price).  
  * A pagination component (1, 2, 3... Next).

**2\. Use Case: Search Products**

* **Goal:** To find products matching a specific keyword.  
* **Inputs:**  
  * Search bar (\<input type="text"\>): The text (keyword) entered by the user.  
  * "Search" button (\<button\>) or "Enter" key press event.  
* **Process:**  
  * The system takes the entered text.  
  * It performs a search using the LIKE operator in the Products table (in fields like productName, description, and categoryName).  
* **Outputs:**  
  * A search results page: A list of matching products.  
  * If no match: "No products were found matching your criteria" message.

**3\. Use Case: Filter Products**

* **Goal:** To narrow down the displayed product list based on specific criteria.  
* **Inputs:**  
  * Filtering components (Sidebar): Category checkboxes, price range slider, brand checkboxes, rating selection.  
  * Sorting dropdown menu (\<select\>): "Price: Low to High", "Highest Rating", etc.  
* **Process:**  
  * The system collects all selected filters (e.g., Category=Tech AND Price \< 5000).  
  * It updates the database query with these AND conditions and re-executes it.  
* **Outputs:**  
  * The newly filtered and sorted product list.

**4\. Use Case: View Product Details**

* **Goal:** To access all information, images, and reviews for a single product.  
* **Inputs:**  
  * Clicking on a product card (link) in a product list.  
* **Process:**  
  * The system gets the productID of the clicked product.  
  * It fetches all related data for this productID from the Products, Reviews, Images, and Sellers tables.  
* **Outputs:**  
  * The Product Detail Page, displaying: Image gallery, product name, price, stock status, "Add to Cart" button, description, seller information, and a reviews tab.

**5\. Use Case: View Shop Page**

* **Goal:** To view a specific seller's profile page and all products they offer.  
* **Inputs:**  
  * Clicking on a shop name (link) on a product detail page or a shop list.  
* **Process:**  
  * The system gets the shopID (or sellerID).  
  * It queries the Shops table for profile information (logo, name, description, rating).  
  * It queries the Products table for all products where sellerID matches.  
* **Outputs:**  
  * A dedicated shop page displaying the shop's banner/logo, description, and a grid of all products sold by that shop.

**6\. Use Case: Create Account (Register)**

* **Goal:** To create a new Customer account in the system.  
* **Inputs:**  
  * Registration form (\<form\>): Fields for Name, Email, Password, Password Confirmation.  
  * "Sign Up" button.  
* **Process:**
  1. (Frontend) JavaScript Validation: Check for empty fields, valid email format, and matching passwords.
  2. (Backend) Server Validation: Repeat checks.
  3. Check if the email already exists in the Users table.
  4. "Hash" the password.
  5. INSERT a new record into the Users table (with role='customer').  
* **Outputs:**  
  * Success: "Registration successful. Please log in." message and redirection to the Login page.  
  * Failure: Error messages on the form ("This email is already in use").

**7\. Use Case: Login**

* **Goal:** To authenticate the user and start a session.  
* **Inputs:**  
  * Login form: Email, Password fields.  
  * "Login" button.  
* **Process:**  
  * System searches for the user by email in the Users table.  
  * If found, it compares the "hashed" version of the entered password with the stored "hash".  
  * If they match, a "session" is created on the server, storing the user's ID and role.  
* **Outputs:**  
  * Success: Redirect to the homepage (menu now shows "My Account", "Logout").  
  * Failure: "Invalid email or password" message.

**8\. Use Case: Request Password Reset**

* **Goal:** To initiate the password reset process when a user forgets their password.  
* **Inputs:**  
  * Forgot Password Page: Email address input (\<input type="email"\>).  
  * "Send Reset Link" button.  
* **Process:**  
  * System checks if the email exists in the Users table.  
  * If it exists, it generates a unique, time-limited "reset token".  
  * It saves this token and its expiry time to the database (associated with the user).  
  * It sends an email to the user containing a unique link (e.g., .../reset-password?token=...).  
* **Outputs:**  
  * A message: "If an account with that email exists, a password reset link has been sent."

**9\. Use Case: Reset Password**

* **Goal:** To allow a user to set a new password using a valid reset token.  
* **Inputs:**  
  * User clicks the link from their email.  
  * Reset Password Form: New Password, Confirm New Password fields.  
  * "Set New Password" button.  
* **Process:**  
  * System retrieves the "token" from the URL.  
  * It validates the token: Does it exist in the database, and has it expired?  
  * If valid, it validates that the two new passwords match.  
  * It "hashes" the new password.  
  * It UPDATEs the user's password in the Users table.  
  * It invalidates/deletes the reset token.  
* **Outputs:**  
  * Success: "Password successfully updated. Please log in." message and redirection to the Login page.  
  * Failure: "Invalid or expired link. Please try again."  
  * 

**Actor: Customer**

*(Can perform all Guest Use Cases, plus:)*

**10\. Use Case: Manage My Profile**

* **Goal:** To allow a customer to update their personal information and addresses.  
* **Inputs:**  
  * My Account Page:  
    * Profile form (Name, Surname, Phone).  
    * Address form (Street, City, Postal Code) \-\> "Add New Address" button.  
    * List of existing addresses with "Edit" / "Delete" buttons.  
    * "Save Changes" button.  
* **Process:**  
  * For profile: System UPDATEs the user's info in the Users table.  
  * For addresses: System performs INSERT, UPDATE, or DELETE on the Addresses table, linked to the userID.  
* **Outputs:**  
  * "Profile updated successfully" message.

**11\. Use Case: Manage Cart**

* **Goal:** To collect and organize products for purchase.  
* **Inputs:**  
  * "Add to Cart" button on a product page.  
  * On the Cart page: "Remove" icon, Quantity increase/decrease buttons (+ / \-).  
* **Process:**  
  * The system adds/updates/removes the productID and quantity in the user's session or Carts table.  
  * It recalculates the cart's subtotal and grand total with each change.  
* **Outputs:**  
  * Updated item count on the cart icon in the menu.  
  * Updated product list and total price on the cart page.  
  * "Product added to cart" notification.

**12\. Use Case: Manage Favorites (Wishlist)**

* **Goal:** To save products for later consideration.  
* **Inputs:**  
  * "Add to Favorites" icon (e.g., a heart) on a product card.  
  * "Remove from Favorites" button on the Favorites page.  
* **Process:**  
  * System INSERTs or DELETEs a record in the Favorites table, linking a userID and a productID.  
* **Outputs:**  
  * The heart icon changes color (e.g., fills in).  
  * The product appears on the user's "My Favorites" page.

**13\. Use Case: Place Order**

* **Goal:** To complete the purchase process for items in the cart.  
* **Inputs:**
  * Checkout page:
    * Address selection component (\<input type="radio"\>).
    * Payment method selection.
    * Credit card form (handled by a secure 3rd-party provider).
    * "Apply Discount Code" field (\<input type="text"\>).
    * "Complete Order" button.
* **Process:**
  1. (Optional) Validate and apply discount code, recalculating the total.
  2. Make Payment: Send request to payment service.
  3. If payment is confirmed:
     * INSERT new record into Orders table (userID, totalAmount, addressID).
     * For each cart item, INSERT into OrderDetails table (orderID, productID, quantity, price).
     * UPDATE (decrement) the stock in the Products table.
     * Clear the user's cart.  
* **Outputs:**  
  * "Your order has been received successfully" confirmation page (with Order Number).  
  * If error: "An error occurred during payment" message.

**14\. Use Case: View Order History**

* **Goal:** To see a list of past orders and their status.  
* **Inputs:**  
  * Clicking "My Orders" in the user account menu.  
  * Clicking "View Details" for a specific order.  
* **Process:**  
  * System queries the Orders table for all orders matching the userID.  
  * If "View Details" is clicked, it also queries OrderDetails for that orderID.  
* **Outputs:**  
  * A list of all past orders (Order ID, Date, Total, Status).  
  * Order Detail view: Shows products purchased, quantities, and shipping status.

**15\. Use Case: Write Review**

* **Goal:** To rate and comment on a product they have previously purchased.
* **Inputs:**
  * On the "My Orders" page or product page (if purchased):
    * Rating component (5 stars \- \<input type="radio"\>).
    * Review title (\<input type="text"\>) and text (\<textarea\>).
    * "Submit Review" button.
* **Process:**
  * System validates that the userID has purchased this productID by checking their Orders.
  * If validated, INSERT the review and rating into the Reviews table (with status \= 'pending').
* **Outputs:**
  * "Thank you for your feedback. Your review will be published after it is approved." message.

**16\. Use Case: Logout**

* **Goal:** To securely end the user's session.  
* **Inputs:**  
  * Clicking the "Logout" link.  
* **Process:**  
  * The system destroys the session data on the server.  
* **Outputs:**  
  * User is redirected to the homepage as a Guest.  
      
      
      
      
      
      
      
      
      
      
      
      
      
      
  


**Actor: Seller**

*(Can perform all Customer Use Cases, plus:)*

**17\. Use Case: Manage Shop Profile**

* **Goal:** To update the public-facing details of the seller's shop.  
* **Inputs:**  
  * Seller Dashboard \-\> "Shop Profile" page:  
    * Shop Name (\<input type="text"\>).  
    * Shop Description (\<textarea\>).  
    * Shop Logo/Banner (\<input type="file"\>).  
    * "Save Changes" button.  
* **Process:**  
  * System UPDATEs the relevant fields in the Shops table for the seller's shopID.  
  * If a new image is uploaded, it saves the file and updates the database path.  
* **Outputs:**  
  * "Shop profile updated successfully." message.

**18\. Use Case: Manage Products (CRUD)**

* **Goal:** To add, update, or delete products belonging to their shop.  
* **Inputs:**  
  * Seller Dashboard \-\> "My Products" page:  
    * "Add New Product" button.  
    * Add/Update Product Form: Name, description, price, stock, category, image upload.  
    * Product List Table: "Edit" and "Delete" buttons for each product.  
* **Process:**  
  * System verifies sellerID for all INSERT, UPDATE, DELETE operations on the Products table.  
* **Outputs:**  
  * "Product successfully added/updated/deleted." message.  
  * The updated list of the seller's products.

**19\. Use Case: Manage Orders**

* **Goal:** To view and process incoming orders for their products.  
* **Inputs:**  
  * Seller Dashboard \-\> "My Orders" page:  
    * List of incoming orders (filtered for their sellerID).  
    * "Update Status" dropdown (\<select\>: Processing, Shipped, Delivered).  
    * Tracking code input (\<input type="text"\>).  
    * "Save" button.  
* **Process:**  
  * System UPDATEs the orderStatus in the Orders (or OrderDetails) table.  
  * (Optional) Triggers a notification to the customer.  
* **Outputs:**  
  * "Order status updated" message.

**20\. Use Case: Manage Customer Interactions**

* **Goal:** To respond to questions or reviews left on their products.  
* **Inputs:**  
  * Seller Dashboard \-\> "Reviews" or "Questions" tab.  
  * A list of reviews/questions.  
  * "Respond" button for each item.  
  * Response text area (\<textarea\>).  
  * "Submit Response" button.  
* **Process:**  
  * System INSERTs a new record into a ReviewResponses or QuestionAnswers table, linking it to the original review/question and the sellerID.  
* **Outputs:**  
  * The seller's response appears nested under the customer's review on the product detail page.


  


**Actor: Admin (Administrator)**

**21\. Use Case: Manage User Accounts**

* **Goal:** To monitor, suspend, or delete customer and seller accounts.  
* **Inputs:**  
  * Admin Panel \-\> "Users" page:  
    * User search bar.  
    * Table list of all users.  
    * "Suspend", "Delete", or "Change Role" buttons for each user.  
* **Process:**  
  * For "Suspend", UPDATE the isActive field in the Users table.  
  * For "Delete", DELETE the record (or mark as inactive).  
* **Outputs:**  
  * "User successfully suspended/deleted." message.

**22\. Use Case: Manage Seller Applications**

* **Goal:** To approve or reject new requests to become a seller.  
* **Inputs:**  
  * Admin Panel \-\> "Seller Applications" page:  
    * A list of applying users (status='pending\_seller').  
    * "Approve" and "Reject" buttons.  
* **Process:**  
  * On "Approve": UPDATE the user's role in the Users table to 'seller' and create their entry in the Shops table.  
  * On "Reject": Change status to 'rejected' or delete the application.  
* **Outputs:**  
  * "Seller approved" / "Seller rejected" message.  
  * Informational email is sent to the user.

**23\. Use Case: Manage Main Categories**

* **Goal:** To manage the site-wide main categories (Tech, Apparel, etc.).  
* **Inputs:**  
  * Admin Panel \-\> "Categories" page:  
    * "Add New Category" form (\<input type="text"\>).  
    * "Edit" and "Delete" buttons for each category in the list.  
* **Process:**  
  * INSERT, UPDATE, or DELETE operations on the Categories table.  
* **Outputs:**  
  * "Category successfully added/updated/deleted." message.

**24\. Use Case: Moderate Reviews**

* **Goal:** To approve or reject customer-submitted reviews before they are public.  
* **Inputs:**  
  * Admin Panel \-\> "Pending Reviews" page:  
    * A queue/list of all reviews with status \= 'pending'.  
    * Each review shows the text, rating, product, and customer.  
    * "Approve" and "Reject" (or "Delete") buttons for each review.  
* **Process:**  
  * On "Approve": System UPDATEs the review's status to 'approved'.  
  * On "Reject": System UPDATEs the review's status to 'rejected' or DELETEs the record.  
* **Outputs:**
  * The review is removed from the pending queue.
  * If approved, the review now appears on the View Product Details page.

---

## Additional Use Cases

**Actor: Guest**

**25\. Use Case: Apply to Become Seller**

* **Goal:** To submit an application to become a seller on the platform.
* **Inputs:**
  * Seller Application form:
    * Business Name (\<input type="text"\>).
    * Business Description (\<textarea\>).
    * Contact Phone (\<input type="tel"\>).
    * "Submit Application" button.
* **Process:**
  * System validates that the user is logged in as a customer.
  * INSERT a new record into the Shops table with is\_approved = FALSE.
  * Create a notification for admins about the new application.
* **Outputs:**
  * "Your seller application has been submitted. You will be notified once it is reviewed." message.

---

**Actor: Customer**

**26\. Use Case: Track Order**

* **Goal:** To view the current status and shipping progress of an order.
* **Inputs:**
  * Clicking "Track" button on an order in "My Orders" page.
* **Process:**
  * System retrieves the order and all related ShopOrders.
  * For each ShopOrder, fetch the shop\_order\_status and tracking\_number.
* **Outputs:**
  * Order tracking page showing:
    * Order status timeline (Pending → Processing → Shipped → Delivered).
    * Tracking number with link to carrier's tracking page.
    * Estimated delivery date (if available).

**27\. Use Case: Cancel Order**

* **Goal:** To cancel an order before it has been shipped.
* **Inputs:**
  * Clicking "Cancel Order" button on an order in "My Orders" page.
  * Cancellation reason selection (\<select\>).
  * "Confirm Cancellation" button.
* **Process:**
  * System validates that the order status is 'pending' or 'processing'.
  * UPDATE all related ShopOrders status to 'cancelled'.
  * UPDATE (increment) the stock in the Products table.
  * Initiate refund process if payment was made.
  * Create notifications for affected sellers.
* **Outputs:**
  * "Your order has been cancelled successfully. Refund will be processed within 3-5 business days." message.
  * If not cancellable: "This order cannot be cancelled as it has already been shipped." message.

**28\. Use Case: Request Return/Refund**

* **Goal:** To request a return or refund for a delivered order.
* **Inputs:**
  * "Request Return" button on a delivered order.
  * Return form:
    * Items to return (checkboxes).
    * Return reason (\<select\>).
    * Additional comments (\<textarea\>).
    * "Submit Return Request" button.
* **Process:**
  * System validates that the order was delivered within the return window.
  * INSERT a new record into the Returns table with status = 'pending'.
  * Create notification for the seller.
* **Outputs:**
  * "Your return request has been submitted. The seller will review it shortly." message.

---

**Actor: Seller**

**21\. Use Case: Manage Discounts**

* **Goal:** To create, update, or delete discount codes for their shop.
* **Inputs:**
  * Seller Dashboard → "Discounts" page:
    * "Add New Discount" button.
    * Discount form: Code, Type (percentage/fixed), Value, Expiry Date, Minimum Order Amount, Usage Limit.
    * "Edit" and "Delete" buttons for existing discounts.
* **Process:**
  * System performs INSERT, UPDATE, or DELETE on the Discounts table with the seller's shop\_id.
* **Outputs:**
  * "Discount code successfully created/updated/deleted." message.
  * Updated list of active discounts.

**22\. Use Case: View Sales Analytics**

* **Goal:** To view performance metrics and sales reports for their shop.
* **Inputs:**
  * Seller Dashboard → "Analytics" page:
    * Date range selector.
    * Report type tabs (Sales, Products, Customers).
* **Process:**
  * System aggregates data from ShopOrders, OrderItems, and Reviews tables for the seller's shop\_id.
  * Calculate metrics: Total revenue, orders count, average order value, top products, customer ratings.
* **Outputs:**
  * Dashboard with:
    * Sales charts and graphs.
    * Top-selling products list.
    * Revenue breakdown by period.
    * Customer review summary.

---

**Actor: Admin**

**25\. Use Case: View Admin Dashboard**

* **Goal:** To monitor overall platform performance and key metrics.
* **Inputs:**
  * Admin Panel → "Dashboard" page.
  * Date range filters.
* **Process:**
  * System aggregates data from Users, Orders, Products, and Shops tables.
  * Calculate platform-wide metrics.
* **Outputs:**
  * Dashboard displaying:
    * Total users, sellers, products, orders.
    * Revenue trends and charts.
    * Recent orders and new registrations.
    * Pending items requiring attention (seller applications, reviews to moderate).

**26\. Use Case: Generate Reports**

* **Goal:** To generate and export detailed reports for business analysis.
* **Inputs:**
  * Admin Panel → "Reports" page:
    * Report type selection (Sales, Users, Products, Sellers).
    * Date range.
    * Export format (CSV, PDF).
    * "Generate Report" button.
* **Process:**
  * System queries relevant tables based on report type and filters.
  * Format data according to selected export type.
* **Outputs:**
  * Downloadable report file.
  * Preview of report data on screen.

