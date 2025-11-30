-- Sample Data for ShoppingApp
-- Run this after schema.sql to populate test data
-- Safe to run multiple times (idempotent)

USE shopping_app;

-- Temporarily disable foreign key checks for clean reset
SET FOREIGN_KEY_CHECKS = 0;

-- Clean existing sample data first
DELETE FROM OrderItems WHERE product_id IN (SELECT product_id FROM Products WHERE shop_id IN (SELECT shop_id FROM Shops WHERE shop_name IN ('TechHub', 'Fashion Forward', 'HomeStyle', 'Time & Style', 'Minimal Shop')));
DELETE FROM CartItems WHERE product_id IN (SELECT product_id FROM Products WHERE shop_id IN (SELECT shop_id FROM Shops WHERE shop_name IN ('TechHub', 'Fashion Forward', 'HomeStyle', 'Time & Style', 'Minimal Shop')));
DELETE FROM Favorites WHERE product_id IN (SELECT product_id FROM Products WHERE shop_id IN (SELECT shop_id FROM Shops WHERE shop_name IN ('TechHub', 'Fashion Forward', 'HomeStyle', 'Time & Style', 'Minimal Shop')));
DELETE FROM Reviews WHERE product_id IN (SELECT product_id FROM Products WHERE shop_id IN (SELECT shop_id FROM Shops WHERE shop_name IN ('TechHub', 'Fashion Forward', 'HomeStyle', 'Time & Style', 'Minimal Shop')));
DELETE FROM Products WHERE shop_id IN (SELECT shop_id FROM Shops WHERE shop_name IN ('TechHub', 'Fashion Forward', 'HomeStyle', 'Time & Style', 'Minimal Shop'));
DELETE FROM Shops WHERE shop_name IN ('TechHub', 'Fashion Forward', 'HomeStyle', 'Time & Style', 'Minimal Shop');
DELETE FROM Categories WHERE category_name IN ('Phones', 'Laptops', 'Bags', 'Shoes', 'Kitchen', 'Watches');
DELETE FROM Categories WHERE category_name IN ('Electronics', 'Clothing', 'Home', 'Beauty', 'Accessories') AND parent_category_id IS NULL;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- =============================================
-- USERS (password for all: Test1234)
-- =============================================
-- BCrypt hash for "Test1234"
SET @password_hash = '$2a$11$RhaSk9C68R5HtU75NL6DUeYOr8oGsuX1dpQQSe6WQnrjeBzmU8/Yy';

