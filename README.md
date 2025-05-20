# Wacky Business Cards - Software Maintenance Manual

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [Key Components](#key-components)
4. [Database Schema](#database-schema)
5. [Authentication & Authorization](#authentication--authorization)
6. [Making Changes](#making-changes)
7. [Common Tasks](#common-tasks)
8. [Troubleshooting](#troubleshooting)
9. [Deployment](#deployment)

## System Overview

Wacky Business Cards is a web application that allows users to create, customize, and manage digital business cards. The system includes user authentication, role-based authorization, and administrative capabilities.

### Core Features
- **User Management**: Registration, login, profile management, and admin controls
- **Business Card Creation**: Customizable business cards with various styling options
- **Admin Dashboard**: User management, statistics, and activity monitoring
- **Email Notifications**: Welcome emails, password reset, and activity notifications

## Architecture

The application follows the Model-View-Controller (MVC) architectural pattern and is built using ASP.NET Core 9.0.

### Technology Stack
- **Backend**: C# / ASP.NET Core 9.0
- **Frontend**: HTML, CSS, JavaScript, Bootstrap 5
- **Database**: SQLite (can be migrated to other providers supported by EF Core)
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Dependency Injection**: Built-in ASP.NET Core DI container

### Project Structure
```
WackyBusinessCards/
├── Controllers/           # MVC Controllers
├── Data/                  # Database context and migrations
├── EmailTemplates/        # HTML email templates
├── Models/                # Domain models
├── Services/              # Business logic services
├── ViewModels/            # View-specific models
├── Views/                 # Razor views
│   ├── Admin/             # Admin-specific views
│   ├── Home/              # Public-facing views
│   ├── Password/          # Password management views
│   ├── Profile/           # User profile views
│   └── Shared/            # Shared layouts and partials
├── wwwroot/               # Static files (CSS, JS, images)
│   ├── css/               # Stylesheets
│   ├── js/                # JavaScript files
│   ├── lib/               # Third-party libraries
│   └── uploads/           # User-uploaded files
├── Program.cs             # Application entry point and configuration
└── appsettings.json       # Application settings
```

## Key Components

### Services

The application uses a service-oriented architecture with the following key services:

1. **BusinessCardService**
   - Handles creation, retrieval, and management of business cards
   - Manages card styling and customization
   - Located in `Services/BusinessCardService.cs`
   - Key methods:
     - `GetBusinessCardAsync`: Retrieves a business card by ID
     - `GetUserBusinessCardsAsync`: Gets all business cards for a user
     - `CreateBusinessCardAsync`: Creates a new business card
     - `UpdateBusinessCardAsync`: Updates an existing business card
     - `DeleteBusinessCardAsync`: Deletes a business card

2. **UserManagementService**
   - Handles user creation, role management, and user deletion
   - Integrates with ASP.NET Core Identity
   - Located in `Services/UserManagementService.cs`
   - Key methods:
     - `CreateUserAsync`: Creates a new user with specified roles
     - `GetUsersAsync`: Retrieves users with their roles
     - `ToggleUserRoleAsync`: Adds or removes a role from a user
     - `DeleteUserAsync`: Deletes a user and their associated data

3. **ActivityLogService**
   - Tracks user and system activities
   - Provides audit trail for administrative actions
   - Located in `Services/ActivityLogService.cs`
   - Key methods:
     - `LogActivityAsync`: Records an activity in the system
     - `GetUserActivitiesAsync`: Gets activities for a specific user
     - `GetRecentActivitiesAsync`: Gets recent system activities

4. **EmailService**
   - Manages email communications
   - Uses templates for consistent messaging
   - Located in `Services/EmailService.cs`
   - Key methods:
     - `SendEmailAsync`: Sends a generic email
     - `SendWelcomeEmailAsync`: Sends a welcome email to new users
     - `SendPasswordResetEmailAsync`: Sends password reset links
     - `SendAccountActivityNotificationAsync`: Notifies users of account activity

5. **FileUploadService**
   - Handles file uploads (profile pictures, card images)
   - Manages file storage and retrieval
   - Located in `Services/FileUploadService.cs`
   - Key methods:
     - `UploadFileAsync`: Uploads a file to the server
     - `DeleteFileAsync`: Removes a file from the server

6. **StatisticsService**
   - Generates system statistics for the admin dashboard
   - Tracks user growth and system usage
   - Located in `Services/StatisticsService.cs`
   - Key methods:
     - `GetDashboardStatisticsAsync`: Gets overview statistics
     - `GetUserGrowthDataAsync`: Gets user registration trends
     - `GetCardCreationDataAsync`: Gets business card creation trends

### Controllers

1. **HomeController**
   - Handles public-facing pages
   - Manages business card display and creation for regular users
   - Located in `Controllers/HomeController.cs`
   - Key actions:
     - `Index`: Displays the home page
     - `Details`: Shows a specific business card
     - `Create`: Handles business card creation
     - `Edit`: Manages business card editing

2. **AdminController**
   - Provides administrative functionality
   - Manages users, roles, and system settings
   - Located in `Controllers/AdminController.cs`
   - Key actions:
     - `Dashboard`: Shows admin dashboard with statistics
     - `Users`: Lists all users in the system
     - `UserDetails`: Shows detailed user information
     - `CreateUser`: Handles user creation by admins
     - `DeleteUser`: Manages user deletion

3. **ProfileController**
   - Handles user profile management
   - Manages user-specific business cards
   - Located in `Controllers/ProfileController.cs`
   - Key actions:
     - `Index`: Shows user profile
     - `Edit`: Handles profile editing
     - `ChangePassword`: Manages password changes

4. **PasswordController**
   - Handles password reset functionality
   - Manages secure password changes
   - Located in `Controllers/PasswordController.cs`
   - Key actions:
     - `ForgotPassword`: Initiates password reset
     - `ResetPassword`: Handles password reset completion

## Database Schema

The application uses Entity Framework Core with the following main entities:

1. **ApplicationUser** (extends IdentityUser)
   - Standard identity properties (Email, UserName, etc.)
   - Extended properties (FirstName, LastName, ProfilePicturePath, etc.)
   - Located in `Models/ApplicationUser.cs`

2. **BusinessCard**
   - Core properties (Name, Title, Company, etc.)
   - Styling properties (Colors, Fonts, Borders, etc.)
   - Relationship to User (UserId)
   - Located in `Models/BusinessCard.cs`

3. **ActivityLog**
   - Tracking information (Action, Timestamp, etc.)
   - Relationship to User (UserId)
   - Entity information (EntityType, EntityId)
   - Located in `Models/ActivityLog.cs`

### Database Context

The `ApplicationDbContext` class (in `Data/ApplicationDbContext.cs`) defines the database structure and relationships. It includes:

- DbSet properties for each entity
- Entity configuration in the `OnModelCreating` method
- Relationship definitions

## Authentication & Authorization

### Authentication
- Uses ASP.NET Core Identity for user authentication
- Supports email/password login
- Includes password reset functionality
- Configuration in `Program.cs`

### Authorization
- Role-based authorization with two primary roles:
  - **Admin**: Full system access including user management
  - **User**: Standard access to create and manage own business cards
- Policy-based authorization for specific features
- Authorization attributes on controllers and actions
- Role constants defined in `Constants/Roles.cs`

## Making Changes

### Adding a New Feature

1. **Plan the Feature**
   - Define the requirements and user stories
   - Identify affected components (models, views, controllers, services)

2. **Update the Data Model (if needed)**
   - Add or modify entity classes in the Models directory
   - Create a new migration:
     ```
     dotnet ef migrations add [MigrationName]
     dotnet ef database update
     ```

3. **Implement Business Logic**
   - Add or update services in the Services directory
   - Register new services in Program.cs if needed

4. **Create/Update Controllers**
   - Add controller actions for the new feature
   - Apply appropriate authorization attributes

5. **Create/Update Views**
   - Add or modify Razor views
   - Use consistent styling (Bootstrap 5 components)

6. **Test the Feature**
   - Test all user flows and edge cases
   - Verify authorization works correctly

### Modifying Existing Features

1. **Identify the Components**
   - Locate the relevant controllers, services, and views
   - Understand the current implementation

2. **Make Targeted Changes**
   - Update only the necessary components
   - Maintain existing patterns and conventions

3. **Test Thoroughly**
   - Verify the modified feature works as expected
   - Check for unintended side effects

## Common Tasks

### Adding a New User Role

1. Add the role name to the `Roles` class in `Constants/Roles.cs`
2. Ensure the role is created in the database (via `RoleManager` in a service or startup code)
3. Update authorization policies in `Program.cs` if needed
4. Add role-specific UI elements in views

```csharp
// Example: Adding a new "Editor" role
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Editor = "Editor"; // New role
}

// In a service or initialization code
await roleManager.CreateAsync(new IdentityRole(Roles.Editor));
```

### Adding a New Business Card Property

1. Update the `BusinessCard` model in `Models/BusinessCard.cs`
2. Create a migration and update the database
3. Update the `BusinessCardViewModel` in `ViewModels/BusinessCardViewModel.cs`
4. Modify the card creation/edit views
5. Update the `BusinessCardService` to handle the new property

```csharp
// Example: Adding a "LinkedIn" property
public class BusinessCard
{
    // Existing properties...

    [StringLength(100)]
    public string? LinkedIn { get; set; }
}
```

### Adding a New Email Notification

1. Create an HTML template in the `EmailTemplates` directory
2. Add a new method to the `EmailService` class
3. Call the method from the appropriate service or controller

```csharp
// Example: Adding a card shared notification
public async Task<bool> SendCardSharedEmailAsync(string to, string sharerName, int cardId)
{
    var subject = "Someone shared a business card with you";
    var htmlContent = await GetEmailTemplateAsync("CardShared", new
    {
        SharerName = sharerName,
        CardUrl = _configuration["ApplicationUrl"] + $"/Home/Details/{cardId}",
        CurrentYear = DateTime.Now.Year.ToString()
    });

    return await SendEmailAsync(to, subject, htmlContent);
}
```

## Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Check the connection string in `appsettings.json`
   - Verify the database file exists and is accessible
   - Run `dotnet ef database update` to ensure migrations are applied

2. **Authentication Problems**
   - Check Identity configuration in `Program.cs`
   - Verify user roles are correctly assigned
   - Check authorization policies and attributes

3. **Email Sending Failures**
   - Verify SMTP settings in `appsettings.json`
   - Check if `EmailSettings:Enabled` is set to `true`
   - Look for exceptions in the logs

4. **File Upload Issues**
   - Check if the uploads directory exists and has write permissions
   - Verify file size limits in `Program.cs`
   - Check for proper encoding in form submissions

### Logging

The application uses the built-in ASP.NET Core logging system. Logs can be found:

- In the console output during development
- In log files if configured (see `appsettings.json`)

To add additional logging:

```csharp
_logger.LogInformation("This is an information message");
_logger.LogWarning("This is a warning message");
_logger.LogError(exception, "An error occurred");
```

## Deployment

### Prerequisites
- .NET 9.0 SDK or Runtime
- Web server (IIS, Nginx, etc.) or cloud hosting
- Database server (if not using SQLite)

### Deployment Steps

1. **Prepare the Application**
   - Update `appsettings.Production.json` with production settings
   - Set `EmailSettings:Enabled` to `true` for production
   - Configure proper connection strings

2. **Build and Publish**
   ```
   dotnet publish -c Release -o ./publish
   ```

3. **Deploy the Files**
   - Copy the published files to your web server
   - Set appropriate permissions

4. **Configure the Web Server**
   - Set up the application in IIS, Nginx, or your preferred server
   - Configure HTTPS

5. **Initialize the Database**
   - Run migrations or deploy the database schema
   - Seed initial data if needed

6. **Verify the Deployment**
   - Test all key functionality
   - Check logs for any errors

### Scaling Considerations

- Consider moving from SQLite to SQL Server or PostgreSQL for higher traffic
- Implement caching for frequently accessed data
- Use a CDN for static assets
- Consider containerization with Docker for easier deployment

## Code Structure and Patterns

### Service Pattern
The application follows the service pattern to encapsulate business logic:

1. **Interface Definition**: Services have corresponding interfaces (e.g., `IBusinessCardService`)
2. **Dependency Injection**: Services are registered in `Program.cs` and injected where needed
3. **Separation of Concerns**: Each service handles a specific domain of functionality

### Repository Pattern
While not explicitly implemented as separate repository classes, the Entity Framework DbContext acts as a repository:

1. **Data Access**: All database access is through the `ApplicationDbContext`
2. **Query Abstraction**: LINQ queries provide an abstraction over raw SQL
3. **Unit of Work**: The DbContext implements the Unit of Work pattern

### View Models
The application uses view models to separate domain models from presentation:

1. **Input Models**: Used to capture user input (e.g., `CreateBusinessCardViewModel`)
2. **Output Models**: Used to display data (e.g., `BusinessCardDetailsViewModel`)
3. **Mapping**: Manual mapping between domain models and view models in services or controllers

## Security Considerations

### Data Protection
1. **Input Validation**: All user inputs are validated using data annotations and model validation
2. **XSS Prevention**: Output encoding in Razor views prevents cross-site scripting
3. **CSRF Protection**: Anti-forgery tokens are used in forms

### Authentication Security
1. **Password Hashing**: ASP.NET Core Identity handles secure password storage
2. **Account Lockout**: Configurable in `Program.cs` to prevent brute force attacks
3. **Secure Cookies**: Authentication cookies are secured with HTTPS

### Authorization
1. **Role-Based Access**: Controllers and actions are protected with role requirements
2. **Resource-Based Access**: Business card operations check for ownership
3. **Principle of Least Privilege**: Users only have access to what they need

## Performance Optimization

### Database Optimization
1. **Indexing**: Key lookup fields are indexed
2. **Eager Loading**: Related entities are loaded with `Include()` to prevent N+1 query problems
3. **Paging**: Large result sets use paging to limit data transfer

### Caching Opportunities
1. **Output Caching**: Frequently accessed, rarely changing pages can be cached
2. **Data Caching**: Common lookup data can be cached in memory
3. **Static Content**: Static files are served with appropriate cache headers

## Testing

### Unit Testing
1. **Test Project**: Create a separate test project (e.g., `WackyBusinessCards.Tests`)
2. **Service Testing**: Focus on testing service methods with mocked dependencies
3. **Controller Testing**: Test controller actions with mocked services

### Integration Testing
1. **Database Testing**: Test with an in-memory or test database
2. **API Testing**: Test controller endpoints with HTTP clients
3. **Authentication Testing**: Test secured endpoints with authenticated requests

### UI Testing
1. **Selenium**: Use Selenium for browser automation tests
2. **Page Object Pattern**: Implement page objects for UI testing
3. **Accessibility Testing**: Verify WCAG compliance

## Future Enhancements

### Potential Improvements
1. **API Layer**: Add a RESTful API for mobile applications
2. **Localization**: Add support for multiple languages
3. **Advanced Analytics**: Implement detailed usage analytics
4. **Social Integration**: Add social media sharing and login options

### Technical Debt
1. **Code Duplication**: Identify and refactor duplicated code
2. **Performance Bottlenecks**: Profile and optimize slow operations
3. **Test Coverage**: Increase unit and integration test coverage

## Conclusion

This maintenance manual provides a comprehensive overview of the Wacky Business Cards application architecture and guidelines for maintaining and extending the system. Developers should follow the established patterns and practices to ensure consistency and maintainability.

By understanding the system's architecture, components, and common tasks, developers can efficiently make changes and enhancements while maintaining the integrity and security of the application.

---

**Document Version**: 1.0
**Last Updated**: June 2024
**Created By**: Augment Code AI Assistant
