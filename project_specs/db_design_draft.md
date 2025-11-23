### **Database Schema (21 Tables)**

#### **Part 1: User, Auth & Profile Tables**

These tables manage who the users are, their roles, addresses, and security.

**1\. Roles**

* **role\_id** (PK, INT, Auto-increment)  
* role\_name (VARCHAR(50), UNIQUE, NOT NULL) \- *e.g., 'customer', 'seller', 'admin'*

**2\. Users**

* **user\_id** (PK, INT, Auto-increment)  
* role\_id (FK \-\> Roles.role\_id, INT, NOT NULL)  
* first\_name (VARCHAR(100), NOT NULL)  
* last\_name (VARCHAR(100), NOT NULL)  
* email (VARCHAR(255), UNIQUE, NOT NULL)  
* password\_hash (VARCHAR(255), NOT NULL) \- *Secure hash (e.g., bcrypt).*  
* phone (VARCHAR(20), NULLABLE)
* is\_active (BOOLEAN, DEFAULT TRUE) \- *For Admin to suspend the account.*
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)
* updated\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP ON UPDATE CURRENT\_TIMESTAMP)

**3\. Addresses**

* **address\_id** (PK, INT, Auto-increment)  
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)  
* address\_label (VARCHAR(100), NOT NULL) \- *'Home', 'Work', etc.*  
* street\_address (VARCHAR(255), NOT NULL)  
* city (VARCHAR(100), NOT NULL)  
* postal\_code (VARCHAR(20), NULLABLE)  
* country (VARCHAR(100), NOT NULL)

**4\. PasswordResets**

* **reset\_id** (PK, INT, Auto-increment)  
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)  
* reset\_token (VARCHAR(255), UNIQUE, NOT NULL) \- *The unique code sent via email.*  
* expires\_at (TIMESTAMP, NOT NULL) \- *Expiration time (e.g., 1 hour).*  
* is\_used (BOOLEAN, DEFAULT FALSE)

#### **Part 2: Catalog & Shop Tables**

These tables define the sellers, products, categories, and the filterable attributes of products.

**5\. Shops/Seller**

* **shop\_id** (PK, INT, Auto-increment)
* seller\_id (FK \-\> Users.user\_id, INT, UNIQUE, NOT NULL) \- *1-to-1 relationship with a user.*
* shop\_name (VARCHAR(255), NOT NULL)
* description (TEXT, NULLABLE)
* logo\_image\_url (VARCHAR(255), NULLABLE)
* banner\_image\_url (VARCHAR(255), NULLABLE)
* average\_rating (DECIMAL(3, 2), DEFAULT 0) \- *Denormalized shop rating.*
* total\_sales (INT, DEFAULT 0) \- *Total number of completed orders.*
* is\_approved (BOOLEAN, DEFAULT FALSE) \- *For Admin approval.*
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)
* updated\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP ON UPDATE CURRENT\_TIMESTAMP)

**6\. Categories**

* **category\_id** (PK, INT, Auto-increment)  
* category\_name (VARCHAR(100), NOT NULL)  
* parent\_category\_id (FK \-\> Categories.category\_id, INT, NULLABLE) \- *For sub-category hierarchy.*

**7\. Products**

* **product\_id** (PK, INT, Auto-increment)
* shop\_id (FK \-\> Shops.shop\_id, INT, NOT NULL)
* category\_id (FK \-\> Categories.category\_id, INT, NOT NULL)
* product\_name (VARCHAR(255), NOT NULL)
* slug (VARCHAR(255), UNIQUE, NOT NULL) \- *SEO-friendly URL identifier.*
* description (TEXT, NOT NULL)
* price (DECIMAL(10, 2), NOT NULL)
* stock\_quantity (INT, NOT NULL, DEFAULT 0)
* main\_image\_url (VARCHAR(255), NULLABLE)
* brand (VARCHAR(100), NULLABLE) \- *Dedicated, free-text field for filtering.*
* model (VARCHAR(100), NULLABLE)
* average\_rating (DECIMAL(3, 2), DEFAULT 0) \- *Denormalized for performance.*
* total\_reviews (INT, DEFAULT 0) \- *Denormalized review count.*
* is\_active (BOOLEAN, DEFAULT TRUE) \- *For the seller to unpublish the product.*
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)
* updated\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP ON UPDATE CURRENT\_TIMESTAMP)

**8\. ProductImages**

* **image\_id** (PK, INT, Auto-increment)  
* product\_id (FK \-\> Products.product\_id, INT, NOT NULL)  
* image\_url (VARCHAR(255), NOT NULL)

