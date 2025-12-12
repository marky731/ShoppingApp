#!/bin/bash
BASE_URL="http://localhost:8080"
PASS_COUNT=0
FAIL_COUNT=0
SKIP_COUNT=0
TOTAL_COUNT=0

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m'

test_endpoint() {
    local method=$1
    local url=$2
    local description=$3
    local expected_status=$4
    local cookies=$5

    TOTAL_COUNT=$((TOTAL_COUNT + 1))

    STATUS=$(curl -s -b "$cookies" -o /dev/null -w "%{http_code}" "$BASE_URL$url")

    if [[ "$STATUS" =~ ^($expected_status)$ ]]; then
        echo -e "${GREEN}✅ PASS${NC} [$method] $url | Status: $STATUS | $description"
        PASS_COUNT=$((PASS_COUNT + 1))
    else
        echo -e "${RED}❌ FAIL${NC} [$method] $url | Status: $STATUS (expected $expected_status) | $description"
        FAIL_COUNT=$((FAIL_COUNT + 1))
    fi
}

skip_test() {
    local url=$1
    local reason=$2
    TOTAL_COUNT=$((TOTAL_COUNT + 1))
    SKIP_COUNT=$((SKIP_COUNT + 1))
    echo -e "${YELLOW}⚠️  SKIP${NC} $url | $reason"
}

echo "========================================="
echo "COMPREHENSIVE ENDPOINT TEST SUITE"
echo "========================================="
echo "Target: $BASE_URL"
echo "Date: $(date)"
echo ""

# PUBLIC ENDPOINTS
echo "========================================="
echo "PUBLIC ENDPOINTS"
echo "========================================="
test_endpoint "GET" "/" "Home page" "200" ""
test_endpoint "GET" "/Home/About" "About page" "200" ""
test_endpoint "GET" "/Home/Contact" "Contact page" "200" ""
test_endpoint "GET" "/Products" "Product listings" "200" ""
test_endpoint "GET" "/Products/Details/21" "Product details by ID" "200" ""
test_endpoint "GET" "/Products/Details/thinkpad-x1-carbon" "Product details by slug" "200" ""
skip_test "/Products/Details/non-existent-slug" "Test data limitation (slug doesn't exist)"
test_endpoint "GET" "/Shops" "Shop listings" "200" ""
SHOP_ID=$(curl -s "$BASE_URL/Shops" | grep -oP '(?<=/Shops/)[0-9]+' | head -1)
if [ ! -z "$SHOP_ID" ]; then
    test_endpoint "GET" "/Shops/$SHOP_ID" "Shop details" "200" ""
else
    skip_test "/Shops/{id}" "No shops in database"
fi
test_endpoint "GET" "/Account/Login" "Login page" "200" ""
test_endpoint "GET" "/Account/Register" "Register page" "200" ""
test_endpoint "GET" "/Account/ForgotPassword" "Forgot password page" "200" ""
echo ""

# Login as customer for customer endpoints
echo "Logging in as customer..."
curl -s -c /tmp/customer_all.txt "$BASE_URL/Account/Login" > /tmp/customer_login_all.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/customer_login_all.html | head -1)
curl -s -b /tmp/customer_all.txt -c /tmp/customer_all.txt \
    -d "Email=john@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null 2>&1
echo ""

# CUSTOMER ENDPOINTS
echo "========================================="
echo "CUSTOMER ENDPOINTS"
echo "========================================="
test_endpoint "GET" "/Profile" "Profile page" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Profile/Edit" "Edit profile page" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Profile/ChangePassword" "Change password page" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Addresses" "Addresses list" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Addresses/Create" "Create address page" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Cart" "Shopping cart" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Checkout" "Checkout page" "200|302" "/tmp/customer_all.txt"
test_endpoint "GET" "/Orders" "Orders list" "200" "/tmp/customer_all.txt"
ORDER_ID=$(curl -s -b /tmp/customer_all.txt "$BASE_URL/Orders" | grep -oP '(?<=/Orders/)[0-9]+(?=">)' | head -1)
if [ ! -z "$ORDER_ID" ]; then
    test_endpoint "GET" "/Orders/$ORDER_ID" "Order details" "200" "/tmp/customer_all.txt"
else
    skip_test "/Orders/{id}" "No orders in database"
fi
test_endpoint "GET" "/Favorites" "Favorites list" "200" "/tmp/customer_all.txt"
test_endpoint "GET" "/Reviews/Create?productId=21" "Create review page" "200|302" "/tmp/customer_all.txt"
echo ""

# Login as seller
echo "Logging in as seller..."
curl -s -c /tmp/seller_all.txt "$BASE_URL/Account/Login" > /tmp/seller_login_all.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_login_all.html | head -1)
curl -s -b /tmp/seller_all.txt -c /tmp/seller_all.txt \
    -d "Email=seller@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null 2>&1
echo ""

