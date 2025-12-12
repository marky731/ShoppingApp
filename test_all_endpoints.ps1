# ShoppingApp Comprehensive CRUD Test Suite
# PowerShell version

$BASE_URL = "http://localhost:8080"
$TEST_RESULTS = @()

# Test credentials
$CREDENTIALS = @{
    admin = @{ email = "admin@example.com"; password = "Test1234" }
    seller = @{ email = "seller@example.com"; password = "Test1234" }
    customer = @{ email = "john@example.com"; password = "Test1234" }
}

function Log-Result {
    param(
        [string]$Category,
        [string]$Endpoint,
        [string]$Method,
        [int]$Status,
        [string]$Result,
        [string]$Notes = ""
    )

    $script:TEST_RESULTS += [PSCustomObject]@{
        Category = $Category
        Endpoint = $Endpoint
        Method = $Method
        Status = $Status
        Result = $Result
        Notes = $Notes
    }

    $emoji = if ($Result -eq "PASS") { "✅" } else { "❌" }
    $notesShort = if ($Notes.Length -gt 50) { $Notes.Substring(0, 50) } else { $Notes }
    Write-Host "$emoji [$($Method.PadRight(6))] $($Endpoint.PadRight(50)) | $Status | $notesShort"
}

function Get-AntiForgeryToken {
    param([string]$Html)

    if ($Html -match '<input[^>]*name="__RequestVerificationToken"[^>]*value="([^"]+)"') {
        return $matches[1]
    }
    return $null
}

function Test-PublicEndpoints {
    Write-Host "`n$('='*80)"
    Write-Host "TESTING PUBLIC ENDPOINTS"
    Write-Host "$('='*80)"

    $endpoints = @(
        @("/", "Home page"),
        @("/Products", "Products listing"),
        @("/Products/Details/1", "Product details by ID"),
        @("/Products/Details/premium-smartphone-pro", "Product details by slug"),
        @("/Shops", "Shops listing"),
        @("/Shops/Details/1", "Shop details"),
        @("/Account/Login", "Login page"),
        @("/Account/Register", "Register page"),
        @("/Account/ForgotPassword", "Forgot password page")
    )

    foreach ($ep in $endpoints) {
        try {
            $response = Invoke-WebRequest -Uri "$BASE_URL$($ep[0])" -UseBasicParsing -ErrorAction SilentlyContinue
            $result = if ($response.StatusCode -eq 200) { "PASS" } else { "FAIL" }
            Log-Result -Category "Public" -Endpoint $ep[0] -Method "GET" -Status $response.StatusCode -Result $result -Notes $ep[1]
        } catch {
            Log-Result -Category "Public" -Endpoint $ep[0] -Method "GET" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
        }
    }
}

function Test-CustomerEndpoints {
    Write-Host "`n$('='*80)"
    Write-Host "TESTING CUSTOMER ENDPOINTS"
    Write-Host "$('='*80)"

    # Create session
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

    # Login
    try {
        $loginPage = Invoke-WebRequest -Uri "$BASE_URL/Account/Login" -SessionVariable session -UseBasicParsing
        $token = Get-AntiForgeryToken -Html $loginPage.Content

        if (-not $token) {
            Write-Host "❌ Failed to get anti-forgery token for login"
            return
        }

        $loginBody = @{
            Email = $CREDENTIALS.customer.email
            Password = $CREDENTIALS.customer.password
            RememberMe = "false"
            __RequestVerificationToken = $token
        }

        $loginResponse = Invoke-WebRequest -Uri "$BASE_URL/Account/Login" -Method POST -Body $loginBody -WebSession $session -UseBasicParsing -MaximumRedirection 5

        if ($loginResponse.Content -notmatch "logout") {
            Write-Host "❌ Failed to login as customer"
            return
        }

        Write-Host "✅ Logged in as customer"
    } catch {
        Write-Host "❌ Login error: $($_.Exception.Message)"
        return
    }

    # Test GET endpoints
    $getEndpoints = @(
        @("/Cart", "Shopping cart"),
        @("/Favorites", "Favorites list"),
        @("/Orders", "Order history"),
        @("/Profile", "User profile"),
        @("/Addresses", "Address list")
    )

    foreach ($ep in $getEndpoints) {
        try {
            $response = Invoke-WebRequest -Uri "$BASE_URL$($ep[0])" -WebSession $session -UseBasicParsing
            $result = if ($response.StatusCode -eq 200) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint $ep[0] -Method "GET" -Status $response.StatusCode -Result $result -Notes $ep[1]
        } catch {
            Log-Result -Category "Customer" -Endpoint $ep[0] -Method "GET" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
        }
    }

    # Test Address CRUD
    Test-AddressCRUD -Session $session

    # Test Cart operations
    Test-CartOperations -Session $session

    # Test Favorites
    Test-FavoritesOperations -Session $session

    # Test Profile
    Test-ProfileOperations -Session $session
}

