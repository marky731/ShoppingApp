#!/usr/bin/env python3
"""
Comprehensive CRUD Testing Script for ShoppingApp
Tests all endpoints with proper authentication and CRUD operations
"""

import requests
from bs4 import BeautifulSoup
import json
from typing import Dict, List, Tuple
import re

BASE_URL = "http://localhost:8080"
TEST_RESULTS = []

# Test credentials
CREDENTIALS = {
    'admin': {'email': 'admin@example.com', 'password': 'Test1234'},
    'seller': {'email': 'seller@example.com', 'password': 'Test1234'},
    'customer': {'email': 'john@example.com', 'password': 'Test1234'}
}

class TestSession:
    """Manages authenticated sessions for testing"""

    def __init__(self, role: str):
        self.role = role
        self.session = requests.Session()
        self.logged_in = False

    def login(self) -> bool:
        """Login and maintain session with cookies"""
        try:
            # Get login page to get anti-forgery token
            response = self.session.get(f"{BASE_URL}/Account/Login")
            if response.status_code != 200:
                return False

            soup = BeautifulSoup(response.text, 'html.parser')
            token_input = soup.find('input', {'name': '__RequestVerificationToken'})

            if not token_input:
                print(f"[{self.role}] No anti-forgery token found")
                return False

            token = token_input.get('value')

            # Perform login POST
            login_data = {
                'Email': CREDENTIALS[self.role]['email'],
                'Password': CREDENTIALS[self.role]['password'],
                'RememberMe': 'false',
                '__RequestVerificationToken': token
            }

            response = self.session.post(
                f"{BASE_URL}/Account/Login",
                data=login_data,
                allow_redirects=True
            )

            # Check if login successful (should redirect to home or dashboard)
            self.logged_in = response.status_code == 200 and 'logout' in response.text.lower()
            return self.logged_in

        except Exception as e:
            print(f"[{self.role}] Login error: {e}")
            return False

    def get_antiforgery_token(self, html: str) -> str:
        """Extract anti-forgery token from HTML"""
        soup = BeautifulSoup(html, 'html.parser')
        token_input = soup.find('input', {'name': '__RequestVerificationToken'})
        return token_input.get('value') if token_input else None

def log_result(category: str, endpoint: str, method: str, status: int, result: str, notes: str = ""):
    """Log test result"""
    TEST_RESULTS.append({
        'category': category,
        'endpoint': endpoint,
        'method': method,
        'status': status,
        'result': result,
        'notes': notes
    })
    status_emoji = "✅" if result == "PASS" else "❌"
    print(f"{status_emoji} [{method:6}] {endpoint:50} | {status} | {notes[:50]}")

