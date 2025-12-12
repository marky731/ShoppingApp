#!/bin/bash
curl -s -c /tmp/seller_ord.txt "http://localhost:8080/Account/Login" > /tmp/seller_ord_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/seller_ord_login.html | head -1)
curl -s -b /tmp/seller_ord.txt -c /tmp/seller_ord.txt \
    -d "Email=seller@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "http://localhost:8080/Account/Login" > /dev/null
    
echo "Checking seller order details error..."
curl -s -b /tmp/seller_ord.txt "http://localhost:8080/Seller/Orders/Details/1" > /tmp/seller_ord_error.html
grep -A10 "Exception Details:" /tmp/seller_ord_error.html | sed 's/<[^>]*>//g' | head -15
