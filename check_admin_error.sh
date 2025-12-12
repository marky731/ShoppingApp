#!/bin/bash
BASE_URL="http://localhost:8080"

# Login as admin
curl -s -c /tmp/admin_test.txt "$BASE_URL/Account/Login" > /tmp/admin_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_login.html | head -1)

curl -s -b /tmp/admin_test.txt -c /tmp/admin_test.txt \
    -d "Email=admin@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null

# Get the error
curl -s -b /tmp/admin_test.txt "$BASE_URL/Admin/Categories/Create" > /tmp/admin_cat_error.html

echo "Error details:"
grep -A5 "Compiler Error Message" /tmp/admin_cat_error.html | sed 's/<[^>]*>//g' | head -10
