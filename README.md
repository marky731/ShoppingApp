# ShoppingApp - MVC Refactor

This branch represents a significant architectural refactoring of the ShoppingApp project. The application has been transitioned from a decoupled, modern stack (ASP.NET Core API + Vue.js frontend) to a traditional, monolithic ASP.NET MVC application.

## Motivation for Refactoring

This refactoring was undertaken to simplify the project structure, potentially for easier development, deployment, or to align with specific project requirements favoring a more integrated approach. It consolidates the backend and frontend into a single codebase, leveraging the robust features of the ASP.NET MVC framework.

## Architectural Overview

### Previous Architecture

The original project followed a decoupled architecture:
-   **Backend:** ASP.NET Core API (likely C#) for handling business logic and data access.
-   **Frontend:** Vue.js application for the user interface, communicating with the API.
-   **Data Layer:** Separate class libraries for Core (domain entities) and Infrastructure (data access, e.g., Entity Framework).

### Current Architecture (MVC Refactor)

The refactored application is a monolithic ASP.NET MVC 5 application:
-   **Framework:** ASP.NET MVC 5 (C#)
-   **Frontend:** Razor views (`.cshtml`) for rendering HTML, styled with Bootstrap, and interactive elements powered by jQuery.
-   **Backend:** Controllers handle requests, interact with models (Entity Framework for data access), and return views.
-   **Database:** SQL Server (managed via Entity Framework Code First migrations).

## Key Changes

-   **Monolithic Structure:** The entire application (UI, business logic, data access) resides within a single ASP.NET project.
-   **Project Structure:** The `src` directory, which previously contained separate projects for API, Core, Infrastructure, and Web, has been removed. The new structure adheres to standard ASP.NET MVC conventions, with `Controllers`, `Views`, and `Models` directories directly under the `ShoppingApp` project.
-   **Technology Stack Shift:**
    -   From ASP.NET Core API to ASP.NET MVC 5.
    -   From Vue.js to Razor Views with Bootstrap and jQuery.
    -   Entity Framework remains for data access.

## Project Structure

The new project structure is organized as follows:

```
/
├── ShoppingApp.sln             # Visual Studio Solution file
├── ShoppingApp/                # Main ASP.NET MVC project
│   ├── App_Data/               # Application data files (e.g., local DB files)
│   ├── App_Start/              # Configuration for routing, bundling, identity, etc.
│   │   ├── BundleConfig.cs     # CSS and JavaScript bundling
│   │   ├── FilterConfig.cs     # Global MVC filters
│   │   ├── IdentityConfig.cs   # ASP.NET Identity configuration
│   │   ├── RouteConfig.cs      # URL routing configuration
│   │   └── Startup.Auth.cs     # OWIN authentication setup
│   ├── Areas/                  # Modular sections for Admin and Seller functionalities
│   │   ├── Admin/              # Admin-specific controllers, views, and registration
│   │   └── Seller/             # Seller-specific controllers, views, and registration
│   ├── Content/                # Static assets like CSS (Bootstrap, custom styles) and fonts
│   ├── Controllers/            # MVC Controllers handling user requests and returning views
│   ├── Migrations/             # Entity Framework Code First migrations
│   ├── Models/                 # Application's domain models, identity models, and ViewModels
│   │   ├── Domain/             # Core business entities (e.g., Product, Order, User)
│   │   ├── Identity/           # ASP.NET Identity related models (ApplicationUser, ApplicationDbContext)
│   │   └── ViewModels/         # Models specifically designed for views
│   ├── Scripts/                # JavaScript files (jQuery, Bootstrap JS, custom scripts)
│   ├── Views/                  # Razor View files (.cshtml) for rendering UI
│   │   ├── Shared/             # Partial views, layout files (_Layout.cshtml)
│   │   └── ...                 # Views for different controllers (Account, Home, Products, etc.)
│   ├── Global.asax             # Entry point for the application, handles global events
│   ├── packages.config         # NuGet package dependencies
│   ├── ShoppingApp.csproj      # Project file for the ASP.NET MVC application
│   ├── Startup.cs              # OWIN startup class
│   └── Web.config              # Main application configuration file
└── ...                         # Other project-level files (e.g., .gitignore, solution files)
```

## Core Features

The ShoppingApp provides the following functionalities:
-   **User Authentication & Authorization:** Secure user login, registration, and role-based access (Admin, Seller, Customer).
-   **Product Management:** Browse products, view details, and manage products (for sellers and admins).
-   **Shopping Cart & Checkout:** Add items to cart, proceed to checkout, and place orders.
-   **Order Management:** View order history (customers), manage orders (sellers, admins).
-   **Reviews & Ratings:** Users can leave reviews for products.
-   **User Profiles:** Manage personal information and addresses.
-   **Admin Dashboard:** Administrative functionalities for managing users, categories, orders, and sellers.
-   **Seller Dashboard:** Seller-specific functionalities for managing products, discounts, and orders.

## How to Run the Application

### Prerequisites

-   **Visual Studio:** Version 2019 or later (with ASP.NET and web development workload installed).
-   **.NET Framework:** Version 4.7.2 or later.
-   **SQL Server:** A local or remote SQL Server instance for the database.

### Setup Steps

1.  **Clone the Repository:**
    ```bash
    git clone <repository-url>
    cd ShoppingApp
    ```
2.  **Open in Visual Studio:**
    -   Open the `ShoppingApp.sln` solution file in Visual Studio.
3.  **Restore NuGet Packages:**
    -   In Visual Studio, right-click on the solution in the Solution Explorer.
    -   Select "Restore NuGet Packages" to download all required dependencies.
4.  **Database Setup and Migrations:**
    -   **Configure Connection String:** Open `Web.config` in the `ShoppingApp` project. Locate the `<connectionStrings>` section and update the `DefaultConnection` to point to your SQL Server instance.
        ```xml
        <connectionStrings>
            <add name="DefaultConnection" connectionString="Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\aspnet-ShoppingApp-20260101010101.mdf;Initial Catalog=aspnet-ShoppingApp-20260101010101;Integrated Security=True" providerName="System.Data.SqlClient" />
        </connectionStrings>
        ```
        *Note: You might need to adjust `Data Source` and `Initial Catalog` based on your SQL Server setup.*
    -   **Apply Migrations:**
        -   Go to `Tools` > `NuGet Package Manager` > `Package Manager Console`.
        -   Ensure "Default project" is set to `ShoppingApp`.
        -   Run the command: `Update-Database`
        This will create the database and apply all pending migrations, setting up the necessary tables.
5.  **Run the Application:**
    -   Press `F5` in Visual Studio or click the "IIS Express" button in the toolbar.
    -   The application will compile and launch in your default web browser, typically at `http://localhost:XXXXX` (where XXXXX is a dynamically assigned port).

## Further Development

-   Explore the `Areas` folder for Admin and Seller specific functionalities.
-   Review `App_Start` for application configuration details.
-   Examine `Controllers`, `Models`, and `Views` to understand the MVC pattern implementation.