**9\. Attributes**

* **attribute\_id** (PK, INT, Auto-increment)  
* attribute\_name (VARCHAR(100), UNIQUE, NOT NULL) \- *e.g., 'Color', 'Size', 'RAM'*

**10\. AttributeOptions** 

* **option\_id** (PK, INT, Auto-increment)  
* attribute\_id (FK \-\> Attributes.attribute\_id, INT, NOT NULL)  
* option\_value (VARCHAR(100), NOT NULL) \- *e.g., 'Black', 'Large', '16GB'*

**11\. CategoryAttributes (Junction Table)**

* **category\_id** (PK, FK \-\> Categories.category\_id)  
* **attribute\_id** (PK, FK \-\> Attributes.attribute\_id)  
* *(Composite Primary Key \- Defines which attributes a category has)*

**12\. ProductAttributeValues**

* **value\_id** (PK, INT, Auto-increment)  
* product\_id (FK \-\> Products.product\_id, INT, NOT NULL)  
* option\_id (FK \-\> AttributeOptions.option\_id, INT, NOT NULL) \- *This product has the option 'Black' (option\_id: 1).*


#### **Part 3: User Interaction Tables**

These tables manage dynamic actions like carts, favorites, reviews, and responses.

**13\. Reviews**

* **review\_id** (PK, INT, Auto-increment)
* product\_id (FK \-\> Products.product\_id, INT, NOT NULL)
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)
* order\_id (FK \-\> Orders.order\_id, INT, NOT NULL) \- *For "Verified Purchase" validation.*
* rating (INT, NOT NULL) \- *1-5 stars.*
* title (VARCHAR(255), NULLABLE) \- *Review headline.*
* comment (TEXT, NULLABLE)
* status (ENUM('pending', 'approved', 'rejected'), DEFAULT 'pending') \- *Admin moderation.*
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**14\. ReviewResponses**

* **response\_id** (PK, INT, Auto-increment)  
* review\_id (FK \-\> Reviews.review\_id, INT, UNIQUE, NOT NULL) \- *One response per review.*  
* seller\_id (FK \-\> Users.user\_id, INT, NOT NULL)  
* response\_text (TEXT, NOT NULL)  
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**15\. Favorites (Junction Table)**

* **user\_id** (PK, FK \-\> Users.user\_id)
* **product\_id** (PK, FK \-\> Products.product\_id)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)
* *(Composite Primary Key)*

**16\. CartItems (Junction Table)**

* **user\_id** (PK, FK \-\> Users.user\_id)
* **product\_id** (PK, FK \-\> Products.product\_id)
* quantity (INT, NOT NULL, DEFAULT 1)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP) \- *For cart abandonment tracking.*

#### 

#### **Part 4: Order & Financial Tables**

These tables manage the billing, splitting of orders to sellers, and discounts.

**17\. Discounts**

* **discount\_id** (PK, INT, Auto-increment)
* shop\_id (FK \-\> Shops.shop\_id, INT, **NULLABLE**) \- *If NULL, it's an Admin code (site-wide).*
* code (VARCHAR(50), UNIQUE, NOT NULL)
* discount\_type (ENUM('percentage', 'fixed\_amount'), NOT NULL)
* value (DECIMAL(10, 2), NOT NULL)
* minimum\_order\_amount (DECIMAL(10, 2), DEFAULT 0) \- *Minimum order value to apply.*
* usage\_limit (INT, NULLABLE) \- *Total times this code can be used.*
* usage\_count (INT, DEFAULT 0) \- *Times this code has been used.*
* per\_user\_limit (INT, DEFAULT 1) \- *Max uses per customer.*
* expires\_at (TIMESTAMP, NULLABLE)
* is\_active (BOOLEAN, DEFAULT TRUE)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**18\. Orders (Main Order)**

* **order\_id** (PK, INT, Auto-increment)  
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)  
* shipping\_address\_id (FK \-\> Addresses.address\_id, INT, NOT NULL)  
* order\_date (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)  
* subtotal\_amount (DECIMAL(10, 2), NOT NULL)  
* discount\_id (FK \-\> Discounts.discount\_id, INT, NULLABLE) \- *Site-wide (Admin) discount used.*  
* total\_amount (DECIMAL(10, 2), NOT NULL) \- *Final amount paid.*  
* payment\_id (VARCHAR(255), NULLABLE) \- *ID from the payment gateway.*

**19\. ShopOrders (Sub-Orders)**

