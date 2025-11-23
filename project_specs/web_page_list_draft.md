**List of Web Pages**

**For Guest & Customer**

**1\. HOMEPAGE (File: index.html)**

* **Definition:** The main landing page where the user first enters the site, showing banners, categories and featured products.  
* **Purpose:** Directing the user into shopping by displaying popular products and campaigns.

**2\. PRODUCT LISTING PAGE (File: products.html or /category/{id})**

* **Definition:** The page listing products by a specific category or search results.  
* **Purpose:** To allow users to explore products easily with browsing, searching and filtering options.

**3\. PRODUCT DETAIL PAGE (File: product-detail.html or /product/{id})**

* **Definition:** The page showing product images, description, price, seller information and customer reviews.  
* **Purpose:** To allow the user to review the product and choose to add it to the cart or wishlist.

**4\. SHOP PAGE (File: /shop/{id})**

* **Definition:** Shows a specific seller's store profile (logo, description) and all their listed products.  
* **Purpose:** To allow users to view all products from a seller and increase trust in the shop.

**5\. LOGIN PAGE (File: login.html)**

* **Definition:** The page where the user logs into their account with e-mail and password.  
* **Purpose:** To provide access to the user's personal account and special features.

**6\. REGISTER PAGE (File: register.html)**

* **Definition:** Form page where the new user creates an account.  
* **Purpose:** To create a customer account and start the shopping experience.

**7\. PASSWORD RESET REQUEST PAGE (File: forgot-password.html)**

* **Definition:** Page where users enter their e-mail to receive a password reset link.  
* **Purpose:** To provide secure access to the user when they forget their password.

**8\. PASSWORD RESET PAGE (File: reset-password.html)**

* **Definition:** The page where the user sets a new password using the verification link received by e-mail.  
* **Purpose:** Completing the password reset process while maintaining account security.

**9\. USER PROFILE PAGE (File: /profile/info)**

* **Definition:** The page where the user can edit personal information and manage addresses.  
* **Purpose:** To ensure that the user can manage their contact and delivery details.

**10\. WISHLIST PAGE (File: /profile/wishlist)**

* **Definition:** The page where the user can save the products they like.  
* **Purpose:** To allow users to revisit products for future purchase.

**11\. CART PAGE (File: cart.html)**

* **Definition:** The page showing the quantity and price details of the products to be purchased.  
* **Purpose:** To allow the user to review the products before payment.

**12\. CHECKOUT PAGE (File: checkout.html)**

* **Definition:** The page where the user confirms the delivery address, payment method and order summary.  
* **Purpose:** To complete the purchase process.

**13\. ORDER CONFIRMATION PAGE (File: /order/success)**

* **Definition:** The page displayed after an order is successfully placed.  
* **Purpose:** To inform the user about the order number and the next steps.

**14\. ORDER HISTORY PAGE (File: /profile/orders)**

* **Definition:** Page where the user can view all previously placed orders and their statuses.  
* **Purpose:** To allow the user to access and review their past purchases.

**15\. REVIEW PRODUCT PAGE (File: /profile/orders/{id}/review)**

* **Definition:** Page where the user can rate and write a review for a purchased product.  
* **Purpose:** To build reliability and provide useful information for other customers.

**FOR SELLER (Path: /seller/\*)** 

**1\. SELLER DASHBOARD PAGE (File: /seller/dashboard)**

* **Definition:** The main interface where the seller manages their store-related actions.  
* **Purpose:** To provide the seller with centralized access to shop profile, products and orders.

**2\. SHOP PROFILE MANAGEMENT PAGE (File: /seller/profile)**

* **Definition:** The page where the seller can update the shop's information such as shop name, description and logo.  
* **Purpose:** To maintain a professional shop appearance for customers.

**3\. PRODUCT MANAGEMENT PAGE (CRUD) (File: /seller/products)**

* **Definition:** The page where the seller can add, update or delete products they offer.  
* **Purpose:** To manage product listings, pricing, stock information and product descriptions.

**4\. ORDER MANAGEMENT PAGE (File: /seller/orders)**

* **Definition:** The page where the seller can view and update the status of incoming orders.  
* **Purpose:** To process orders by marking them as Processing, Shipped or Delivered and add tracking information.

**5\. CUSTOMER INTERACTION PAGE (File: /seller/reviews)**

* **Definition:** The section where the seller can view customer reviews or questions and respond to them.  
* **Purpose:** To improve customer satisfaction and build trust by providing feedback and support.

**6\. SELLER DISCOUNT MANAGEMENT PAGE (File: /seller/discounts)**

* **Definition:** The page where the seller can create, edit, or deactivate discount codes (e.g., "SHOP10OFF") valid only for their own shop.  
* **Purpose:** To enable sellers to run their own promotions and increase their sales.

**FOR ADMIN (Path: /admin/\*)** 

**1\. USER MANAGEMENT PAGE (File: /admin/users)**

* **Definition:** The page where the admin can view, suspend, delete or change the role of user accounts.  
* **Purpose:** To ensure safe and controlled access to the platform by managing customer and seller accounts.

**2\. SELLER APPLICATION PAGE (File: /admin/applications)**

* **Definition:** The page where the admin reviews and decides on pending seller application requests.  
* **Purpose:** To approve or reject users who want to become sellers, ensuring that only trusted sellers can operate on the platform.

**3\. CATEGORY MANAGEMENT PAGE (File: /admin/categories)**

* **Definition:** The page where the admin can add, edit or delete product categories.  
* **Purpose:** To organize the product catalog and maintain a clear and structured browsing experience for users.

**4\. REVIEW MODERATION PAGE (File: /admin/reviews)**

* **Definition:** The page where the admin reviews customer-submitted product reviews before they are published.  
* **Purpose:** To maintain content quality and prevent misleading, inappropriate or harmful reviews from appearing publicly.

**5\. ADMIN DISCOUNT MANAGEMENT PAGE (File: /admin/discounts)**

* **Definition:** The page where the admin can create, edit, or deactivate site-wide discount codes (e.g., "WELCOME15") valid for all products on the platform.  
* **Purpose:** To manage global promotions and marketing campaigns for the entire website.

**6\. ATTRIBUTE MANAGEMENT PAGE (File: /admin/attributes)**

* **Definition:** The page where the admin manages filterable attributes (e.g., 'Color', 'Size') and their predefined options (e.g., 'Black', 'Large', '16GB').  
* **Purpose:** To ensure data consistency for product filtering and to define which attributes are available for which categories.

