#!/bin/bash
# Check Reviews/Create error
curl -s -c /tmp/err_test.txt "http://localhost:8080/Account/Login" > /tmp/err_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/err_login.html | head -1)
curl -s -b /tmp/err_test.txt -c /tmp/err_test.txt \
    -d "Email=john@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "http://localhost:8080/Account/Login" > /dev/null

echo "=== /Reviews/Create Error ==="
curl -s -b /tmp/err_test.txt "http://localhost:8080/Reviews/Create" > /tmp/review_error.html
grep -A5 "Exception Details:" /tmp/review_error.html | sed 's/<[^>]*>//g' | head -10

echo ""
echo "=== Checking shop IDs ==="
curl -s "http://localhost:8080/Shops" | grep -oP '(?<=/Shops/)[0-9]+' | sort -u | head -5

echo ""
echo "=== Checking seller orders ==="
curl -s -c /tmp/seller_err.txt "http://localhost:8080/Account/Login" > /tmp/seller_err_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_err_login.html | head -1)
curl -s -b /tmp/seller_err.txt -c /tmp/seller_err.txt \
    -d "Email=seller@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "http://localhost:8080/Account/Login" > /dev/null
curl -s -b /tmp/seller_err.txt "http://localhost:8080/Seller/Orders" | grep -oP '(?<=/Seller/Orders/Details/)[0-9]+' | head -3