* **shop\_order\_id** (PK, INT, Auto-increment)  
* order\_id (FK \-\> Orders.order\_id, INT, NOT NULL) \- *Links to the main order.*  
* shop\_id (FK \-\> Shops.shop\_id, INT, NOT NULL) \- *Links to the seller.*  
* shop\_order\_status (ENUM('pending', 'processing', 'shipped', ...), DEFAULT 'pending')  
* tracking\_number (VARCHAR(100), NULLABLE)  
* shop\_subtotal (DECIMAL(10, 2), NOT NULL)  
* discount\_id (FK \-\> Discounts.discount\_id, INT, NULLABLE) \- *Shop-specific (Seller) discount used.*  
* shop\_total (DECIMAL(10, 2), NOT NULL)

**20\. OrderItems**

* **order\_item\_id** (PK, INT, Auto-increment)  
* shop\_order\_id (FK \-\> ShopOrders.shop\_order\_id, INT, NOT NULL) \- *Links to the sub-order.*  
* product\_id (FK \-\> Products.product\_id, INT, NOT NULL)  
* quantity (INT, NOT NULL)  
* price\_at\_purchase (DECIMAL(10, 2), NOT NULL) \- *The price of the product at the time of sale.*

**Part 5: System Tables**

**21\. Notifications**

* **notification\_id** (PK, INT, Auto-increment)
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL) \- *The recipient of the notification.*
* message (TEXT, NOT NULL) \- *e.g., "Your order \#123 has been shipped\!"*
* link\_url (VARCHAR(255), NULLABLE) \- *Link to click on (e.g., /my-orders/123).*
* is\_read (BOOLEAN, DEFAULT FALSE)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

---

#### **Part 6: Additional Tables**

**22\. ProductQuestions**

* **question\_id** (PK, INT, Auto-increment)
* product\_id (FK \-\> Products.product\_id, INT, NOT NULL)
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)
* question\_text (TEXT, NOT NULL)
* is\_answered (BOOLEAN, DEFAULT FALSE)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**23\. QuestionAnswers**

* **answer\_id** (PK, INT, Auto-increment)
* question\_id (FK \-\> ProductQuestions.question\_id, INT, NOT NULL)
* seller\_id (FK \-\> Users.user\_id, INT, NOT NULL)
* answer\_text (TEXT, NOT NULL)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**24\. Payments**

* **payment\_id** (PK, INT, Auto-increment)
* order\_id (FK \-\> Orders.order\_id, INT, NOT NULL)
* payment\_method (ENUM('credit\_card', 'debit\_card', 'paypal', 'bank\_transfer'), NOT NULL)
* payment\_status (ENUM('pending', 'completed', 'failed', 'refunded'), DEFAULT 'pending')
* transaction\_id (VARCHAR(255), NULLABLE) \- *ID from payment gateway.*
* amount (DECIMAL(10, 2), NOT NULL)
* currency (VARCHAR(3), DEFAULT 'USD')
* paid\_at (TIMESTAMP, NULLABLE)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**25\. Returns**

* **return\_id** (PK, INT, Auto-increment)
* shop\_order\_id (FK \-\> ShopOrders.shop\_order\_id, INT, NOT NULL)
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)
* return\_reason (VARCHAR(255), NOT NULL)
* return\_status (ENUM('pending', 'approved', 'rejected', 'completed'), DEFAULT 'pending')
* refund\_amount (DECIMAL(10, 2), NULLABLE)
* admin\_notes (TEXT, NULLABLE)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)
* updated\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP ON UPDATE CURRENT\_TIMESTAMP)

**26\. ReturnItems**

* **return\_item\_id** (PK, INT, Auto-increment)
* return\_id (FK \-\> Returns.return\_id, INT, NOT NULL)
* order\_item\_id (FK \-\> OrderItems.order\_item\_id, INT, NOT NULL)
* quantity (INT, NOT NULL)

**27\. OrderStatusHistory**

* **history\_id** (PK, INT, Auto-increment)
* shop\_order\_id (FK \-\> ShopOrders.shop\_order\_id, INT, NOT NULL)
* old\_status (ENUM('pending', 'processing', 'shipped', 'delivered', 'cancelled'), NULLABLE)
* new\_status (ENUM('pending', 'processing', 'shipped', 'delivered', 'cancelled'), NOT NULL)
* changed\_by (FK \-\> Users.user\_id, INT, NOT NULL) \- *Seller or Admin who changed status.*
* notes (TEXT, NULLABLE)
* created\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

**28\. DiscountUsage**

