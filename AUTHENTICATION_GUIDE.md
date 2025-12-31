# ASP.NET Core Identity - Authentication & Authorization Guide

## Overview

The Cargo Parcel Tracker now features enterprise-grade authentication and authorization using ASP.NET Core Identity - Microsoft's production-ready identity management system.

## Features Implemented

### 1. **ASP.NET Core Identity Integration**
- Custom `ApplicationUser` extending `IdentityUser`
- Entity Framework Core storage with SQLite
- Secure password hashing (PBKDF2)
- Built-in protection against common attacks

### 2. **Role-Based Access Control (RBAC)**

#### **Roles:**
- **Trader**: Can view and create cargo parcels
- **Admin**: Can manage vessels and approve voyage allocations

#### **Access Matrix:**

| Feature | Trader | Admin |
|---------|--------|-------|
| View Dashboard | ✅ | ✅ |
| View Cargo Parcels | ✅ | ✅ |
| Create Cargo Parcels | ✅ | ✅ |
| Edit Cargo Parcels | ✅ | ✅ |
| Delete Cargo Parcels | ✅ | ✅ |
| View Vessels | ❌ | ✅ |
| Manage Vessels | ❌ | ✅ |
| View Voyage Allocations | ❌ | ✅ |
| Approve Allocations | ❌ | ✅ |

### 3. **Test User Accounts**

Two pre-seeded accounts for testing:

**Admin Account:**
- Username: `admin`
- Password: `admin123`
- Role: Admin
- Email: admin@shell.com

**Trader Account:**
- Username: `patryk`
- Password: `patryk123`
- Role: Trader
- Email: patryk@shell.com

### 4. **Authentication Features**

#### Login System
- Username/password authentication
- "Remember Me" functionality
- Return URL support for seamless redirects
- Secure cookie-based sessions
- 24-hour session expiration with sliding renewal

#### Registration System
- Self-service account creation
- Email validation
- Password confirmation
- Auto-assignment to "Trader" role
- Custom user fields (FullName)

#### Security Features
- Anti-forgery tokens on all forms
- Password requirements (relaxed for demo):
  - Minimum 6 characters
  - No complexity requirements (configurable)
- Unique email addresses
- Secure password storage (hashed and salted)

## Code Implementation

### 1. ApplicationUser Model
```csharp
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

### 2. DbContext Configuration
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // Existing DbSets...
}
```

### 3. Identity Configuration (Program.cs)
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    // ... more options
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### 4. Authorization Attributes

**Vessels Controller (Admin Only):**
```csharp
[Authorize(Roles = "Admin")]
public class VesselsController : Controller
{
    // Only administrators can access vessel management
}
```

**Cargo Parcels Controller (Trader & Admin):**
```csharp
[Authorize(Roles = "Trader,Admin")]
public class CargoParcelsController : Controller
{
    // Traders and administrators can manage cargo parcels
}
```

**Voyage Allocations Controller (Admin Only):**
```csharp
[Authorize(Roles = "Admin")]
public class VoyageAllocationsController : Controller
{
    // Only administrators can approve voyage allocations
}
```

### 5. View Integration

**Navigation Bar (_Layout.cshtml):**
```cshtml
@if (User.Identity?.IsAuthenticated == true)
{
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle">
            👤 @User.Identity.Name
        </a>
        <ul class="dropdown-menu">
            <li><h6>Role: @(User.IsInRole("Admin") ? "Admin" : "Trader")</h6></li>
            <li>
                <form asp-controller="Account" asp-action="Logout">
                    <button type="submit">Sign Out</button>
                </form>
            </li>
        </ul>
    </li>
}
else
{
    <li><a asp-controller="Account" asp-action="Login">Login</a></li>
    <li><a asp-controller="Account" asp-action="Register">Register</a></li>
}
```

## User Experience

### 1. **Unauthenticated Users**
- Can access public pages (Home)
- Redirected to login when accessing protected resources
- See Login/Register links in navigation

### 2. **Authenticated Traders**
- Access to cargo parcel management
- Restricted from vessel management
- See user menu with role information

