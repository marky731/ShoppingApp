#!/bin/bash

# Comprehensive Seller and Admin CRUD Testing Script

BASE_URL="http://localhost:8080"
RESULTS_FILE="seller_admin_test_results.txt"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

PASS_COUNT=0
FAIL_COUNT=0

echo "=========================================" | tee $RESULTS_FILE
echo "SELLER & ADMIN CRUD TEST SUITE" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE
echo "Target: $BASE_URL" | tee -a $RESULTS_FILE
echo "Date: $(date)" | tee -a $RESULTS_FILE
echo "" | tee -a $RESULTS_FILE

log_test() {
    local category=$1
    local endpoint=$2
    local method=$3
    local status=$4
    local expected=$5
    local notes=$6

    if [[ "$status" == "$expected" ]] || [[ "$expected" == *"$status"* ]]; then
        echo -e "${GREEN}✅ PASS${NC} [$category] $method $endpoint | Status: $status | $notes" | tee -a $RESULTS_FILE
        ((PASS_COUNT++))
    else
        echo -e "${RED}❌ FAIL${NC} [$category] $method $endpoint | Status: $status (expected $expected) | $notes" | tee -a $RESULTS_FILE
        ((FAIL_COUNT++))
    fi
}

# ==========================================
# SELLER TESTS
# ==========================================

echo "" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE
echo "TESTING SELLER ENDPOINTS" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE

# Login as Seller
echo "" | tee -a $RESULTS_FILE
echo "Logging in as seller..." | tee -a $RESULTS_FILE
curl -s -c /tmp/seller_cookies.txt "$BASE_URL/Account/Login" > /tmp/seller_login_page.html
SELLER_TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_login_page.html | head -1)

curl -s -b /tmp/seller_cookies.txt -c /tmp/seller_cookies.txt \
    -d "Email=seller@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$SELLER_TOKEN" \
    -L "$BASE_URL/Account/Login" > /tmp/seller_login_result.html

if grep -q "logout" /tmp/seller_login_result.html; then
    echo -e "${GREEN}✅ Successfully logged in as seller${NC}" | tee -a $RESULTS_FILE
else
    echo -e "${RED}❌ Failed to login as seller${NC}" | tee -a $RESULTS_FILE
    exit 1
fi

# Test Seller GET endpoints
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Seller GET Endpoints ---" | tee -a $RESULTS_FILE

endpoints=(
    "/Seller/Dashboard:Dashboard"
    "/Seller/Products:Products list"
    "/Seller/Products/Create:Create product form"
    "/Seller/Orders:Orders list"
    "/Seller/Discounts:Discounts list"
    "/Seller/Discounts/Create:Create discount form"
    "/Seller/Shop/Edit:Shop settings"
)