* **usage\_id** (PK, INT, Auto-increment)
* discount\_id (FK \-\> Discounts.discount\_id, INT, NOT NULL)
* user\_id (FK \-\> Users.user\_id, INT, NOT NULL)
* order\_id (FK \-\> Orders.order\_id, INT, NOT NULL)
* used\_at (TIMESTAMP, DEFAULT CURRENT\_TIMESTAMP)

---

### **User and Shop Relationships**

* There is a **One-to-Many (1-N)** relationship between Roles and Users (One role can have many users).  
* There is a **One-to-One (1-1)** relationship between Users (with the 'seller' role) and Shops (One seller user can have only one shop).  
* There is a **One-to-Many (1-N)** relationship between Users and Addresses (One user can have many addresses).  
* There is a **One-to-Many (1-N)** relationship between Users and PasswordResets (One user can make many password reset requests).

### **Catalog and Product Relationships**

* The Categories table has a **One-to-Many (1-N)** relationship **with itself** (One parent category can have many sub-categories).  
* There is a **One-to-Many (1-N)** relationship between Shops and Products (One shop can sell many products).  
* There is a **One-to-Many (1-N)** relationship between Categories and Products (One category can contain many products).  
* There is a **One-to-Many (1-N)** relationship between Products and ProductImages (One product can have many images).

### **Product Attribute (Filtering) Relationships**

* There is a **One-to-Many (1-N)** relationship between Attributes and AttributeOptions (One attribute, e.g., "Color", can have many options, e.g., "Black", "Blue").  
* There is a **Many-to-Many (M-N)** relationship between Categories and Attributes via the CategoryAttributes **junction table** (One category can require many attributes; one attribute can be used in many categories).  
* There is a **Many-to-Many (M-N)** relationship between Products and AttributeOptions via the ProductAttributeValues**junction table** (One product can have many selected options; one option can be on many products).

### **Interaction (Cart, Favorite, Review) Relationships**

* There is a **Many-to-Many (M-N)** relationship between Users and Products via the CartItems **junction table** (The user's cart).  
* There is a **Many-to-Many (M-N)** relationship between Users and Products via the Favorites **junction table** (The user's favorites).  
* There is a **One-to-Many (1-N)** relationship between Products and Reviews (One product can have many reviews).  
* There is a **One-to-Many (1-N)** relationship between Users and Reviews (One user can write many reviews).  
* There is a **One-to-One (1-1)** relationship between Reviews and ReviewResponses (One review can have only one seller response).

### **Order and Financial Relationships**

* There is a **One-to-Many (1-N)** relationship between Users and Orders (One user can place many main orders).  
* There is a **One-to-Many (1-N)** relationship between Orders (Main Order) and ShopOrders (Sub-Order) (One main order can be split into many sub-orders for different sellers).  
* There is a **One-to-Many (1-N)** relationship between Shops and ShopOrders (One shop can have many sub-orders).  
* There is a **One-to-Many (1-N)** relationship between ShopOrders and OrderItems (One shop order can contain many product items).  
* There is a **One-to-Many (1-N)** relationship between Products and OrderItems (One product can be in many order items).  
* There is an **Optional One-to-Many (1-N)** relationship between Orders and Reviews (for validation).  
* There is an **Optional One-to-Many (1-N)** relationship between Discounts and Orders (for Admin discounts).  
* There is an **Optional One-to-Many (1-N)** relationship between Discounts and ShopOrders (for Seller discounts).

### **System Relationships**

* There is a **One-to-Many (1-N)** relationship between Users and Notifications (One user can receive many notifications).

### **Additional Table Relationships**

* There is a **One-to-Many (1-N)** relationship between Products and ProductQuestions (One product can have many questions).
* There is a **One-to-Many (1-N)** relationship between Users and ProductQuestions (One user can ask many questions).
* There is a **One-to-One (1-1)** relationship between ProductQuestions and QuestionAnswers (One question can have one seller answer).
* There is a **One-to-One (1-1)** relationship between Orders and Payments (One order has one payment record).
* There is a **One-to-Many (1-N)** relationship between ShopOrders and Returns (One shop order can have multiple return requests).
* There is a **One-to-Many (1-N)** relationship between Returns and ReturnItems (One return can contain many items).
* There is a **One-to-Many (1-N)** relationship between ShopOrders and OrderStatusHistory (One shop order can have many status changes).
* There is a **One-to-Many (1-N)** relationship between Discounts and DiscountUsage (One discount can be used many times).
* There is a **One-to-Many (1-N)** relationship between Users and DiscountUsage (One user can use many discounts).