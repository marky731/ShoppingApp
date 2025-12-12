#!/bin/bash

echo "=== ANALYZING ALL CONTROLLER ENDPOINTS ==="
echo ""

# Function to extract endpoints from a controller
extract_endpoints() {
    local file=$1
    local area=$2
    local controller=$(basename "$file" | sed 's/Controller.cs//')
    
    echo "[$area] $controller:"
    grep -E "^\s*public\s+(async\s+)?(Task<)?ActionResult" "$file" | \
        grep -v "//\|/\*" | \
        sed 's/.*public\s\+\(async\s\+\)\?\(Task<\)\?ActionResult\(>\)\?//' | \
        sed 's/(.*//' | \
        awk '{print "  - " $1}' | \
        sort -u
    echo ""
}

# Public Controllers
for file in ShoppingApp/Controllers/*Controller.cs; do
    if [ -f "$file" ]; then
        extract_endpoints "$file" "Public/Customer"
    fi
done

# Seller Area
for file in ShoppingApp/Areas/Seller/Controllers/*Controller.cs; do
    if [ -f "$file" ]; then
        extract_endpoints "$file" "Seller"
    fi
done

# Admin Area
for file in ShoppingApp/Areas/Admin/Controllers/*Controller.cs; do
    if [ -f "$file" ]; then
        extract_endpoints "$file" "Admin"
    fi
done