def test_public_endpoints():
    """Test all public GET endpoints"""
    print("\n" + "="*80)
    print("TESTING PUBLIC ENDPOINTS")
    print("="*80)

    session = requests.Session()

    endpoints = [
        ('/', 'Home page'),
        ('/Products', 'Products listing'),
        ('/Products/Details/1', 'Product details by ID'),
        ('/Products/Details/premium-smartphone-pro', 'Product details by slug'),
        ('/Shops', 'Shops listing'),
        ('/Shops/Details/1', 'Shop details'),
        ('/Account/Login', 'Login page'),
        ('/Account/Register', 'Register page'),
        ('/Account/ForgotPassword', 'Forgot password page'),
    ]

    for endpoint, description in endpoints:
        try:
            response = session.get(f"{BASE_URL}{endpoint}")
            result = "PASS" if response.status_code == 200 else "FAIL"
            log_result("Public", endpoint, "GET", response.status_code, result, description)
        except Exception as e:
            log_result("Public", endpoint, "GET", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_customer_endpoints():
    """Test all customer endpoints with CRUD operations"""
    print("\n" + "="*80)
    print("TESTING CUSTOMER ENDPOINTS")
    print("="*80)

    session = TestSession('customer')
    if not session.login():
        print("❌ Failed to login as customer")
        return

    print("✅ Logged in as customer")

    # Test GET endpoints
    get_endpoints = [
        ('/Cart', 'Shopping cart'),
        ('/Favorites', 'Favorites list'),
        ('/Orders', 'Order history'),
        ('/Profile', 'User profile'),
        ('/Addresses', 'Address list'),
    ]

    for endpoint, description in get_endpoints:
        try:
            response = session.session.get(f"{BASE_URL}{endpoint}")
            result = "PASS" if response.status_code == 200 else "FAIL"
            log_result("Customer", endpoint, "GET", response.status_code, result, description)
        except Exception as e:
            log_result("Customer", endpoint, "GET", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Test Cart POST operations
    test_cart_operations(session)

    # Test Address CRUD
    test_address_crud(session)

    # Test Favorites Toggle
    test_favorites_operations(session)

    # Test Profile Update
    test_profile_operations(session)

def test_cart_operations(session: TestSession):
    """Test shopping cart add/update/remove operations"""
    print("\n--- Testing Cart Operations ---")

    # Add to cart
    try:
        response = session.session.get(f"{BASE_URL}/Cart")
        token = session.get_antiforgery_token(response.text)

        if token:
            # Try to add product to cart
            add_data = {
                'productId': 1,
                'quantity': 2,
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Cart/Add", data=add_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Customer", "/Cart/Add", "POST", response.status_code, result, "Add item to cart")
        else:
            log_result("Customer", "/Cart/Add", "POST", 0, "FAIL", "No anti-forgery token")
    except Exception as e:
        log_result("Customer", "/Cart/Add", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Update cart
    try:
        update_data = {
            'productId': 1,
            'quantity': 3,
            '__RequestVerificationToken': token
        }
        response = session.session.post(f"{BASE_URL}/Cart/Update", data=update_data)
        result = "PASS" if response.status_code in [200, 302] else "FAIL"
        log_result("Customer", "/Cart/Update", "POST", response.status_code, result, "Update cart quantity")
    except Exception as e:
        log_result("Customer", "/Cart/Update", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Remove from cart
    try:
        response = session.session.post(f"{BASE_URL}/Cart/Remove/1", data={'__RequestVerificationToken': token})
        result = "PASS" if response.status_code in [200, 302] else "FAIL"
        log_result("Customer", "/Cart/Remove/{id}", "POST", response.status_code, result, "Remove item from cart")
    except Exception as e:
        log_result("Customer", "/Cart/Remove/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_address_crud(session: TestSession):
    """Test address create/read/update/delete operations"""
    print("\n--- Testing Address CRUD ---")

    # Get Create page
    try:
        response = session.session.get(f"{BASE_URL}/Addresses/Create")
        result = "PASS" if response.status_code == 200 else "FAIL"
        log_result("Customer", "/Addresses/Create", "GET", response.status_code, result, "Address create form")
        token = session.get_antiforgery_token(response.text)
    except Exception as e:
        log_result("Customer", "/Addresses/Create", "GET", 0, "FAIL", f"Error: {str(e)[:50]}")
        return

    # Create address
    try:
        if token:
            create_data = {
                'AddressLabel': 'Test Address',
                'StreetAddress': '123 Test St',
                'City': 'Test City',
                'StateProvince': 'TC',
                'PostalCode': '12345',
                'Country': 'Test Country',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Addresses/Create", data=create_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Customer", "/Addresses/Create", "POST", response.status_code, result, "Create new address")
    except Exception as e:
        log_result("Customer", "/Addresses/Create", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Get Edit page
    try:
        response = session.session.get(f"{BASE_URL}/Addresses/Edit/1")
        result = "PASS" if response.status_code == 200 else "FAIL"
        log_result("Customer", "/Addresses/Edit/{id}", "GET", response.status_code, result, "Address edit form")
        token = session.get_antiforgery_token(response.text)
    except Exception as e:
        log_result("Customer", "/Addresses/Edit/{id}", "GET", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Update address
    try:
        if token:
            update_data = {
                'AddressId': 1,
                'AddressLabel': 'Updated Address',
                'StreetAddress': '456 Updated St',
                'City': 'Updated City',
                'StateProvince': 'UC',
                'PostalCode': '54321',
                'Country': 'Updated Country',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Addresses/Edit/1", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Customer", "/Addresses/Edit/{id}", "POST", response.status_code, result, "Update address")
    except Exception as e:
        log_result("Customer", "/Addresses/Edit/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Delete address
    try:
        response = session.session.get(f"{BASE_URL}/Addresses")
        token = session.get_antiforgery_token(response.text)
        if token:
            response = session.session.post(f"{BASE_URL}/Addresses/Delete/1", data={'__RequestVerificationToken': token})
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Customer", "/Addresses/Delete/{id}", "POST", response.status_code, result, "Delete address")
    except Exception as e:
        log_result("Customer", "/Addresses/Delete/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_favorites_operations(session: TestSession):
    """Test favorites toggle operations"""
    print("\n--- Testing Favorites Operations ---")

    try:
        response = session.session.get(f"{BASE_URL}/Favorites")
        token = session.get_antiforgery_token(response.text)

        if token:
            # Toggle favorite (add)
            response = session.session.post(
                f"{BASE_URL}/Favorites/Toggle/1",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Customer", "/Favorites/Toggle/{id}", "POST", response.status_code, result, "Toggle favorite")
    except Exception as e:
        log_result("Customer", "/Favorites/Toggle/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_profile_operations(session: TestSession):
    """Test profile update operations"""
    print("\n--- Testing Profile Operations ---")

    # Get Edit page
    try:
        response = session.session.get(f"{BASE_URL}/Profile/Edit")
        result = "PASS" if response.status_code == 200 else "FAIL"
        log_result("Customer", "/Profile/Edit", "GET", response.status_code, result, "Profile edit form")
        token = session.get_antiforgery_token(response.text)
    except Exception as e:
        log_result("Customer", "/Profile/Edit", "GET", 0, "FAIL", f"Error: {str(e)[:50]}")
        return

    # Update profile
    try:
        if token:
            update_data = {
                'FirstName': 'John',
                'LastName': 'Updated',
                'PhoneNumber': '1234567890',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Profile/Edit", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Customer", "/Profile/Edit", "POST", response.status_code, result, "Update profile")
    except Exception as e:
        log_result("Customer", "/Profile/Edit", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_seller_endpoints():
    """Test all seller endpoints with CRUD operations"""
    print("\n" + "="*80)
    print("TESTING SELLER ENDPOINTS")
    print("="*80)

    session = TestSession('seller')
    if not session.login():
        print("❌ Failed to login as seller")
        return

    print("✅ Logged in as seller")

    # Test GET endpoints
    get_endpoints = [
        ('/Seller/Dashboard', 'Seller dashboard'),
        ('/Seller/Products', 'Product list'),
        ('/Seller/Products/Create', 'Create product form'),
        ('/Seller/Products/Edit/1', 'Edit product form'),
        ('/Seller/Products/Details/1', 'Product details'),
        ('/Seller/Orders', 'Order list'),
        ('/Seller/Orders/Details/1', 'Order details'),
        ('/Seller/Discounts', 'Discount list'),
        ('/Seller/Discounts/Create', 'Create discount form'),
        ('/Seller/Discounts/Edit/1', 'Edit discount form'),
        ('/Seller/Shop/Edit', 'Shop settings'),
    ]

    for endpoint, description in get_endpoints:
        try:
            response = session.session.get(f"{BASE_URL}{endpoint}")
            result = "PASS" if response.status_code == 200 else "FAIL"
            log_result("Seller", endpoint, "GET", response.status_code, result, description)
        except Exception as e:
            log_result("Seller", endpoint, "GET", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Test Product CRUD
    test_seller_product_crud(session)

    # Test Discount CRUD
    test_seller_discount_crud(session)

    # Test Shop Update
    test_seller_shop_update(session)

    # Test Order Status Update
    test_seller_order_update(session)

def test_seller_product_crud(session: TestSession):
    """Test seller product CRUD operations"""
    print("\n--- Testing Seller Product CRUD ---")

    # Create product - GET form
    try:
        response = session.session.get(f"{BASE_URL}/Seller/Products/Create")
        token = session.get_antiforgery_token(response.text)

        # Create product - POST
        if token:
            create_data = {
                'Name': 'Test Product',
                'Description': 'Test Description',
                'CategoryId': 1,
                'Price': 99.99,
                'StockQuantity': 100,
                'SKU': 'TEST-SKU-001',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Seller/Products/Create", data=create_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Products/Create", "POST", response.status_code, result, "Create product")
    except Exception as e:
        log_result("Seller", "/Seller/Products/Create", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Edit product - GET form already tested above

    # Edit product - POST
    try:
        response = session.session.get(f"{BASE_URL}/Seller/Products/Edit/1")
        token = session.get_antiforgery_token(response.text)

        if token:
            update_data = {
                'ProductId': 1,
                'Name': 'Updated Product',
                'Description': 'Updated Description',
                'CategoryId': 1,
                'Price': 89.99,
                'StockQuantity': 50,
                'SKU': 'TEST-SKU-001',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Seller/Products/Edit/1", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Products/Edit/{id}", "POST", response.status_code, result, "Update product")
    except Exception as e:
        log_result("Seller", "/Seller/Products/Edit/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Delete product
    try:
        response = session.session.get(f"{BASE_URL}/Seller/Products")
        token = session.get_antiforgery_token(response.text)

        if token:
            response = session.session.post(f"{BASE_URL}/Seller/Products/Delete/1", data={'__RequestVerificationToken': token})
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Products/Delete/{id}", "POST", response.status_code, result, "Delete product")
    except Exception as e:
        log_result("Seller", "/Seller/Products/Delete/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_seller_discount_crud(session: TestSession):
    """Test seller discount CRUD operations"""
    print("\n--- Testing Seller Discount CRUD ---")

    # Create discount
    try:
        response = session.session.get(f"{BASE_URL}/Seller/Discounts/Create")
        token = session.get_antiforgery_token(response.text)

        if token:
            create_data = {
                'Code': 'TESTCODE10',
                'DiscountType': 'Percentage',
                'DiscountValue': 10,
                'MinimumOrderValue': 50,
                'MaxUsageCount': 100,
                'ValidFrom': '2025-01-01',
                'ValidTo': '2025-12-31',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Seller/Discounts/Create", data=create_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Discounts/Create", "POST", response.status_code, result, "Create discount")
    except Exception as e:
        log_result("Seller", "/Seller/Discounts/Create", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Edit discount
    try:
        response = session.session.get(f"{BASE_URL}/Seller/Discounts/Edit/1")
        token = session.get_antiforgery_token(response.text)

        if token:
            update_data = {
                'DiscountId': 1,
                'Code': 'UPDATED10',
                'DiscountType': 'Percentage',
                'DiscountValue': 15,
                'MinimumOrderValue': 60,
                'MaxUsageCount': 150,
                'ValidFrom': '2025-01-01',
                'ValidTo': '2025-12-31',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Seller/Discounts/Edit/1", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Discounts/Edit/{id}", "POST", response.status_code, result, "Update discount")
    except Exception as e:
        log_result("Seller", "/Seller/Discounts/Edit/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Delete discount
    try:
        response = session.session.get(f"{BASE_URL}/Seller/Discounts")
        token = session.get_antiforgery_token(response.text)

        if token:
            response = session.session.post(f"{BASE_URL}/Seller/Discounts/Delete/1", data={'__RequestVerificationToken': token})
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Discounts/Delete/{id}", "POST", response.status_code, result, "Delete discount")
    except Exception as e:
        log_result("Seller", "/Seller/Discounts/Delete/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_seller_shop_update(session: TestSession):
    """Test seller shop update operations"""
    print("\n--- Testing Seller Shop Update ---")

    try:
        response = session.session.get(f"{BASE_URL}/Seller/Shop/Edit")
        token = session.get_antiforgery_token(response.text)

        if token:
            update_data = {
                'ShopName': 'Updated Shop Name',
                'Description': 'Updated shop description',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Seller/Shop/Edit", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Shop/Edit", "POST", response.status_code, result, "Update shop")
    except Exception as e:
        log_result("Seller", "/Seller/Shop/Edit", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_seller_order_update(session: TestSession):
    """Test seller order status update"""
    print("\n--- Testing Seller Order Update ---")

    try:
        response = session.session.get(f"{BASE_URL}/Seller/Orders/Details/1")
        token = session.get_antiforgery_token(response.text)

        if token:
            update_data = {
                'shopOrderId': 1,
                'status': 'Processing',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Seller/Orders/UpdateStatus", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Seller", "/Seller/Orders/UpdateStatus", "POST", response.status_code, result, "Update order status")
    except Exception as e:
        log_result("Seller", "/Seller/Orders/UpdateStatus", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_admin_endpoints():
    """Test all admin endpoints with CRUD operations"""
    print("\n" + "="*80)
    print("TESTING ADMIN ENDPOINTS")
    print("="*80)

    session = TestSession('admin')
    if not session.login():
        print("❌ Failed to login as admin")
        return

    print("✅ Logged in as admin")

    # Test GET endpoints
    get_endpoints = [
        ('/Admin/Dashboard', 'Admin dashboard'),
        ('/Admin/Users', 'User list'),
        ('/Admin/Sellers/Pending', 'Pending sellers'),
        ('/Admin/Categories', 'Category list'),
        ('/Admin/Categories/Create', 'Create category form'),
        ('/Admin/Categories/Edit/1', 'Edit category form'),
        ('/Admin/Reviews/Pending', 'Pending reviews'),
    ]

    for endpoint, description in get_endpoints:
        try:
            response = session.session.get(f"{BASE_URL}{endpoint}")
            result = "PASS" if response.status_code == 200 else "FAIL"
            log_result("Admin", endpoint, "GET", response.status_code, result, description)
        except Exception as e:
            log_result("Admin", endpoint, "GET", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Test Category CRUD
    test_admin_category_crud(session)

    # Test User Management
    test_admin_user_management(session)

    # Test Seller Approval
    test_admin_seller_approval(session)

    # Test Review Moderation
    test_admin_review_moderation(session)

def test_admin_category_crud(session: TestSession):
    """Test admin category CRUD operations"""
    print("\n--- Testing Admin Category CRUD ---")

    # Create category
    try:
        response = session.session.get(f"{BASE_URL}/Admin/Categories/Create")
        token = session.get_antiforgery_token(response.text)

        if token:
            create_data = {
                'Name': 'Test Category',
                'Description': 'Test category description',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Admin/Categories/Create", data=create_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Admin", "/Admin/Categories/Create", "POST", response.status_code, result, "Create category")
    except Exception as e:
        log_result("Admin", "/Admin/Categories/Create", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Edit category
    try:
        response = session.session.get(f"{BASE_URL}/Admin/Categories/Edit/1")
        token = session.get_antiforgery_token(response.text)

        if token:
            update_data = {
                'CategoryId': 1,
                'Name': 'Updated Category',
                'Description': 'Updated description',
                '__RequestVerificationToken': token
            }
            response = session.session.post(f"{BASE_URL}/Admin/Categories/Edit/1", data=update_data)
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Admin", "/Admin/Categories/Edit/{id}", "POST", response.status_code, result, "Update category")
    except Exception as e:
        log_result("Admin", "/Admin/Categories/Edit/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

    # Delete category
    try:
        response = session.session.get(f"{BASE_URL}/Admin/Categories")
        token = session.get_antiforgery_token(response.text)

        if token:
            response = session.session.post(f"{BASE_URL}/Admin/Categories/Delete/1", data={'__RequestVerificationToken': token})
            result = "PASS" if response.status_code in [200, 302] else "FAIL"
            log_result("Admin", "/Admin/Categories/Delete/{id}", "POST", response.status_code, result, "Delete category")
    except Exception as e:
        log_result("Admin", "/Admin/Categories/Delete/{id}", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_admin_user_management(session: TestSession):
    """Test admin user management operations"""
    print("\n--- Testing Admin User Management ---")

    try:
        response = session.session.get(f"{BASE_URL}/Admin/Users")
        token = session.get_antiforgery_token(response.text)

        # Suspend user
        if token:
            response = session.session.post(
                f"{BASE_URL}/Admin/Users/Suspend/user-id-here",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302, 404] else "FAIL"
            log_result("Admin", "/Admin/Users/Suspend/{id}", "POST", response.status_code, result, "Suspend user")

        # Activate user
        if token:
            response = session.session.post(
                f"{BASE_URL}/Admin/Users/Activate/user-id-here",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302, 404] else "FAIL"
            log_result("Admin", "/Admin/Users/Activate/{id}", "POST", response.status_code, result, "Activate user")
    except Exception as e:
        log_result("Admin", "/Admin/Users/Suspend|Activate", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_admin_seller_approval(session: TestSession):
    """Test admin seller approval operations"""
    print("\n--- Testing Admin Seller Approval ---")

    try:
        response = session.session.get(f"{BASE_URL}/Admin/Sellers/Pending")
        token = session.get_antiforgery_token(response.text)

        # Approve seller
        if token:
            response = session.session.post(
                f"{BASE_URL}/Admin/Sellers/Approve/1",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302, 404] else "FAIL"
            log_result("Admin", "/Admin/Sellers/Approve/{id}", "POST", response.status_code, result, "Approve seller")

        # Reject seller
        if token:
            response = session.session.post(
                f"{BASE_URL}/Admin/Sellers/Reject/1",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302, 404] else "FAIL"
            log_result("Admin", "/Admin/Sellers/Reject/{id}", "POST", response.status_code, result, "Reject seller")
    except Exception as e:
        log_result("Admin", "/Admin/Sellers/Approve|Reject", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def test_admin_review_moderation(session: TestSession):
    """Test admin review moderation operations"""
    print("\n--- Testing Admin Review Moderation ---")

    try:
        response = session.session.get(f"{BASE_URL}/Admin/Reviews/Pending")
        token = session.get_antiforgery_token(response.text)

        # Approve review
        if token:
            response = session.session.post(
                f"{BASE_URL}/Admin/Reviews/Approve/1",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302, 404] else "FAIL"
            log_result("Admin", "/Admin/Reviews/Approve/{id}", "POST", response.status_code, result, "Approve review")

        # Reject review
        if token:
            response = session.session.post(
                f"{BASE_URL}/Admin/Reviews/Reject/1",
                data={'__RequestVerificationToken': token}
            )
            result = "PASS" if response.status_code in [200, 302, 404] else "FAIL"
            log_result("Admin", "/Admin/Reviews/Reject/{id}", "POST", response.status_code, result, "Reject review")
    except Exception as e:
        log_result("Admin", "/Admin/Reviews/Approve|Reject", "POST", 0, "FAIL", f"Error: {str(e)[:50]}")

def generate_report():
    """Generate comprehensive test report"""
    print("\n" + "="*80)
    print("TEST SUMMARY")
    print("="*80)

    # Count results by category
    categories = {}
    for result in TEST_RESULTS:
        cat = result['category']
        if cat not in categories:
            categories[cat] = {'total': 0, 'pass': 0, 'fail': 0}
        categories[cat]['total'] += 1
        if result['result'] == 'PASS':
            categories[cat]['pass'] += 1
        else:
            categories[cat]['fail'] += 1

    print(f"\n{'Category':<20} {'Total':<10} {'Pass':<10} {'Fail':<10} {'Pass Rate':<10}")
    print("-" * 60)

    total_tests = 0
    total_pass = 0
    total_fail = 0

    for cat, stats in sorted(categories.items()):
        pass_rate = (stats['pass'] / stats['total'] * 100) if stats['total'] > 0 else 0
        print(f"{cat:<20} {stats['total']:<10} {stats['pass']:<10} {stats['fail']:<10} {pass_rate:>6.1f}%")
        total_tests += stats['total']
        total_pass += stats['pass']
        total_fail += stats['fail']

    print("-" * 60)
    overall_pass_rate = (total_pass / total_tests * 100) if total_tests > 0 else 0
    print(f"{'TOTAL':<20} {total_tests:<10} {total_pass:<10} {total_fail:<10} {overall_pass_rate:>6.1f}%")

    # Show failures
    failures = [r for r in TEST_RESULTS if r['result'] == 'FAIL']
    if failures:
        print("\n" + "="*80)
        print("FAILURES")
        print("="*80)
        for f in failures:
            print(f"\n❌ [{f['category']}] {f['method']} {f['endpoint']}")
            print(f"   Status: {f['status']}")
            print(f"   Notes: {f['notes']}")

    # Save to JSON
    with open('test_results.json', 'w') as f:
        json.dump(TEST_RESULTS, f, indent=2)

    print("\n" + "="*80)
    print(f"Full results saved to: test_results.json")
    print("="*80)

def main():
    """Run all tests"""
    print("="*80)
    print("SHOPPINGAPP COMPREHENSIVE CRUD TEST SUITE")
    print("="*80)
    print(f"Target: {BASE_URL}")
    print("="*80)

    # Check if server is running
    try:
        response = requests.get(BASE_URL, timeout=5)
        print(f"✅ Server is running (Status: {response.status_code})")
    except Exception as e:
        print(f"❌ Server is not running: {e}")
        print(f"\nPlease start the server first:")
        print(f"  Visual Studio: Press F5")
        print(f"  Or run: iisexpress.exe /config:... /site:ShoppingApp")
        return

    # Run all tests
    test_public_endpoints()
    test_customer_endpoints()
    test_seller_endpoints()
    test_admin_endpoints()

    # Generate report
    generate_report()

if __name__ == "__main__":
    main()
