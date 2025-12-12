#!/bin/bash
BASE_URL="http://localhost:8080"
PASS_COUNT=0
FAIL_COUNT=0
TOTAL_COUNT=0

# Colors
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

    if [ "$method" = "GET" ]; then
        STATUS=$(curl -s -b "$cookies" -o /dev/null -w "%{http_code}" "$BASE_URL$url")
    else
        # For POST, we'd need tokens - skip for now, just test GET
        STATUS=$(curl -s -b "$cookies" -o /dev/null -w "%{http_code}" "$BASE_URL$url")
    fi

    if [[ "$STATUS" =~ ^($expected_status)$ ]]; then
        echo -e "${GREEN}✅ PASS${NC} [$method] $url | Status: $STATUS | $description"
        PASS_COUNT=$((PASS_COUNT + 1))
    else
        echo -e "${RED}❌ FAIL${NC} [$method] $url | Status: $STATUS (expected $expected_status) | $description"
        FAIL_COUNT=$((FAIL_COUNT + 1))
    fi
}

echo "========================================="
echo "TESTING MISSING ENDPOINTS"
echo "========================================="
echo ""

# Login as customer
curl -s -c /tmp/customer_missing.txt "$BASE_URL/Account/Login" > /tmp/login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/login.html | head -1)
curl -s -b /tmp/customer_missing.txt -c /tmp/customer_missing.txt \
    -d "Email=john@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null 2>&1

echo "--- PUBLIC/CUSTOMER ENDPOINTS ---"
test_endpoint "GET" "/" "Home page" "200" "/tmp/customer_missing.txt"
test_endpoint "GET" "/Home/About" "About page" "200" "/tmp/customer_missing.txt"
test_endpoint "GET" "/Home/Contact" "Contact page" "200" "/tmp/customer_missing.txt"
test_endpoint "GET" "/Account/ForgotPassword" "Forgot password page" "200" "/tmp/customer_missing.txt"
test_endpoint "GET" "/Cart/Clear" "Clear cart (may redirect)" "200|302" "/tmp/customer_missing.txt"
test_endpoint "GET" "/Checkout" "Checkout page" "200|302" "/tmp/customer_missing.txt"
test_endpoint "GET" "/Profile/ChangePassword" "Change password page" "200" "/tmp/customer_missing.txt"
echo ""

# Login as seller
curl -s -c /tmp/seller_missing.txt "$BASE_URL/Account/Login" > /tmp/seller_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_login.html | head -1)
curl -s -b /tmp/seller_missing.txt -c /tmp/seller_missing.txt \
    -d "Email=seller@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null 2>&1

echo "--- SELLER ENDPOINTS ---"
test_endpoint "GET" "/Seller/Shop/Create" "Create shop page" "200|302|404" "/tmp/seller_missing.txt"

# Get an order ID
ORDER_ID=$(curl -s -b /tmp/seller_missing.txt "$BASE_URL/Seller/Orders" | grep -oP '(?<=/Seller/Orders/Details/)[0-9]+' | head -1)
if [ ! -z "$ORDER_ID" ]; then
    test_endpoint "GET" "/Seller/Orders/Details/$ORDER_ID" "Order details" "200" "/tmp/seller_missing.txt"
else
    echo -e "${YELLOW}⚠️  SKIP${NC} [GET] /Seller/Orders/Details/{id} | No orders available"
    TOTAL_COUNT=$((TOTAL_COUNT + 1))
fi
echo ""

# Login as admin
curl -s -c /tmp/admin_missing.txt "$BASE_URL/Account/Login" > /tmp/admin_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_login.html | head -1)
curl -s -b /tmp/admin_missing.txt -c /tmp/admin_missing.txt \
    -d "Email=admin@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null 2>&1

echo "--- ADMIN ENDPOINTS ---"
test_endpoint "GET" "/Admin/Reviews/Pending" "Pending reviews (already tested)" "200" "/tmp/admin_missing.txt"

# Get a review ID if available
REVIEW_ID=$(curl -s -b /tmp/admin_missing.txt "$BASE_URL/Admin/Reviews/Pending" | grep -oP '(?<=/Admin/Reviews/Details/)[0-9]+' | head -1)
if [ ! -z "$REVIEW_ID" ]; then
    test_endpoint "GET" "/Admin/Reviews/Details/$REVIEW_ID" "Review details" "200" "/tmp/admin_missing.txt"
else
    echo -e "${YELLOW}⚠️  SKIP${NC} [GET] /Admin/Reviews/Details/{id} | No reviews available"
    TOTAL_COUNT=$((TOTAL_COUNT + 1))
fi

# Get a seller ID
SELLER_ID=$(curl -s -b /tmp/admin_missing.txt "$BASE_URL/Admin/Sellers/Pending" | grep -oP '(?<=/Admin/Sellers/Approve/)[0-9]+' | head -1)
if [ ! -z "$SELLER_ID" ]; then
    echo -e "${YELLOW}⚠️  INFO${NC} Found pending seller ID: $SELLER_ID (not testing Approve/Reject to avoid data changes)"
fi

# Get a user ID
USER_ID=$(curl -s -b /tmp/admin_missing.txt "$BASE_URL/Admin/Users" | grep -oP '(?<=/Admin/Users/Details/)[0-9]+' | head -1)
if [ ! -z "$USER_ID" ]; then
    test_endpoint "GET" "/Admin/Users/Details/$USER_ID" "User details" "200" "/tmp/admin_missing.txt"
else
    echo -e "${YELLOW}⚠️  SKIP${NC} [GET] /Admin/Users/Details/{id} | No users available"
    TOTAL_COUNT=$((TOTAL_COUNT + 1))
fi

echo ""
echo "========================================="
echo "SUMMARY"
echo "========================================="
echo -e "Total Tests: $TOTAL_COUNT"
echo -e "${GREEN}Passed: $PASS_COUNT${NC}"
echo -e "${RED}Failed: $FAIL_COUNT${NC}"
PASS_RATE=$(awk "BEGIN {printf \"%.1f\", ($PASS_COUNT/$TOTAL_COUNT)*100}")
echo "Pass Rate: ${PASS_RATE}%"
echo "========================================="
