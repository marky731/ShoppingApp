#!/bin/bash
BASE_URL="http://localhost:8080"

# Login as admin
curl -s -c /tmp/admin_cat_test.txt "$BASE_URL/Account/Login" > /tmp/admin_cat_login.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/admin_cat_login.html | head -1)

curl -s -b /tmp/admin_cat_test.txt -c /tmp/admin_cat_test.txt \
    -d "Email=admin@example.com&Password=Test1234&RememberMe=false&__RequestVerificationToken=$TOKEN" \
    -L "$BASE_URL/Account/Login" > /dev/null

echo "Testing Admin Categories CRUD Operations"
echo "========================================="

# Test 1: GET Create
echo -n "Test 1: GET /Admin/Categories/Create ... "
STATUS=$(curl -s -b /tmp/admin_cat_test.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Admin/Categories/Create")
if [ "$STATUS" = "200" ]; then
    echo "✅ PASS ($STATUS)"
else
    echo "❌ FAIL ($STATUS)"
fi

# Test 2: POST Create
echo -n "Test 2: POST /Admin/Categories/Create ... "
curl -s -b /tmp/admin_cat_test.txt "$BASE_URL/Admin/Categories/Create" > /tmp/create_form.html
TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/create_form.html | head -1)

STATUS=$(curl -s -b /tmp/admin_cat_test.txt -c /tmp/admin_cat_test.txt \
    -d "CategoryName=TestCategory$(date +%s)&ParentCategoryId=&__RequestVerificationToken=$TOKEN" \
    -o /tmp/create_result.html -w "%{http_code}" -L "$BASE_URL/Admin/Categories/Create")

if [ "$STATUS" = "200" ] || [ "$STATUS" = "302" ]; then
    echo "✅ PASS ($STATUS)"
    # Extract the newly created category ID
    CAT_ID=$(grep -oP '(?<=/Admin/Categories/Edit/)[0-9]+' /tmp/create_result.html | head -1)
    if [ -z "$CAT_ID" ]; then
        # Try getting it from the categories list
        curl -s -b /tmp/admin_cat_test.txt "$BASE_URL/Admin/Categories" > /tmp/cat_list.html
        CAT_ID=$(grep -oP '(?<=/Admin/Categories/Edit/)[0-9]+' /tmp/cat_list.html | head -1)
    fi
else
    echo "❌ FAIL ($STATUS)"
fi

# Test 3: GET Edit
if [ ! -z "$CAT_ID" ]; then
    echo -n "Test 3: GET /Admin/Categories/Edit/$CAT_ID ... "
    STATUS=$(curl -s -b /tmp/admin_cat_test.txt -o /dev/null -w "%{http_code}" "$BASE_URL/Admin/Categories/Edit/$CAT_ID")
    if [ "$STATUS" = "200" ]; then
        echo "✅ PASS ($STATUS)"
    else
        echo "❌ FAIL ($STATUS)"
    fi
else
    echo "Test 3: GET /Admin/Categories/Edit/{id} ... ⚠️  SKIP (no category ID)"
fi

# Test 4: POST Edit
if [ ! -z "$CAT_ID" ]; then
    echo -n "Test 4: POST /Admin/Categories/Edit/$CAT_ID ... "
    curl -s -b /tmp/admin_cat_test.txt "$BASE_URL/Admin/Categories/Edit/$CAT_ID" > /tmp/edit_form.html
    TOKEN=$(grep -oP '(?<=name="__RequestVerificationToken" type="hidden" value=")[^"]+' /tmp/edit_form.html | head -1)

    STATUS=$(curl -s -b /tmp/admin_cat_test.txt -c /tmp/admin_cat_test.txt \
        -d "CategoryId=$CAT_ID&CategoryName=UpdatedCategory$(date +%s)&ParentCategoryId=&__RequestVerificationToken=$TOKEN" \
        -o /dev/null -w "%{http_code}" -L "$BASE_URL/Admin/Categories/Edit/$CAT_ID")

    if [ "$STATUS" = "200" ] || [ "$STATUS" = "302" ]; then
        echo "✅ PASS ($STATUS)"
    else
        echo "❌ FAIL ($STATUS)"
    fi
else
    echo "Test 4: POST /Admin/Categories/Edit/{id} ... ⚠️  SKIP (no category ID)"
fi

echo ""
echo "Admin Categories CRUD testing complete!"
