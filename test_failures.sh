#!/bin/bash

# Test script to get detailed error information for failures

BASE_URL="http://localhost:8080"

echo "========================================="
echo "TESTING FAILURES IN DETAIL"
echo "========================================="

# Test 1: Product Details by Slug
echo -e "\n1. Testing /Products/Details/premium-smartphone-pro"
curl -s "$BASE_URL/Products/Details/premium-smartphone-pro" > /tmp/error1.html
STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/Products/Details/premium-smartphone-pro")
echo "Status: $STATUS"
if [ "$STATUS" != "200" ]; then
    echo "Error details:"
    grep -oP '(?<=<title>)[^<]+' /tmp/error1.html || echo "No error title found"
    grep -oP '(?<=<h2>)[^<]+' /tmp/error1.html | head -1 || echo "No h2 found"
fi

# First login as customer to test authenticated endpoints
echo -e "\n2. Logging in as customer..."
# Get login page to get cookie and token
curl -s -c /tmp/cookies.txt "$BASE_URL/Account/Login" > /tmp/login_page.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/login_page.html | head -1)
echo "Got anti-forgery token: ${TOKEN:0:20}..."

# Login POST
curl -s -b /tmp/cookies.txt -c /tmp/cookies.txt \
    -d "Email=john@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /tmp/login_result.html

# Check if logged in
if grep -q "logout" /tmp/login_result.html; then
    echo "✅ Successfully logged in as customer"
else
    echo "❌ Failed to login"
    exit 1
fi

# Test 2: Addresses Edit page
echo -e "\n3. Testing /Addresses/Edit/1"
curl -s -b /tmp/cookies.txt "$BASE_URL/Addresses/Edit/1" > /tmp/error2.html
STATUS=$(curl -s -b /tmp/cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Addresses/Edit/1")
echo "Status: $STATUS"
if [ "$STATUS" != "200" ]; then
    echo "Error details:"
    grep -oP '(?<=<title>)[^<]+' /tmp/error2.html | head -1
    grep -oP '(?<=<b>)[^<]+</b>' /tmp/error2.html | head -5 | sed 's/<\/b>//' || \
    grep -oP '(?<=<h2 class="text-danger">)[^<]+' /tmp/error2.html
fi

# Test 3: Cart Remove
echo -e "\n4. Testing /Cart/Remove/1"
# Get token from Cart page
curl -s -b /tmp/cookies.txt "$BASE_URL/Cart" > /tmp/cart_page.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/cart_page.html | head -1)

curl -s -b /tmp/cookies.txt \
    -d "__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Cart/Remove/1" > /tmp/error3.html
STATUS=$(curl -s -b /tmp/cookies.txt -d "__RequestVerificationToken=$TOKEN" -o /dev/null -w "%{http_code}" "$BASE_URL/Cart/Remove/1")
echo "Status: $STATUS"
if [ "$STATUS" != "200" ] && [ "$STATUS" != "302" ]; then
    echo "Error details:"
    grep -oP '(?<=<title>)[^<]+' /tmp/error3.html | head -1
    grep -oP '(?<=<h2 class="text-danger">)[^<]+' /tmp/error3.html || \
    grep -oP '(?<=<div class="line"[^>]*>)[^<]+' /tmp/error3.html | head -3
fi

# Test 4: Favorites Toggle
echo -e "\n5. Testing /Favorites/Toggle/1"
# Get token from Favorites page
curl -s -b /tmp/cookies.txt "$BASE_URL/Favorites" > /tmp/favorites_page.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/favorites_page.html | head -1)

curl -s -b /tmp/cookies.txt \
    -d "__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Favorites/Toggle/1" > /tmp/error4.html
STATUS=$(curl -s -b /tmp/cookies.txt -d "__RequestVerificationToken=$TOKEN" -o /dev/null -w "%{http_code}" "$BASE_URL/Favorites/Toggle/1")
echo "Status: $STATUS"
if [ "$STATUS" != "200" ] && [ "$STATUS" != "302" ]; then
    echo "Error details:"
    grep -oP '(?<=<title>)[^<]+' /tmp/error4.html | head -1
    grep -oP '(?<=<h2 class="text-danger">)[^<]+' /tmp/error4.html || \
    grep -oP '(?<=<div class="line"[^>]*>)[^<]+' /tmp/error4.html | head -3
fi

# Test 5: Profile Edit
echo -e "\n6. Testing /Profile/Edit"
curl -s -b /tmp/cookies.txt "$BASE_URL/Profile/Edit" > /tmp/error5.html
STATUS=$(curl -s -b /tmp/cookies.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Profile/Edit")
echo "Status: $STATUS"
if [ "$STATUS" != "200" ]; then
    echo "Error details:"
    grep -oP '(?<=<title>)[^<]+' /tmp/error5.html | head -1
    echo "Exception details:"
    grep -oP '(?<=<b> Description: </b>)[^<]+' /tmp/error5.html || \
    grep -oP '(?<=<h2 class="text-danger">)[^<]+' /tmp/error5.html || \
    grep -oP '(?<=<div class="line"[^>]*>)[^<]+' /tmp/error5.html | head -5
fi

echo -e "\n========================================="
echo "Detailed error files saved in /tmp/"
echo "========================================="
