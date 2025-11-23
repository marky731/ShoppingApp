# ShoppingApp

A multi-seller e-commerce marketplace platform built with ASP.NET Core Web API and MySQL.

## Tech Stack

- **Frontend**: Vue 3, Vite, Pinia, Vue Router, Bootstrap 5
- **Backend**: ASP.NET Core 9.0 Web API
- **Database**: MySQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Architecture**: Layered Architecture with Repository Pattern

## Project Structure

```
ShoppingApp/
├── database/
│   ├── schema.sql              # MySQL database schema (28 tables)
│   └── sample_data.sql         # Optional test data
├── project_specs/              # Design documents
│   ├── use_cases_draft.md
│   ├── db_design_draft.md
│   ├── tech_stack.md
│   └── optimization_notes.md
├── setup.sh                    # Setup script (macOS/Linux)
├── setup.ps1                   # Setup script (Windows)
└── src/
    ├── ShoppingApp.Web/             # Frontend (Vue 3)
    │   ├── src/
    │   │   ├── components/         # Vue components
    │   │   ├── views/              # Page views
    │   │   ├── stores/             # Pinia stores
    │   │   ├── services/           # API services
    │   │   └── router/             # Vue Router
    │   └── package.json
    │
    ├── ShoppingApp.Core/           # Domain Layer
    │   ├── Entities/               # Domain models
    │   └── Interfaces/
    │       └── Repositories/       # Repository interfaces
    │
    ├── ShoppingApp.Infrastructure/ # Data Access Layer
    │   ├── Data/
    │   │   └── AppDbContext.cs
    │   └── Repositories/           # Repository implementations
    │
    └── ShoppingApp.API/            # Presentation Layer
        ├── Controllers/
        │   ├── Auth/
        │   └── Products/
        ├── Models/DTOs/
        ├── Services/
        └── Program.cs
```

## Getting Started

### Prerequisites

- Node.js 18+ (for frontend)
- .NET 9.0 SDK
- MySQL Server 8.0+

### Quick Setup (Recommended)

Setup scripts are available to automatically install dependencies and configure the database:

**macOS/Linux:**
```bash
./setup.sh
```

**Windows (PowerShell):**
```powershell
.\setup.ps1
```

The scripts will check prerequisites, install all dependencies, and guide you through database setup (with optional sample data).



### Database Setup

> **Note**: Each developer needs their own local MySQL instance. The database is not shared - you must create it locally and populate test data yourself.

1. Update connection string in `src/ShoppingApp.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=shopping_app;User=root;Password=YOUR_PASSWORD;"
  }
}
```

2. Create the database:
```bash
mysql -u root -p < database/schema.sql
```

3. The schema creates empty tables with seeded roles (customer, seller, admin). You can optionally populate test data:
```bash
mysql -u root -p shopping_app < database/sample_data.sql
```

### Running the Application

**Backend API:**
```bash
cd src/ShoppingApp.API
dotnet run
```
The API will be available at `http://localhost:5001`


**Frontend:**
```bash
cd src/ShoppingApp.Web
npm install
npm run dev
```
The frontend will be available at `http://localhost:5173`

## API Endpoints

### Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login & get JWT | No |
| GET | `/api/auth/me` | Get current user | Yes |

### Products
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/products` | List products (paginated, filterable) | No |
| GET | `/api/products/{id}` | Get product by ID | No |
| GET | `/api/products/slug/{slug}` | Get product by slug | No |

### Categories
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/categories` | List all categories | No |
| GET | `/api/categories/{id}` | Get category by ID | No |

### Query Parameters for Products

```
GET /api/products?page=1&pageSize=12&search=laptop&categoryId=1&minPrice=100&maxPrice=1000&brand=Apple&sortBy=price_asc
```

- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 12)
- `search` - Search in name, description, brand
- `categoryId` - Filter by category
- `minPrice` / `maxPrice` - Price range filter
- `brand` - Filter by brand
- `sortBy` - Sort options: `price_asc`, `price_desc`, `rating`, `newest`

## Architecture

### Layered Architecture

1. **Core Layer** (`ShoppingApp.Core`)
   - Domain entities
   - Repository interfaces
   - No external dependencies

2. **Infrastructure Layer** (`ShoppingApp.Infrastructure`)
   - EF Core DbContext
   - Repository implementations
   - Database configurations

3. **API Layer** (`ShoppingApp.API`)
   - Controllers
   - DTOs
   - Services
   - Dependency injection setup

### Repository Pattern

- `IRepository<T>` - Generic CRUD operations
- `IUnitOfWork` - Transaction management
- Specialized repositories for complex queries (e.g., `IProductRepository`)

## Database Schema

28 tables organized into:
- **User & Auth**: Roles, Users, Addresses, PasswordResets
- **Catalog**: Shops, Categories, Products, ProductImages, Attributes
- **Interactions**: Reviews, Favorites, CartItems
- **Orders**: Orders, ShopOrders, OrderItems, Discounts
- **System**: Notifications, Payments, Returns, OrderStatusHistory

See `project_specs/db_design_draft.md` for detailed schema.

## User Roles

- **Guest** - Browse products, search, view details
- **Customer** - Cart, orders, reviews, wishlist
- **Seller** - Product management, order fulfillment, shop profile
- **Admin** - User management, category management, review moderation

## Development

### Building

```bash
cd src/ShoppingApp.API
dotnet build
```

### Configuration

JWT settings in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyAtLeast32Characters",
    "Issuer": "ShoppingApp",
    "Audience": "ShoppingAppUsers",
    "ExpirationInMinutes": 60
  }
}
```

## Contributing

See `PROGRESS.md` for current development status and roadmap.

