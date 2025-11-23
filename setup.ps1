# ShoppingApp Setup Script for Windows
# Run in PowerShell: .\setup.ps1

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   ShoppingApp Setup Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$missingPrereqs = @()

# Check .NET SDK
Write-Host -NoNewline "Checking .NET SDK... "
try {
    $dotnetVersion = dotnet --version 2>$null
    Write-Host "Found v$dotnetVersion" -ForegroundColor Green
    $majorVersion = [int]($dotnetVersion.Split('.')[0])
    if ($majorVersion -lt 9) {
        Write-Host "  Warning: .NET 9.0+ recommended" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Not found" -ForegroundColor Red
    $missingPrereqs += ".NET SDK 9.0+ (https://dotnet.microsoft.com/download)"
}

# Check Node.js
Write-Host -NoNewline "Checking Node.js... "
try {
    $nodeVersion = node --version 2>$null
    Write-Host "Found $nodeVersion" -ForegroundColor Green
    $majorVersion = [int]($nodeVersion.TrimStart('v').Split('.')[0])
    if ($majorVersion -lt 18) {
        Write-Host "  Warning: Node.js 18+ recommended" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Not found" -ForegroundColor Red
    $missingPrereqs += "Node.js 18+ (https://nodejs.org)"
}

# Check npm
Write-Host -NoNewline "Checking npm... "
try {
    $npmVersion = npm --version 2>$null
    Write-Host "Found v$npmVersion" -ForegroundColor Green
} catch {
    Write-Host "Not found" -ForegroundColor Red
    $missingPrereqs += "npm (comes with Node.js)"
}

# Check MySQL
Write-Host -NoNewline "Checking MySQL... "
try {
    $mysqlOutput = mysql --version 2>$null
    if ($mysqlOutput -match '(\d+\.\d+\.\d+)') {
        Write-Host "Found v$($Matches[1])" -ForegroundColor Green
    } else {
        Write-Host "Found" -ForegroundColor Green
    }
} catch {
    Write-Host "Not found" -ForegroundColor Red
    $missingPrereqs += "MySQL 8.0+ (https://dev.mysql.com/downloads/)"
}

Write-Host ""

# Exit if prerequisites missing
if ($missingPrereqs.Count -gt 0) {
    Write-Host "Missing prerequisites:" -ForegroundColor Red
    foreach ($prereq in $missingPrereqs) {
        Write-Host "  - $prereq"
    }
    Write-Host ""
    Write-Host "Please install the missing prerequisites and run this script again."
    exit 1
}

Write-Host "All prerequisites found!" -ForegroundColor Green
Write-Host ""

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Install backend dependencies
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Installing Backend Dependencies" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Set-Location "$scriptDir\src\ShoppingApp.API"
Write-Host "Restoring NuGet packages..."
dotnet restore
Write-Host "Backend dependencies installed!" -ForegroundColor Green
Write-Host ""

# Install frontend dependencies
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Installing Frontend Dependencies" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Set-Location "$scriptDir\src\ShoppingApp.Web"
Write-Host "Installing npm packages..."
npm install
Write-Host "Frontend dependencies installed!" -ForegroundColor Green
Write-Host ""

# Database setup
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Database Setup" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Do you want to set up the MySQL database? (y/n)"
Write-Host "This will:"
Write-Host "  - Create the 'shopping_app' database with schema"
Write-Host "  - Optionally load sample data"
Write-Host ""

$setupDb = Read-Host "Set up database? [y/N]"

if ($setupDb -match '^[Yy]$') {
    $mysqlUser = Read-Host "MySQL username [root]"
    if ([string]::IsNullOrEmpty($mysqlUser)) { $mysqlUser = "root" }

    $mysqlPass = Read-Host "MySQL password" -AsSecureString
    $mysqlPassPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($mysqlPass))

    Write-Host "Creating database schema..."
    Get-Content "$scriptDir\database\schema.sql" | mysql -u $mysqlUser -p"$mysqlPassPlain"
    Write-Host "Database schema created!" -ForegroundColor Green

    $loadSample = Read-Host "Load sample data? [y/N]"

    if ($loadSample -match '^[Yy]$') {
        Write-Host "Loading sample data..."
        Get-Content "$scriptDir\database\sample_data.sql" | mysql -u $mysqlUser -p"$mysqlPassPlain" shopping_app
        Write-Host "Sample data loaded!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Test credentials:"
        Write-Host "  Customer: john@example.com / Test1234"
        Write-Host "  Seller: seller@example.com / Test1234"
    }

    Write-Host ""
    Write-Host "Note: Update the connection string in" -ForegroundColor Yellow
    Write-Host "  src\ShoppingApp.API\appsettings.json"
    Write-Host "if your MySQL credentials differ from:"
    Write-Host "  User=root;Password=shopapp123"
}

Set-Location $scriptDir

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:"
Write-Host ""
Write-Host "  Backend (Terminal 1):"
Write-Host "    cd src\ShoppingApp.API"
Write-Host "    dotnet run"
Write-Host "    # Runs at http://localhost:5001"
Write-Host ""
Write-Host "  Frontend (Terminal 2):"
Write-Host "    cd src\ShoppingApp.Web"
Write-Host "    npm run dev"
Write-Host "    # Runs at http://localhost:5173"
Write-Host ""
