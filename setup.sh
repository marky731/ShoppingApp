#!/bin/bash

# ShoppingApp Setup Script
# This script checks prerequisites and installs all dependencies

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================="
echo "   ShoppingApp Setup Script"
echo "========================================="
echo ""

# Track missing prerequisites
MISSING_PREREQS=()

# Check .NET SDK
echo -n "Checking .NET SDK... "
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}Found v$DOTNET_VERSION${NC}"

    # Check if version is 9.0+
    MAJOR_VERSION=$(echo $DOTNET_VERSION | cut -d. -f1)
    if [ "$MAJOR_VERSION" -lt 9 ]; then
        echo -e "${YELLOW}  Warning: .NET 9.0+ recommended (found $DOTNET_VERSION)${NC}"
    fi
else
    echo -e "${RED}Not found${NC}"
    MISSING_PREREQS+=(".NET SDK 9.0+ (https://dotnet.microsoft.com/download)")
fi

# Check Node.js
echo -n "Checking Node.js... "
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    echo -e "${GREEN}Found $NODE_VERSION${NC}"

    # Check if version is 18+
    MAJOR_VERSION=$(echo $NODE_VERSION | sed 's/v//' | cut -d. -f1)
    if [ "$MAJOR_VERSION" -lt 18 ]; then
        echo -e "${YELLOW}  Warning: Node.js 18+ recommended (found $NODE_VERSION)${NC}"
    fi
else
    echo -e "${RED}Not found${NC}"
    MISSING_PREREQS+=("Node.js 18+ (https://nodejs.org)")
fi

# Check npm
echo -n "Checking npm... "
if command -v npm &> /dev/null; then
    NPM_VERSION=$(npm --version)
    echo -e "${GREEN}Found v$NPM_VERSION${NC}"
else
    echo -e "${RED}Not found${NC}"
    MISSING_PREREQS+=("npm (comes with Node.js)")
fi

# Check MySQL
echo -n "Checking MySQL... "
if command -v mysql &> /dev/null; then
    MYSQL_VERSION=$(mysql --version | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)
    echo -e "${GREEN}Found v$MYSQL_VERSION${NC}"
else
    echo -e "${RED}Not found${NC}"
    MISSING_PREREQS+=("MySQL 8.0+ (https://dev.mysql.com/downloads/)")
fi

echo ""

# Exit if prerequisites are missing
if [ ${#MISSING_PREREQS[@]} -gt 0 ]; then
    echo -e "${RED}Missing prerequisites:${NC}"
    for prereq in "${MISSING_PREREQS[@]}"; do
        echo "  - $prereq"
    done
    echo ""
    echo "Please install the missing prerequisites and run this script again."
    exit 1
fi

echo -e "${GREEN}All prerequisites found!${NC}"
echo ""

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Install backend dependencies
echo "========================================="
echo "Installing Backend Dependencies"
echo "========================================="
cd "$SCRIPT_DIR/src/ShoppingApp.API"
echo "Restoring NuGet packages..."
dotnet restore
echo -e "${GREEN}Backend dependencies installed!${NC}"
echo ""

# Install frontend dependencies
echo "========================================="
echo "Installing Frontend Dependencies"
echo "========================================="
cd "$SCRIPT_DIR/src/ShoppingApp.Web"
echo "Installing npm packages..."
npm install
echo -e "${GREEN}Frontend dependencies installed!${NC}"
echo ""

# Database setup prompt
echo "========================================="
echo "Database Setup"
echo "========================================="
echo ""
echo "Do you want to set up the MySQL database? (y/n)"
echo "This will:"
echo "  - Create the 'shopping_app' database with schema"
echo "  - Optionally load sample data"
echo ""
read -p "Set up database? [y/N]: " SETUP_DB

if [[ "$SETUP_DB" =~ ^[Yy]$ ]]; then
    echo ""
    read -p "MySQL username [root]: " MYSQL_USER
    MYSQL_USER=${MYSQL_USER:-root}

    read -sp "MySQL password: " MYSQL_PASS
    echo ""

    echo "Creating database schema..."
    mysql -u "$MYSQL_USER" -p"$MYSQL_PASS" < "$SCRIPT_DIR/database/schema.sql"
    echo -e "${GREEN}Database schema created!${NC}"

    echo ""
    read -p "Load sample data? [y/N]: " LOAD_SAMPLE

    if [[ "$LOAD_SAMPLE" =~ ^[Yy]$ ]]; then
        echo "Loading sample data..."
        mysql -u "$MYSQL_USER" -p"$MYSQL_PASS" shopping_app < "$SCRIPT_DIR/database/sample_data.sql"
        echo -e "${GREEN}Sample data loaded!${NC}"
        echo ""
        echo "Test credentials:"
        echo "  Customer: john@example.com / Test1234"
        echo "  Seller: seller@example.com / Test1234"
    fi

    # Update connection string reminder
    echo ""
    echo -e "${YELLOW}Note:${NC} Update the connection string in"
    echo "  src/ShoppingApp.API/appsettings.json"
    echo "if your MySQL credentials differ from:"
    echo "  User=root;Password=shopapp123"
fi

echo ""
echo "========================================="
echo -e "${GREEN}Setup Complete!${NC}"
echo "========================================="
echo ""
echo "To run the application:"
echo ""
echo "  Backend (Terminal 1):"
echo "    cd src/ShoppingApp.API"
echo "    dotnet run"
echo "    # Runs at http://localhost:5001"
echo ""
echo "  Frontend (Terminal 2):"
echo "    cd src/ShoppingApp.Web"
echo "    npm run dev"
echo "    # Runs at http://localhost:5173"
echo ""
