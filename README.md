# ShoppingApp - MVC Refactor

This branch represents a complete architectural refactoring of the ShoppingApp project. The original architecture, which consisted of a separated ASP.NET Core API backend and a Vue.js frontend, has been replaced with a traditional, monolithic ASP.NET MVC application.

## Key Changes

- **Monolithic Architecture:** The project is now a single, self-contained ASP.NET MVC application. The backend and frontend are tightly coupled.
- **Project Structure:** The original `src` directory, which housed the API, Core, Infrastructure, and Web projects, has been removed. It is replaced by a new `ShoppingApp` directory that follows the standard ASP.NET MVC project structure.
- **Technology Stack:** The backend is now built with ASP.NET MVC 5, and the frontend is rendered using Razor views with Bootstrap and jQuery.

## Project Structure

The new project structure is as follows:

```
/
├── ShoppingApp.sln
├── ShoppingApp/
│   ├── App_Start/      # Configuration files (Bundles, Filters, Routes)
│   ├── Areas/          # Admin and Seller areas
│   ├── Content/        # CSS and fonts
│   ├── Controllers/    # MVC controllers
│   ├── Models/         # Domain models and ViewModels
│   ├── Scripts/        # JavaScript files
│   ├── Views/          # Razor views
│   ├── Global.asax
│   ├── Web.config
│   └── ...
└── ...
```

## How to Run the Application

1.  **Open the solution:** Open `ShoppingApp.sln` in Visual Studio.
2.  **Restore NuGet packages:** Right-click on the solution in the Solution Explorer and select "Restore NuGet Packages".
3.  **Update the database:** Open the Package Manager Console and run `Update-Database` to apply the Entity Framework migrations.
4.  **Run the application:** Press F5 or click the "Run" button in Visual Studio to start the application. It will be hosted on a local IIS Express server.