for ep in "${endpoints[@]}"; do
    IFS=':' read -r path desc <<< "$ep"
    status=$(curl -s -b /tmp/seller_cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL$path")
    log_test "Seller" "$path" "GET" "$status" "200" "$desc"
done

# Test Seller Product CRUD
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Seller Product CRUD ---" | tee -a $RESULTS_FILE

# Get token from Products page
curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Products/Create" > /tmp/seller_product_create.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_product_create.html | head -1)

# Create Product
status=$(curl -s -b /tmp/seller_cookies.txt \
    -d "Name=Test Product&Description=Test Description&CategoryId=1&Price=99.99&StockQuantity=100&SKU=TEST-001&Brand=TestBrand&__RequestVerificationToken=$TOKEN" \
    -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Products/Create")
log_test "Seller" "/Seller/Products/Create" "POST" "$status" "200 302" "Create product"

# Get product ID from list page
curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Products" > /tmp/seller_products_list.html
PRODUCT_ID=$(grep -oP '/Seller/Products/Edit/\K\d+' /tmp/seller_products_list.html | head -1)

if [ ! -z "$PRODUCT_ID" ]; then
    # Test Edit GET
    status=$(curl -s -b /tmp/seller_cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Seller/Products/Edit/$PRODUCT_ID")
    log_test "Seller" "/Seller/Products/Edit/{id}" "GET" "$status" "200" "Product edit form"

    # Test Details GET
    status=$(curl -s -b /tmp/seller_cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Seller/Products/Details/$PRODUCT_ID")
    log_test "Seller" "/Seller/Products/Details/{id}" "GET" "$status" "200" "Product details"

    # Get token for edit
    curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Products/Edit/$PRODUCT_ID" > /tmp/seller_product_edit.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_product_edit.html | head -1)

    # Update Product
    status=$(curl -s -b /tmp/seller_cookies.txt \
        -d "ProductId=$PRODUCT_ID&Name=Updated Product&Description=Updated Description&CategoryId=1&Price=89.99&StockQuantity=50&SKU=TEST-001&Brand=TestBrand&__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Products/Edit/$PRODUCT_ID")
    log_test "Seller" "/Seller/Products/Edit/{id}" "POST" "$status" "200 302" "Update product"

    # Delete Product
    curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Products" > /tmp/seller_products_for_delete.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_products_for_delete.html | head -1)

    status=$(curl -s -b /tmp/seller_cookies.txt \
        -d "__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Products/Delete/$PRODUCT_ID")
    log_test "Seller" "/Seller/Products/Delete/{id}" "POST" "$status" "200 302" "Delete product"
else
    echo -e "${YELLOW}⚠️  No products found for CRUD testing${NC}" | tee -a $RESULTS_FILE
fi

# Test Seller Discount CRUD
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Seller Discount CRUD ---" | tee -a $RESULTS_FILE

curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Discounts/Create" > /tmp/seller_discount_create.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_discount_create.html | head -1)

# Create Discount
status=$(curl -s -b /tmp/seller_cookies.txt \
    -d "Code=TESTCODE10&DiscountType=Percentage&DiscountValue=10&MinimumOrderValue=50&MaxUsageCount=100&ValidFrom=2025-01-01&ValidTo=2025-12-31&__RequestVerificationToken=$TOKEN" \
    -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Discounts/Create")
log_test "Seller" "/Seller/Discounts/Create" "POST" "$status" "200 302" "Create discount"

# Get discount ID
curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Discounts" > /tmp/seller_discounts_list.html
DISCOUNT_ID=$(grep -oP '/Seller/Discounts/Edit/\K\d+' /tmp/seller_discounts_list.html | head -1)

if [ ! -z "$DISCOUNT_ID" ]; then
    # Test Edit GET
    status=$(curl -s -b /tmp/seller_cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Seller/Discounts/Edit/$DISCOUNT_ID")
    log_test "Seller" "/Seller/Discounts/Edit/{id}" "GET" "$status" "200" "Discount edit form"

    # Get token for edit
    curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Discounts/Edit/$DISCOUNT_ID" > /tmp/seller_discount_edit.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_discount_edit.html | head -1)

    # Update Discount
    status=$(curl -s -b /tmp/seller_cookies.txt \
        -d "DiscountId=$DISCOUNT_ID&Code=UPDATED10&DiscountType=Percentage&DiscountValue=15&MinimumOrderValue=60&MaxUsageCount=150&ValidFrom=2025-01-01&ValidTo=2025-12-31&__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Discounts/Edit/$DISCOUNT_ID")
    log_test "Seller" "/Seller/Discounts/Edit/{id}" "POST" "$status" "200 302" "Update discount"

    # Delete Discount
    curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Discounts" > /tmp/seller_discounts_for_delete.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_discounts_for_delete.html | head -1)

    status=$(curl -s -b /tmp/seller_cookies.txt \
        -d "__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Discounts/Delete/$DISCOUNT_ID")
    log_test "Seller" "/Seller/Discounts/Delete/{id}" "POST" "$status" "200 302" "Delete discount"
fi

# Test Shop Update
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Seller Shop Update ---" | tee -a $RESULTS_FILE

curl -s -b /tmp/seller_cookies.txt "$BASE_URL/Seller/Shop/Edit" > /tmp/seller_shop_edit.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_shop_edit.html | head -1)

status=$(curl -s -b /tmp/seller_cookies.txt \
    -d "ShopName=Updated Shop Name&Description=Updated shop description&__RequestVerificationToken=$TOKEN" \
    -o /dev/null -w "%{http_code}" -L "$BASE_URL/Seller/Shop/Edit")
log_test "Seller" "/Seller/Shop/Edit" "POST" "$status" "200 302" "Update shop"

# ==========================================
# ADMIN TESTS
# ==========================================

echo "" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE
echo "TESTING ADMIN ENDPOINTS" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE

# Login as Admin
echo "" | tee -a $RESULTS_FILE
echo "Logging in as admin..." | tee -a $RESULTS_FILE
curl -s -c /tmp/admin_cookies.txt "$BASE_URL/Account/Login" > /tmp/admin_login_page.html
ADMIN_TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_login_page.html | head -1)

curl -s -b /tmp/admin_cookies.txt -c /tmp/admin_cookies.txt \
    -d "Email=admin@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$ADMIN_TOKEN" \
    -L "$BASE_URL/Account/Login" > /tmp/admin_login_result.html

if grep -q "logout" /tmp/admin_login_result.html; then
    echo -e "${GREEN}✅ Successfully logged in as admin${NC}" | tee -a $RESULTS_FILE
else
    echo -e "${RED}❌ Failed to login as admin${NC}" | tee -a $RESULTS_FILE
    exit 1
fi

# Test Admin GET endpoints
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Admin GET Endpoints ---" | tee -a $RESULTS_FILE

endpoints=(
    "/Admin/Dashboard:Admin dashboard"
    "/Admin/Users:Users list"
    "/Admin/Sellers/Pending:Pending sellers"
    "/Admin/Categories:Categories list"
    "/Admin/Categories/Create:Create category form"
    "/Admin/Reviews/Pending:Pending reviews"
)

for ep in "${endpoints[@]}"; do
    IFS=':' read -r path desc <<< "$ep"
    status=$(curl -s -b /tmp/admin_cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL$path")
    log_test "Admin" "$path" "GET" "$status" "200" "$desc"
done

# Test Admin Category CRUD
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Admin Category CRUD ---" | tee -a $RESULTS_FILE

curl -s -b /tmp/admin_cookies.txt "$BASE_URL/Admin/Categories/Create" > /tmp/admin_category_create.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_category_create.html | head -1)

# Create Category
status=$(curl -s -b /tmp/admin_cookies.txt \
    -d "Name=Test Category&Description=Test category description&__RequestVerificationToken=$TOKEN" \
    -o /dev/null -w "%{http_code}" -L "$BASE_URL/Admin/Categories/Create")
log_test "Admin" "/Admin/Categories/Create" "POST" "$status" "200 302" "Create category"

# Get category ID
curl -s -b /tmp/admin_cookies.txt "$BASE_URL/Admin/Categories" > /tmp/admin_categories_list.html
CATEGORY_ID=$(grep -oP '/Admin/Categories/Edit/\K\d+' /tmp/admin_categories_list.html | tail -1)

if [ ! -z "$CATEGORY_ID" ]; then
    # Test Edit GET
    status=$(curl -s -b /tmp/admin_cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Admin/Categories/Edit/$CATEGORY_ID")
    log_test "Admin" "/Admin/Categories/Edit/{id}" "GET" "$status" "200" "Category edit form"

    # Get token for edit
    curl -s -b /tmp/admin_cookies.txt "$BASE_URL/Admin/Categories/Edit/$CATEGORY_ID" > /tmp/admin_category_edit.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_category_edit.html | head -1)

    # Update Category
    status=$(curl -s -b /tmp/admin_cookies.txt \
        -d "CategoryId=$CATEGORY_ID&Name=Updated Category&Description=Updated description&__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Admin/Categories/Edit/$CATEGORY_ID")
    log_test "Admin" "/Admin/Categories/Edit/{id}" "POST" "$status" "200 302" "Update category"

    # Delete Category
    curl -s -b /tmp/admin_cookies.txt "$BASE_URL/Admin/Categories" > /tmp/admin_categories_for_delete.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_categories_for_delete.html | head -1)

    status=$(curl -s -b /tmp/admin_cookies.txt \
        -d "__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Admin/Categories/Delete/$CATEGORY_ID")
    log_test "Admin" "/Admin/Categories/Delete/{id}" "POST" "$status" "200 302" "Delete category"
fi

# Test Admin User Management
echo "" | tee -a $RESULTS_FILE
echo "--- Testing Admin User Management ---" | tee -a $RESULTS_FILE

curl -s -b /tmp/admin_cookies.txt "$BASE_URL/Admin/Users" > /tmp/admin_users_list.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_users_list.html | head -1)

# Try to get a user ID (non-admin)
USER_ID=$(grep -oP 'Suspend/[^"]+' /tmp/admin_users_list.html | head -1 | cut -d'/' -f2)

if [ ! -z "$USER_ID" ]; then
    # Suspend user
    status=$(curl -s -b /tmp/admin_cookies.txt \
        -d "__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Admin/Users/Suspend/$USER_ID")
    log_test "Admin" "/Admin/Users/Suspend/{id}" "POST" "$status" "200 302 404" "Suspend user"

    # Activate user
    status=$(curl -s -b /tmp/admin_cookies.txt \
        -d "__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Admin/Users/Activate/$USER_ID")
    log_test "Admin" "/Admin/Users/Activate/{id}" "POST" "$status" "200 302 404" "Activate user"
else
    echo -e "${YELLOW}⚠️  No users found for suspend/activate testing${NC}" | tee -a $RESULTS_FILE
fi

# ==========================================
# SUMMARY
# ==========================================

echo "" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE
echo "TEST SUMMARY" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE
TOTAL=$((PASS_COUNT + FAIL_COUNT))
PASS_RATE=$(awk "BEGIN {printf \"%.1f\", ($PASS_COUNT/$TOTAL)*100}")

echo "" | tee -a $RESULTS_FILE
echo "Total Tests: $TOTAL" | tee -a $RESULTS_FILE
echo -e "${GREEN}Passed: $PASS_COUNT${NC}" | tee -a $RESULTS_FILE
echo -e "${RED}Failed: $FAIL_COUNT${NC}" | tee -a $RESULTS_FILE
echo "Pass Rate: $PASS_RATE%" | tee -a $RESULTS_FILE
echo "" | tee -a $RESULTS_FILE
echo "Full results saved to: $RESULTS_FILE" | tee -a $RESULTS_FILE
echo "=========================================" | tee -a $RESULTS_FILE
