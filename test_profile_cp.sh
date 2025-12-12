#!/bin/bash
curl -s -c /tmp/test_cp.txt "http://localhost:8080/Account/Login" > /tmp/login_cp.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/login_cp.html | head -1)
curl -s -b /tmp/test_cp.txt -c /tmp/test_cp.txt \
    -d "Email=john@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "http://localhost:8080/Account/Login" > /dev/null
echo -n "/Profile/ChangePassword: "
curl -s -b /tmp/test_cp.txt -o /dev/null -w "%{http_code}" "http://localhost:8080/Profile/ChangePassword"
echo ""
