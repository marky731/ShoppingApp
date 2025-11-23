-- ShoppingApp Database Schema
-- 28 Tables for Multi-Seller E-Commerce Platform

CREATE DATABASE IF NOT EXISTS shopping_app;
USE shopping_app;

-- =============================================
-- Part 1: User, Auth & Profile Tables
-- =============================================

-- 1. Roles
CREATE TABLE Roles (
    role_id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(50) UNIQUE NOT NULL
);

-- Insert default roles
INSERT INTO Roles (role_name) VALUES ('customer'), ('seller'), ('admin');

-- 2. Users
CREATE TABLE Users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    role_id INT NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (role_id) REFERENCES Roles(role_id)
);

-- 3. Addresses
CREATE TABLE Addresses (
    address_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    address_label VARCHAR(100) NOT NULL,
    street_address VARCHAR(255) NOT NULL,
    city VARCHAR(100) NOT NULL,
    postal_code VARCHAR(20),
    country VARCHAR(100) NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- 4. PasswordResets
CREATE TABLE PasswordResets (
    reset_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    reset_token VARCHAR(255) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- =============================================
-- Part 2: Catalog & Shop Tables
-- =============================================

-- 5. Shops
CREATE TABLE Shops (
    shop_id INT AUTO_INCREMENT PRIMARY KEY,
    seller_id INT UNIQUE NOT NULL,
    shop_name VARCHAR(255) NOT NULL,
    description TEXT,
    logo_image_url VARCHAR(255),
    banner_image_url VARCHAR(255),
    average_rating DECIMAL(3, 2) DEFAULT 0,
    total_sales INT DEFAULT 0,
    is_approved BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (seller_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- 6. Categories
CREATE TABLE Categories (
    category_id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    parent_category_id INT,
    FOREIGN KEY (parent_category_id) REFERENCES Categories(category_id) ON DELETE SET NULL
);

-- 7. Products
CREATE TABLE Products (
    product_id INT AUTO_INCREMENT PRIMARY KEY,
    shop_id INT NOT NULL,
    category_id INT NOT NULL,
    product_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0,
    main_image_url VARCHAR(255),
    brand VARCHAR(100),
    model VARCHAR(100),
    average_rating DECIMAL(3, 2) DEFAULT 0,
    total_reviews INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (shop_id) REFERENCES Shops(shop_id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES Categories(category_id)
);

-- 8. ProductImages
CREATE TABLE ProductImages (
    image_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    image_url VARCHAR(255) NOT NULL,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);

-- 9. Attributes
CREATE TABLE Attributes (
    attribute_id INT AUTO_INCREMENT PRIMARY KEY,
    attribute_name VARCHAR(100) UNIQUE NOT NULL
);

-- 10. AttributeOptions
CREATE TABLE AttributeOptions (
    option_id INT AUTO_INCREMENT PRIMARY KEY,
    attribute_id INT NOT NULL,
    option_value VARCHAR(100) NOT NULL,
    FOREIGN KEY (attribute_id) REFERENCES Attributes(attribute_id) ON DELETE CASCADE
);

-- 11. CategoryAttributes (Junction Table)
CREATE TABLE CategoryAttributes (
    category_id INT NOT NULL,
    attribute_id INT NOT NULL,
    PRIMARY KEY (category_id, attribute_id),
    FOREIGN KEY (category_id) REFERENCES Categories(category_id) ON DELETE CASCADE,
    FOREIGN KEY (attribute_id) REFERENCES Attributes(attribute_id) ON DELETE CASCADE
);

-- 12. ProductAttributeValues
CREATE TABLE ProductAttributeValues (
    value_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    option_id INT NOT NULL,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (option_id) REFERENCES AttributeOptions(option_id) ON DELETE CASCADE
);

-- =============================================
-- Part 3: User Interaction Tables
-- =============================================

-- 13. Reviews (forward declaration needed for Orders FK)
-- Will be created after Orders table

-- 14. ReviewResponses
-- Will be created after Reviews table

-- 15. Favorites (Junction Table)
CREATE TABLE Favorites (
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, product_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);

-- 16. CartItems (Junction Table)
CREATE TABLE CartItems (
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, product_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE
);

-- =============================================
-- Part 4: Order & Financial Tables
-- =============================================

-- 17. Discounts
CREATE TABLE Discounts (
    discount_id INT AUTO_INCREMENT PRIMARY KEY,
    shop_id INT,
    code VARCHAR(50) UNIQUE NOT NULL,
    discount_type ENUM('percentage', 'fixed_amount') NOT NULL,
    value DECIMAL(10, 2) NOT NULL,
    minimum_order_amount DECIMAL(10, 2) DEFAULT 0,
    usage_limit INT,
    usage_count INT DEFAULT 0,
    per_user_limit INT DEFAULT 1,
    expires_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (shop_id) REFERENCES Shops(shop_id) ON DELETE CASCADE
);

-- 18. Orders (Main Order)
CREATE TABLE Orders (
    order_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    shipping_address_id INT NOT NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    subtotal_amount DECIMAL(10, 2) NOT NULL,
    discount_id INT,
    total_amount DECIMAL(10, 2) NOT NULL,
    payment_id VARCHAR(255),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (shipping_address_id) REFERENCES Addresses(address_id),
    FOREIGN KEY (discount_id) REFERENCES Discounts(discount_id)
);

-- 19. ShopOrders (Sub-Orders)
CREATE TABLE ShopOrders (
    shop_order_id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    shop_id INT NOT NULL,
    shop_order_status ENUM('pending', 'processing', 'shipped', 'delivered', 'cancelled') DEFAULT 'pending',
    tracking_number VARCHAR(100),
    shop_subtotal DECIMAL(10, 2) NOT NULL,
    discount_id INT,
    shop_total DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (shop_id) REFERENCES Shops(shop_id),
    FOREIGN KEY (discount_id) REFERENCES Discounts(discount_id)
);

-- 20. OrderItems
CREATE TABLE OrderItems (
    order_item_id INT AUTO_INCREMENT PRIMARY KEY,
    shop_order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    price_at_purchase DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (shop_order_id) REFERENCES ShopOrders(shop_order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);

-- =============================================
-- Part 5: System Tables
-- =============================================

-- 21. Notifications
CREATE TABLE Notifications (
    notification_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    message TEXT NOT NULL,
    link_url VARCHAR(255),
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- =============================================
-- Part 3 (continued): Reviews (needs Orders to exist)
-- =============================================

-- 13. Reviews
CREATE TABLE Reviews (
    review_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    user_id INT NOT NULL,
    order_id INT NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(255),
    comment TEXT,
    status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id)
);

-- 14. ReviewResponses
CREATE TABLE ReviewResponses (
    response_id INT AUTO_INCREMENT PRIMARY KEY,
    review_id INT UNIQUE NOT NULL,
    seller_id INT NOT NULL,
    response_text TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (review_id) REFERENCES Reviews(review_id) ON DELETE CASCADE,
    FOREIGN KEY (seller_id) REFERENCES Users(user_id)
);

-- =============================================
-- Part 6: Additional Tables
-- =============================================

-- 22. ProductQuestions
CREATE TABLE ProductQuestions (
    question_id INT AUTO_INCREMENT PRIMARY KEY,
    product_id INT NOT NULL,
    user_id INT NOT NULL,
    question_text TEXT NOT NULL,
    is_answered BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES Products(product_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

-- 23. QuestionAnswers
CREATE TABLE QuestionAnswers (
    answer_id INT AUTO_INCREMENT PRIMARY KEY,
    question_id INT NOT NULL,
    seller_id INT NOT NULL,
    answer_text TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (question_id) REFERENCES ProductQuestions(question_id) ON DELETE CASCADE,
    FOREIGN KEY (seller_id) REFERENCES Users(user_id)
);

-- 24. Payments
CREATE TABLE Payments (
    payment_id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    payment_method ENUM('credit_card', 'debit_card', 'paypal', 'bank_transfer') NOT NULL,
    payment_status ENUM('pending', 'completed', 'failed', 'refunded') DEFAULT 'pending',
    transaction_id VARCHAR(255),
    amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'USD',
    paid_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE
);

-- 25. Returns
CREATE TABLE Returns (
    return_id INT AUTO_INCREMENT PRIMARY KEY,
    shop_order_id INT NOT NULL,
    user_id INT NOT NULL,
    return_reason VARCHAR(255) NOT NULL,
    return_status ENUM('pending', 'approved', 'rejected', 'completed') DEFAULT 'pending',
    refund_amount DECIMAL(10, 2),
    admin_notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (shop_order_id) REFERENCES ShopOrders(shop_order_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

-- 26. ReturnItems
CREATE TABLE ReturnItems (
    return_item_id INT AUTO_INCREMENT PRIMARY KEY,
    return_id INT NOT NULL,
    order_item_id INT NOT NULL,
    quantity INT NOT NULL,
    FOREIGN KEY (return_id) REFERENCES Returns(return_id) ON DELETE CASCADE,
    FOREIGN KEY (order_item_id) REFERENCES OrderItems(order_item_id)
);

-- 27. OrderStatusHistory
CREATE TABLE OrderStatusHistory (
    history_id INT AUTO_INCREMENT PRIMARY KEY,
    shop_order_id INT NOT NULL,
    old_status ENUM('pending', 'processing', 'shipped', 'delivered', 'cancelled'),
    new_status ENUM('pending', 'processing', 'shipped', 'delivered', 'cancelled') NOT NULL,
    changed_by INT NOT NULL,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (shop_order_id) REFERENCES ShopOrders(shop_order_id) ON DELETE CASCADE,
    FOREIGN KEY (changed_by) REFERENCES Users(user_id)
);

-- 28. DiscountUsage
CREATE TABLE DiscountUsage (
    usage_id INT AUTO_INCREMENT PRIMARY KEY,
    discount_id INT NOT NULL,
    user_id INT NOT NULL,
    order_id INT NOT NULL,
    used_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (discount_id) REFERENCES Discounts(discount_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (order_id) REFERENCES Orders(order_id)
);

-- =============================================
-- Indexes for Performance
-- =============================================

-- Products indexes
CREATE INDEX idx_products_shop ON Products(shop_id);
CREATE INDEX idx_products_category ON Products(category_id);
CREATE INDEX idx_products_price ON Products(price);
CREATE INDEX idx_products_brand ON Products(brand);
CREATE INDEX idx_products_active ON Products(is_active);
CREATE FULLTEXT INDEX idx_products_search ON Products(product_name, description);

-- Orders indexes
CREATE INDEX idx_orders_user ON Orders(user_id);
CREATE INDEX idx_orders_date ON Orders(order_date);
CREATE INDEX idx_shop_orders_status ON ShopOrders(shop_order_status);

-- Reviews indexes
CREATE INDEX idx_reviews_product ON Reviews(product_id);
CREATE INDEX idx_reviews_status ON Reviews(status);

-- User indexes
CREATE INDEX idx_users_email ON Users(email);
CREATE INDEX idx_users_role ON Users(role_id);