### 3. **Authenticated Admins**
- Full access to all features
- Can manage vessels
- Can approve voyage allocations
- See user menu with admin role

### 4. **Access Denied Handling**
- Custom Access Denied page
- Clear explanation of role requirements
- Options to return home or sign out

## Database Schema

Identity adds these tables automatically:

- `AspNetUsers` - User accounts
- `AspNetRoles` - Role definitions
- `AspNetUserRoles` - User-Role mappings
- `AspNetUserClaims` - User claims
- `AspNetRoleClaims` - Role claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - Authentication tokens

## Security Considerations

### What's Implemented:
✅ Secure password hashing (PBKDF2)
✅ Anti-forgery token protection
✅ Role-based authorization
✅ Secure cookie authentication
✅ Protection against SQL injection (EF Core)
✅ Protection against XSS (Razor encoding)
✅ HTTPS support
✅ Unique email enforcement

### Production Recommendations:
- Enable email confirmation
- Implement two-factor authentication
- Add account lockout policies
- Strengthen password requirements
- Implement audit logging
- Add CAPTCHA to registration
- Enable HTTPS-only cookies
- Implement refresh tokens for API

## API Authentication

The Web API endpoints are currently open. To secure them:

```csharp
[Authorize(Roles = "Trader,Admin")]
[ApiController]
[Route("api/parcels")]
public class CargoParcelsApiController : ControllerBase
{
    // API methods protected by authentication
}
```

For token-based API auth, consider:
- JWT Bearer tokens
- OAuth 2.0 / OpenID Connect
- API keys with rate limiting

## Testing the Implementation

### 1. Test as Trader
```bash
1. Navigate to http://localhost:5000
2. Click "Login"
3. Enter credentials: patryk / patryk123
4. Try accessing:
   - ✅ Cargo Parcels (should work)
   - ❌ Vessels (should show Access Denied)
   - ❌ Voyage Allocations (should show Access Denied)
```

### 2. Test as Admin
```bash
1. Sign out (if logged in as Trader)
2. Login with: admin / admin123
3. Try accessing:
   - ✅ Cargo Parcels (should work)
   - ✅ Vessels (should work)
   - ✅ Voyage Allocations (should work)
```

### 3. Test Registration
```bash
1. Click "Register"
2. Create a new account
3. Verify you're assigned the "Trader" role
4. Test accessing resources
```

### 4. Test Logout
```bash
1. Click on your username dropdown
2. Click "Sign Out"
3. Verify you're redirected to home
4. Verify you can't access protected resources
```

## Extending the System

### Add New Roles
```csharp
// In IdentitySeeder.cs
string[] roles = { "Admin", "Trader", "Manager", "Viewer" };
```

### Add Claims-Based Authorization
```csharp
[Authorize(Policy = "RequireApprovalPermission")]
public IActionResult ApproveVoyage(int id)
{
    // Only users with approval permission
}
```

### Add Custom User Properties
```csharp
public class ApplicationUser : IdentityUser
{
    public string? Department { get; set; }
    public string? EmployeeId { get; set; }
}
```

## Architecture Highlights

### 1. **Separation of Concerns**
- Authentication logic in `AccountController`
- Authorization via attributes
- Identity configuration in `Program.cs`
- Database seeding in `IdentitySeeder`

### 2. **Convention Over Configuration**
- Uses ASP.NET Core Identity defaults
- Custom configuration where needed
- Follows Microsoft best practices

### 3. **Dependency Injection**
```csharp
public AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
{
    // Identity services injected automatically
}
```

### 4. **ViewModels for Clean APIs**
- `LoginViewModel` - Login form data
- `RegisterViewModel` - Registration form data
- Validation attributes on ViewModels
- Separate from domain models

## Resources

- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [Authorization in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authorization/introduction)
- [Security Best Practices](https://docs.microsoft.com/aspnet/core/security/authentication/identity-configuration)

---

**✅ ASP.NET Core Identity Implementation Complete**

The Cargo Parcel Tracker now demonstrates enterprise-grade authentication and authorization using .NET's built-in, production-ready identity system.