function Test-AddressCRUD {
    param($Session)

    Write-Host "`n--- Testing Address CRUD ---"

    # GET Create page
    try {
        $response = Invoke-WebRequest -Uri "$BASE_URL/Addresses/Create" -WebSession $Session -UseBasicParsing
        $result = if ($response.StatusCode -eq 200) { "PASS" } else { "FAIL" }
        Log-Result -Category "Customer" -Endpoint "/Addresses/Create" -Method "GET" -Status $response.StatusCode -Result $result -Notes "Address create form"
        $token = Get-AntiForgeryToken -Html $response.Content
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Addresses/Create" -Method "GET" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
        return
    }

    # POST Create
    try {
        if ($token) {
            $createBody = @{
                AddressLabel = "Test Address"
                StreetAddress = "123 Test St"
                City = "Test City"
                StateProvince = "TC"
                PostalCode = "12345"
                Country = "Test Country"
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Addresses/Create" -Method POST -Body $createBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Addresses/Create" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Create new address"
        }
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Addresses/Create" -Method "POST" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }

    # GET Edit page
    try {
        $response = Invoke-WebRequest -Uri "$BASE_URL/Addresses/Edit/1" -WebSession $Session -UseBasicParsing
        $result = if ($response.StatusCode -eq 200) { "PASS" } else { "FAIL" }
        Log-Result -Category "Customer" -Endpoint "/Addresses/Edit/{id}" -Method "GET" -Status $response.StatusCode -Result $result -Notes "Address edit form"
        $token = Get-AntiForgeryToken -Html $response.Content
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Addresses/Edit/{id}" -Method "GET" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }

    # POST Edit
    try {
        if ($token) {
            $editBody = @{
                AddressId = 1
                AddressLabel = "Updated Address"
                StreetAddress = "456 Updated St"
                City = "Updated City"
                StateProvince = "UC"
                PostalCode = "54321"
                Country = "Updated Country"
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Addresses/Edit/1" -Method POST -Body $editBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Addresses/Edit/{id}" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Update address"
        }
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Addresses/Edit/{id}" -Method "POST" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }

    # POST Delete
    try {
        $response = Invoke-WebRequest -Uri "$BASE_URL/Addresses" -WebSession $Session -UseBasicParsing
        $token = Get-AntiForgeryToken -Html $response.Content

        if ($token) {
            $deleteBody = @{
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Addresses/Delete/1" -Method POST -Body $deleteBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Addresses/Delete/{id}" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Delete address"
        }
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Addresses/Delete/{id}" -Method "POST" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }
}

function Test-CartOperations {
    param($Session)

    Write-Host "`n--- Testing Cart Operations ---"

    try {
        $response = Invoke-WebRequest -Uri "$BASE_URL/Cart" -WebSession $Session -UseBasicParsing
        $token = Get-AntiForgeryToken -Html $response.Content

        # Add to cart
        if ($token) {
            $addBody = @{
                productId = 1
                quantity = 2
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Cart/Add" -Method POST -Body $addBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Cart/Add" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Add item to cart"
        }

        # Update cart
        if ($token) {
            $updateBody = @{
                productId = 1
                quantity = 3
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Cart/Update" -Method POST -Body $updateBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Cart/Update" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Update cart quantity"
        }

        # Remove from cart
        if ($token) {
            $removeBody = @{
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Cart/Remove/1" -Method POST -Body $removeBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Cart/Remove/{id}" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Remove item from cart"
        }
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Cart/*" -Method "POST" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }
}

function Test-FavoritesOperations {
    param($Session)

    Write-Host "`n--- Testing Favorites Operations ---"

    try {
        $response = Invoke-WebRequest -Uri "$BASE_URL/Favorites" -WebSession $Session -UseBasicParsing
        $token = Get-AntiForgeryToken -Html $response.Content

        if ($token) {
            $toggleBody = @{
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Favorites/Toggle/1" -Method POST -Body $toggleBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Favorites/Toggle/{id}" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Toggle favorite"
        }
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Favorites/Toggle/{id}" -Method "POST" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }
}

function Test-ProfileOperations {
    param($Session)

    Write-Host "`n--- Testing Profile Operations ---"

    # GET Edit page
    try {
        $response = Invoke-WebRequest -Uri "$BASE_URL/Profile/Edit" -WebSession $Session -UseBasicParsing
        $result = if ($response.StatusCode -eq 200) { "PASS" } else { "FAIL" }
        Log-Result -Category "Customer" -Endpoint "/Profile/Edit" -Method "GET" -Status $response.StatusCode -Result $result -Notes "Profile edit form"
        $token = Get-AntiForgeryToken -Html $response.Content
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Profile/Edit" -Method "GET" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
        return
    }

    # POST Edit
    try {
        if ($token) {
            $editBody = @{
                FirstName = "John"
                LastName = "Updated"
                PhoneNumber = "1234567890"
                __RequestVerificationToken = $token
            }

            $response = Invoke-WebRequest -Uri "$BASE_URL/Profile/Edit" -Method POST -Body $editBody -WebSession $Session -UseBasicParsing -MaximumRedirection 5
            $result = if ($response.StatusCode -in @(200, 302)) { "PASS" } else { "FAIL" }
            Log-Result -Category "Customer" -Endpoint "/Profile/Edit" -Method "POST" -Status $response.StatusCode -Result $result -Notes "Update profile"
        }
    } catch {
        Log-Result -Category "Customer" -Endpoint "/Profile/Edit" -Method "POST" -Status 0 -Result "FAIL" -Notes "Error: $($_.Exception.Message.Substring(0, [Math]::Min(50, $_.Exception.Message.Length)))"
    }
}

function Generate-Report {
    Write-Host "`n$('='*80)"
    Write-Host "TEST SUMMARY"
    Write-Host "$('='*80)"

    # Group by category
    $grouped = $script:TEST_RESULTS | Group-Object -Property Category

    Write-Host "`n$('Category'.PadRight(20)) $('Total'.PadRight(10)) $('Pass'.PadRight(10)) $('Fail'.PadRight(10)) $('Pass Rate'.PadRight(10))"
    Write-Host ("-" * 60)

    $totalTests = 0
    $totalPass = 0
    $totalFail = 0

    foreach ($group in $grouped | Sort-Object Name) {
        $pass = ($group.Group | Where-Object { $_.Result -eq "PASS" }).Count
        $fail = ($group.Group | Where-Object { $_.Result -eq "FAIL" }).Count
        $total = $group.Count
        $passRate = if ($total -gt 0) { ($pass / $total * 100) } else { 0 }

        Write-Host "$($group.Name.PadRight(20)) $($total.ToString().PadRight(10)) $($pass.ToString().PadRight(10)) $($fail.ToString().PadRight(10)) $("{0:N1}%" -f $passRate)"

        $totalTests += $total
        $totalPass += $pass
        $totalFail += $fail
    }

    Write-Host ("-" * 60)
    $overallPassRate = if ($totalTests -gt 0) { ($totalPass / $totalTests * 100) } else { 0 }
    Write-Host "$('TOTAL'.PadRight(20)) $($totalTests.ToString().PadRight(10)) $($totalPass.ToString().PadRight(10)) $($totalFail.ToString().PadRight(10)) $("{0:N1}%" -f $overallPassRate)"

    # Show failures
    $failures = $script:TEST_RESULTS | Where-Object { $_.Result -eq "FAIL" }
    if ($failures.Count -gt 0) {
        Write-Host "`n$('='*80)"
        Write-Host "FAILURES"
        Write-Host "$('='*80)"

        foreach ($f in $failures) {
            Write-Host "`n❌ [$($f.Category)] $($f.Method) $($f.Endpoint)"
            Write-Host "   Status: $($f.Status)"
            Write-Host "   Notes: $($f.Notes)"
        }
    }

    # Save to JSON
    $script:TEST_RESULTS | ConvertTo-Json -Depth 3 | Out-File "test_results.json" -Encoding UTF8

    Write-Host "`n$('='*80)"
    Write-Host "Full results saved to: test_results.json"
    Write-Host "$('='*80)"
}

# Main execution
Write-Host "$('='*80)"
Write-Host "SHOPPINGAPP COMPREHENSIVE CRUD TEST SUITE"
Write-Host "$('='*80)"
Write-Host "Target: $BASE_URL"
Write-Host "$('='*80)"

# Check if server is running
try {
    $response = Invoke-WebRequest -Uri $BASE_URL -UseBasicParsing -TimeoutSec 5
    Write-Host "✅ Server is running (Status: $($response.StatusCode))"
} catch {
    Write-Host "❌ Server is not running: $($_.Exception.Message)"
    Write-Host "`nPlease start the server first"
    exit 1
}

# Run all tests
Test-PublicEndpoints
Test-CustomerEndpoints

# Note: Seller and Admin tests would follow the same pattern
# For now, running Customer tests to demonstrate the approach

Generate-Report
