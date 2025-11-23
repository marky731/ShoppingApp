-- Cleanup duplicate categories in ShoppingApp
-- Run with: mysql -u root -p < database/cleanup_duplicates.sql

USE shopping_app;

-- First, update products to use the correct category IDs
-- Keep: Electronics=1, Clothing=2, Home=7, Beauty=8, Accessories=9
-- Subcategories: Phones=3, Laptops=4, Bags=17, Shoes=18, Kitchen=19, Watches=20

-- Update products with duplicate parent categories
UPDATE Products SET category_id = 1 WHERE category_id IN (5, 10, 21);
UPDATE Products SET category_id = 2 WHERE category_id IN (6, 11, 22);
UPDATE Products SET category_id = 7 WHERE category_id IN (12, 23);
UPDATE Products SET category_id = 8 WHERE category_id IN (13, 24);
UPDATE Products SET category_id = 9 WHERE category_id IN (14, 25);

-- Update products with duplicate subcategories
UPDATE Products SET category_id = 3 WHERE category_id IN (15, 26);
UPDATE Products SET category_id = 4 WHERE category_id IN (16, 27);
UPDATE Products SET category_id = 17 WHERE category_id IN (28);
UPDATE Products SET category_id = 18 WHERE category_id IN (29);
UPDATE Products SET category_id = 19 WHERE category_id IN (30);
UPDATE Products SET category_id = 20 WHERE category_id IN (31);

-- Delete duplicate subcategories first (they reference parent categories)
DELETE FROM Categories WHERE category_id IN (15, 16, 26, 27, 28, 29, 30, 31);

-- Delete duplicate parent categories
DELETE FROM Categories WHERE category_id IN (5, 6, 10, 11, 12, 13, 14, 21, 22, 23, 24, 25);

-- Verify cleanup
SELECT 'Remaining categories:' AS Status;
SELECT category_id, category_name, parent_category_id FROM Categories ORDER BY parent_category_id, category_name;

SELECT CONCAT('Total categories: ', COUNT(*)) AS Summary FROM Categories;