-- Admin user
INSERT INTO Users (role_id, first_name, last_name, email, password_hash, phone, is_active, created_at, updated_at)
VALUES (3, 'Admin', 'User', 'admin@example.com', @password_hash, '555-0001', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE first_name = VALUES(first_name);

-- Seller 1: TechHub (Electronics)
INSERT INTO Users (role_id, first_name, last_name, email, password_hash, phone, is_active, created_at, updated_at)
VALUES (2, 'Tech', 'Seller', 'seller@example.com', @password_hash, '555-0100', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE first_name = VALUES(first_name);

-- Seller 2: Fashion Forward (Clothing)
INSERT INTO Users (role_id, first_name, last_name, email, password_hash, phone, is_active, created_at, updated_at)
VALUES (2, 'Fashion', 'Seller', 'seller2@example.com', @password_hash, '555-0200', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE first_name = VALUES(first_name);

-- Seller 3: HomeStyle (Home & Kitchen)
INSERT INTO Users (role_id, first_name, last_name, email, password_hash, phone, is_active, created_at, updated_at)
VALUES (2, 'Home', 'Seller', 'seller3@example.com', @password_hash, '555-0300', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE first_name = VALUES(first_name);

-- Seller 4: Time & Style (Accessories)
INSERT INTO Users (role_id, first_name, last_name, email, password_hash, phone, is_active, created_at, updated_at)
VALUES (2, 'Style', 'Seller', 'seller4@example.com', @password_hash, '555-0400', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE first_name = VALUES(first_name);

-- Customer user
INSERT INTO Users (role_id, first_name, last_name, email, password_hash, phone, is_active, created_at, updated_at)
VALUES (1, 'John', 'Doe', 'john@example.com', @password_hash, '555-0101', 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE first_name = VALUES(first_name);

-- =============================================
-- SHOPS (one per seller)
-- =============================================
SET @seller1_id = (SELECT user_id FROM Users WHERE email = 'seller@example.com');
SET @seller2_id = (SELECT user_id FROM Users WHERE email = 'seller2@example.com');
SET @seller3_id = (SELECT user_id FROM Users WHERE email = 'seller3@example.com');
SET @seller4_id = (SELECT user_id FROM Users WHERE email = 'seller4@example.com');

-- Shop 1: TechHub (Electronics)
INSERT INTO Shops (seller_id, shop_name, description, logo_image_url, is_approved, average_rating, total_sales, created_at, updated_at)
VALUES (@seller1_id, 'TechHub', 'Your one-stop shop for premium electronics and gadgets', 'https://images.unsplash.com/photo-1531297484001-80022131f5a1?w=200', 1, 4.8, 250, NOW(), NOW())
ON DUPLICATE KEY UPDATE shop_name = VALUES(shop_name);

-- Shop 2: Fashion Forward (Clothing)
INSERT INTO Shops (seller_id, shop_name, description, logo_image_url, is_approved, average_rating, total_sales, created_at, updated_at)
VALUES (@seller2_id, 'Fashion Forward', 'Trendy bags and shoes for the modern lifestyle', 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=200', 1, 4.6, 180, NOW(), NOW())
ON DUPLICATE KEY UPDATE shop_name = VALUES(shop_name);

-- Shop 3: HomeStyle (Home & Kitchen)
INSERT INTO Shops (seller_id, shop_name, description, logo_image_url, is_approved, average_rating, total_sales, created_at, updated_at)
VALUES (@seller3_id, 'HomeStyle', 'Elegant home and kitchen essentials for modern living', 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=200', 1, 4.5, 120, NOW(), NOW())
ON DUPLICATE KEY UPDATE shop_name = VALUES(shop_name);

-- Shop 4: Time & Style (Accessories)
INSERT INTO Shops (seller_id, shop_name, description, logo_image_url, is_approved, average_rating, total_sales, created_at, updated_at)
VALUES (@seller4_id, 'Time & Style', 'Curated collection of elegant timepieces and accessories', 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=200', 1, 4.7, 95, NOW(), NOW())
ON DUPLICATE KEY UPDATE shop_name = VALUES(shop_name);

-- Get shop IDs
SET @shop1_id = (SELECT shop_id FROM Shops WHERE seller_id = @seller1_id);
SET @shop2_id = (SELECT shop_id FROM Shops WHERE seller_id = @seller2_id);
SET @shop3_id = (SELECT shop_id FROM Shops WHERE seller_id = @seller3_id);
SET @shop4_id = (SELECT shop_id FROM Shops WHERE seller_id = @seller4_id);

-- =============================================
-- CATEGORIES
-- =============================================
-- Add parent categories
INSERT INTO Categories (category_name, parent_category_id) VALUES
('Electronics', NULL),
('Clothing', NULL),
('Home', NULL),
('Beauty', NULL),
('Accessories', NULL);

-- Get parent category IDs
SET @electronics_id = (SELECT category_id FROM Categories WHERE category_name = 'Electronics' AND parent_category_id IS NULL);
SET @clothing_id = (SELECT category_id FROM Categories WHERE category_name = 'Clothing' AND parent_category_id IS NULL);
SET @home_id = (SELECT category_id FROM Categories WHERE category_name = 'Home' AND parent_category_id IS NULL);
SET @accessories_id = (SELECT category_id FROM Categories WHERE category_name = 'Accessories' AND parent_category_id IS NULL);

-- Add subcategories
INSERT INTO Categories (category_name, parent_category_id) VALUES
('Phones', @electronics_id),
('Laptops', @electronics_id),
('Bags', @clothing_id),
('Shoes', @clothing_id),
('Kitchen', @home_id),
('Watches', @accessories_id);

-- Get subcategory IDs
SET @phones_id = (SELECT category_id FROM Categories WHERE category_name = 'Phones' AND parent_category_id = @electronics_id);
SET @laptops_id = (SELECT category_id FROM Categories WHERE category_name = 'Laptops' AND parent_category_id = @electronics_id);
SET @bags_id = (SELECT category_id FROM Categories WHERE category_name = 'Bags' AND parent_category_id = @clothing_id);
SET @shoes_id = (SELECT category_id FROM Categories WHERE category_name = 'Shoes' AND parent_category_id = @clothing_id);
SET @kitchen_id = (SELECT category_id FROM Categories WHERE category_name = 'Kitchen' AND parent_category_id = @home_id);
SET @watches_id = (SELECT category_id FROM Categories WHERE category_name = 'Watches' AND parent_category_id = @accessories_id);

-- =============================================
-- PRODUCTS (distributed among shops)
-- =============================================

-- ========== SHOP 1: TechHub (Electronics) ==========
INSERT INTO Products (shop_id, category_id, product_name, slug, description, price, stock_quantity, main_image_url, brand, model, average_rating, total_reviews, is_active, created_at, updated_at) VALUES

-- Phones
(@shop1_id, @phones_id, 'iPhone 15 Pro', 'iphone-15-pro',
'The latest iPhone with A17 Pro chip, titanium design, and advanced camera system. Features a 6.1-inch Super Retina XDR display.',
999.00, 25, 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500', 'Apple', 'iPhone 15 Pro', 4.8, 156, 1, NOW(), NOW()),

(@shop1_id, @phones_id, 'Samsung Galaxy S24', 'samsung-galaxy-s24',
'Experience Galaxy AI with the Samsung Galaxy S24. Features a dynamic AMOLED display and advanced camera capabilities.',
849.00, 30, 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=500', 'Samsung', 'Galaxy S24', 4.7, 98, 1, NOW(), NOW()),

(@shop1_id, @phones_id, 'Google Pixel 8', 'google-pixel-8',
'Pure Android experience with AI-powered camera and 7 years of updates.',
699.00, 20, 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500', 'Google', 'Pixel 8', 4.6, 78, 1, NOW(), NOW()),

(@shop1_id, @phones_id, 'OnePlus 12', 'oneplus-12',
'Flagship killer with Snapdragon 8 Gen 3 and Hasselblad camera.',
799.00, 15, 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500', 'OnePlus', '12', 4.5, 45, 1, NOW(), NOW()),

-- Laptops
(@shop1_id, @laptops_id, 'MacBook Pro 14"', 'macbook-pro-14',
'Supercharged by M3 Pro chip. Features a stunning Liquid Retina XDR display and up to 18 hours of battery life.',
1999.00, 15, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500', 'Apple', 'MacBook Pro 14', 4.9, 234, 1, NOW(), NOW()),

(@shop1_id, @laptops_id, 'Dell XPS 15', 'dell-xps-15',
'Stunning 15.6 OLED display with Intel Core i7 and 32GB RAM.',
1799.00, 10, 'https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=500', 'Dell', 'XPS 15', 4.7, 112, 1, NOW(), NOW()),

(@shop1_id, @laptops_id, 'ThinkPad X1 Carbon', 'thinkpad-x1-carbon',
'Business ultrabook with legendary keyboard and all-day battery.',
1499.00, 12, 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500', 'Lenovo', 'X1 Carbon Gen 11', 4.8, 89, 1, NOW(), NOW());

-- ========== SHOP 2: Fashion Forward (Clothing) ==========
INSERT INTO Products (shop_id, category_id, product_name, slug, description, price, stock_quantity, main_image_url, brand, model, average_rating, total_reviews, is_active, created_at, updated_at) VALUES

-- Bags
(@shop2_id, @bags_id, 'White Linen Tote Bag', 'white-linen-tote-bag',
'Minimalist white linen tote bag perfect for everyday use. Eco-friendly and durable with reinforced handles.',
9.99, 100, 'https://images.unsplash.com/photo-1544816155-12df9643f363?w=500', 'Fashion Forward', 'Classic Tote', 4.5, 67, 1, NOW(), NOW()),

(@shop2_id, @bags_id, 'Canvas Messenger Bag', 'canvas-messenger-bag',
'Vintage-style messenger bag with leather accents and padded laptop sleeve.',
49.99, 60, 'https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=500', 'Fashion Forward', 'Urban Messenger', 4.4, 34, 1, NOW(), NOW()),

(@shop2_id, @bags_id, 'Leather Backpack', 'leather-backpack',
'Premium full-grain leather backpack with anti-theft pocket.',
129.99, 25, 'https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=500', 'Fashion Forward', 'Heritage Pack', 4.7, 56, 1, NOW(), NOW()),

-- Shoes
(@shop2_id, @shoes_id, 'White Lace-Up Sneakers', 'white-lace-up-sneakers',
'Classic white sneakers with a modern minimal design. Comfortable cushioned sole for all-day wear.',
59.99, 50, 'https://images.unsplash.com/photo-1549298916-b41d501d3772?w=500', 'Fashion Forward', 'Urban Step', 4.6, 89, 1, NOW(), NOW()),

(@shop2_id, @shoes_id, 'Running Shoes', 'running-shoes',
'Lightweight running shoes with responsive cushioning and breathable mesh.',
89.99, 45, 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500', 'Fashion Forward', 'Swift Runner', 4.5, 123, 1, NOW(), NOW()),

(@shop2_id, @shoes_id, 'Leather Loafers', 'leather-loafers',
'Classic leather loafers with cushioned insole for all-day comfort.',
79.99, 30, 'https://images.unsplash.com/photo-1614252369475-531eba835eb1?w=500', 'Fashion Forward', 'Classic Slip', 4.6, 67, 1, NOW(), NOW());

-- ========== SHOP 3: HomeStyle (Home & Kitchen) ==========
INSERT INTO Products (shop_id, category_id, product_name, slug, description, price, stock_quantity, main_image_url, brand, model, average_rating, total_reviews, is_active, created_at, updated_at) VALUES

(@shop3_id, @kitchen_id, 'Glass Water Bottle', 'glass-water-bottle',
'Eco-friendly borosilicate glass water bottle with bamboo lid. BPA-free and dishwasher safe.',
14.99, 75, 'https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=500', 'HomeStyle', 'Pure Glass', 4.4, 52, 1, NOW(), NOW()),

(@shop3_id, @kitchen_id, 'Ceramic Coffee Mug Set', 'ceramic-coffee-mug-set',
'Set of 4 minimalist ceramic mugs in neutral tones. Microwave and dishwasher safe.',
29.99, 35, 'https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?w=500', 'HomeStyle', 'Morning Set', 4.6, 78, 1, NOW(), NOW()),

(@shop3_id, @kitchen_id, 'Bamboo Cutting Board Set', 'bamboo-cutting-board-set',
'Set of 3 organic bamboo cutting boards in different sizes.',
34.99, 40, 'https://images.unsplash.com/photo-1594226801341-41427b4e5c22?w=500', 'HomeStyle', 'Kitchen Essentials', 4.5, 89, 1, NOW(), NOW()),

(@shop3_id, @kitchen_id, 'Stainless Steel Kettle', 'stainless-steel-kettle',
'Modern pour-over kettle with gooseneck spout and temperature gauge.',
54.99, 25, 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=500', 'HomeStyle', 'Precision Pour', 4.7, 98, 1, NOW(), NOW());

-- ========== SHOP 4: Time & Style (Accessories) ==========
INSERT INTO Products (shop_id, category_id, product_name, slug, description, price, stock_quantity, main_image_url, brand, model, average_rating, total_reviews, is_active, created_at, updated_at) VALUES

(@shop4_id, @watches_id, 'Silver Minimalist Watch', 'silver-minimalist-watch',
'Elegant silver watch with a clean dial design. Japanese quartz movement with stainless steel mesh band.',
39.99, 40, 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500', 'Time & Style', 'Time Classic', 4.7, 145, 1, NOW(), NOW()),

(@shop4_id, @watches_id, 'Black Chronograph Watch', 'black-chronograph-watch',
'Sporty chronograph with matte black case and silicone strap.',
59.99, 35, 'https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=500', 'Time & Style', 'Sport Chrono', 4.6, 78, 1, NOW(), NOW()),

(@shop4_id, @watches_id, 'Rose Gold Watch', 'rose-gold-watch',
'Elegant rose gold watch with white dial and leather strap.',
49.99, 30, 'https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?w=500', 'Time & Style', 'Elegance', 4.8, 134, 1, NOW(), NOW());

-- =============================================
-- SUMMARY
-- =============================================
SELECT 'Sample data inserted successfully!' AS Status;
SELECT 'Users:' AS '', COUNT(*) AS Count FROM Users;
SELECT 'Shops:' AS '', COUNT(*) AS Count FROM Shops;
SELECT 'Categories:' AS '', COUNT(*) AS Count FROM Categories;
SELECT 'Products:' AS '', COUNT(*) AS Count FROM Products;

-- Show product distribution per shop
SELECT s.shop_name AS Shop, COUNT(p.product_id) AS Products
FROM Shops s
LEFT JOIN Products p ON s.shop_id = p.shop_id
GROUP BY s.shop_id, s.shop_name;