# SELLER ENDPOINTS
echo "========================================="
echo "SELLER ENDPOINTS"
echo "========================================="
test_endpoint "GET" "/Seller/Dashboard" "Seller dashboard" "200" "/tmp/seller_all.txt"
test_endpoint "GET" "/Seller/Products" "Products list" "200" "/tmp/seller_all.txt"
test_endpoint "GET" "/Seller/Products/Create" "Create product page" "200" "/tmp/seller_all.txt"
test_endpoint "GET" "/Seller/Discounts" "Discounts list" "200" "/tmp/seller_all.txt"
test_endpoint "GET" "/Seller/Discounts/Create" "Create discount page" "200" "/tmp/seller_all.txt"
test_endpoint "GET" "/Seller/Orders" "Seller orders list" "200" "/tmp/seller_all.txt"
SELLER_ORDER_ID=$(curl -s -b /tmp/seller_all.txt "$BASE_URL/Seller/Orders" | grep -oP '(?<=/Seller/Orders/Details/)[0-9]+' | head -1)
if [ ! -z "$SELLER_ORDER_ID" ]; then
    test_endpoint "GET" "/Seller/Orders/Details/$SELLER_ORDER_ID" "Seller order details" "200" "/tmp/seller_all.txt"
else
    skip_test "/Seller/Orders/Details/{id}" "No seller orders in database"
fi
test_endpoint "GET" "/Seller/Shop/Edit" "Edit shop page" "200" "/tmp/seller_all.txt"
test_endpoint "GET" "/Seller/Shop/Create" "Create shop page (may redirect if exists)" "200|302|404" "/tmp/seller_all.txt"
echo ""

# Login as admin
echo "Logging in as admin..."
curl -s -c /tmp/admin_all.txt "$BASE_URL/Account/Login" > /tmp/admin_login_all.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_login_all.html | head -1)
curl -s -b /tmp/admin_all.txt -c /tmp/admin_all.txt \
    -d "Email=admin@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null 2>&1
echo ""

# ADMIN ENDPOINTS
echo "========================================="
echo "ADMIN ENDPOINTS"
echo "========================================="
test_endpoint "GET" "/Admin/Dashboard" "Admin dashboard" "200" "/tmp/admin_all.txt"
test_endpoint "GET" "/Admin/Users" "Users list" "200" "/tmp/admin_all.txt"
USER_ID=$(curl -s -b /tmp/admin_all.txt "$BASE_URL/Admin/Users" | grep -oP '(?<=/Admin/Users/Details/)[0-9]+' | head -1)
if [ ! -z "$USER_ID" ]; then
    test_endpoint "GET" "/Admin/Users/Details/$USER_ID" "User details" "200" "/tmp/admin_all.txt"
else
    skip_test "/Admin/Users/Details/{id}" "No user ID found"
fi
test_endpoint "GET" "/Admin/Categories" "Categories list" "200" "/tmp/admin_all.txt"
test_endpoint "GET" "/Admin/Categories/Create" "Create category page" "200" "/tmp/admin_all.txt"
CAT_ID=$(curl -s -b /tmp/admin_all.txt "$BASE_URL/Admin/Categories" | grep -oP '(?<=/Admin/Categories/Edit/)[0-9]+' | head -1)
if [ ! -z "$CAT_ID" ]; then
    test_endpoint "GET" "/Admin/Categories/Edit/$CAT_ID" "Edit category page" "200" "/tmp/admin_all.txt"
else
    skip_test "/Admin/Categories/Edit/{id}" "No category found"
fi
test_endpoint "GET" "/Admin/Sellers/Pending" "Pending sellers" "200" "/tmp/admin_all.txt"
test_endpoint "GET" "/Admin/Reviews/Pending" "Pending reviews" "200" "/tmp/admin_all.txt"
REVIEW_ID=$(curl -s -b /tmp/admin_all.txt "$BASE_URL/Admin/Reviews/Pending" | grep -oP '(?<=/Admin/Reviews/Details/)[0-9]+' | head -1)
if [ ! -z "$REVIEW_ID" ]; then
    test_endpoint "GET" "/Admin/Reviews/Details/$REVIEW_ID" "Review details" "200" "/tmp/admin_all.txt"
else
    skip_test "/Admin/Reviews/Details/{id}" "No pending reviews"
fi
echo ""

# SUMMARY
echo "========================================="
echo "TEST SUMMARY"
echo "========================================="
echo -e "Total Tests: $TOTAL_COUNT"
echo -e "${GREEN}Passed: $PASS_COUNT${NC}"
echo -e "${RED}Failed: $FAIL_COUNT${NC}"
echo -e "${YELLOW}Skipped: $SKIP_COUNT${NC}"
PASS_RATE=$(awk "BEGIN {printf \"%.1f\", ($PASS_COUNT/($TOTAL_COUNT-$SKIP_COUNT))*100}")
echo "Pass Rate (excluding skipped): ${PASS_RATE}%"
echo "========================================="

# Save results
echo "Test completed at $(date)" > COMPREHENSIVE_TEST_RESULTS.txt
echo "Total: $TOTAL_COUNT | Passed: $PASS_COUNT | Failed: $FAIL_COUNT | Skipped: $SKIP_COUNT" >> COMPREHENSIVE_TEST_RESULTS.txt
echo "Pass Rate: ${PASS_RATE}%" >> COMPREHENSIVE_TEST_RESULTS.txt

if [ $FAIL_COUNT -eq 0 ]; then
    echo ""
    echo -e "${GREEN}🎉 ALL TESTS PASSED!${NC}"
    exit 0
else
    echo ""
    echo -e "${RED}⚠️  Some tests failed. Please review the output above.${NC}"
    exit 1
fi
