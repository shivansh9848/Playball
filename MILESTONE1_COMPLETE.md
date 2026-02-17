# Playball Sports Booking Platform - Milestone 1 ✅

## 🎉 Implementation Complete

Milestone 1 has been fully implemented with the following features:

### ✅ Completed Tasks

1. **Core Setup** - .NET 10.0 Web API project configured
2. **Database Schema** - Entity Framework Core with SQL Server
3. **User Authentication** - JWT-based authentication with BCrypt password hashing
4. **Role-Based Authorization** - 4 roles: Admin, VenueOwner, GameOwner, User
5. **Swagger Documentation** - Integrated with JWT Bearer authentication
6. **Middleware & Filters** - Exception handling and role-based authorization
7. **Repository Pattern** - Generic repository with specific implementations
8. **Domain Entities** - All 11 entities created with proper relationships
9. **Database Migrations** - Initial migration created and ready to apply

---

## 🗄️ Database Setup

### Option 1: SQL Server LocalDB (Recommended for Development)

If you have SQL Server LocalDB installed:

```bash
dotnet ef database update --project Assignment_Example_HU.csproj
```

### Option 2: SQL Server Express/Full

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PlayballDB;Trusted_Connection=true;TrustServerCertificate=true"
}
```

Then run:
```bash
dotnet ef database update --project Assignment_Example_HU.csproj
```

### Option 3: SQL Server with Username/Password

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=PlayballDB;User Id=your-username;Password=your-password;TrustServerCertificate=true"
}
```

---

## 🚀 Running the Application

1. **Restore packages** (if not already done):
   ```bash
   dotnet restore Assignment_Example_HU.csproj
   ```

2. **Build the project**:
   ```bash
   dotnet build Assignment_Example_HU.csproj
   ```

3. **Apply migrations** (ensure SQL Server is running):
   ```bash
   dotnet ef database update --project Assignment_Example_HU.csproj
   ```

4. **Run the application**:
   ```bash
   dotnet run --project Assignment_Example_HU.csproj
   ```

5. **Access Swagger UI**:
   - Navigate to: `https://localhost:{port}` or check the console for the URL
   - Swagger UI will open automatically (configured as root page)

---

## 🔐 Default Admin Credentials

A default admin user is seeded automatically:

- **Email**: `admin@playball.com`
- **Password**: `Admin@123`
- **Role**: Admin

---

## 📋 API Endpoints (Milestone 1)

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login and get JWT token | No |
| GET | `/api/auth/profile` | Get current user profile | Yes |
| GET | `/api/auth/users/{id}/profile` | Get user profile by ID | Yes |

---

## 🧪 Testing the API

### 1. Register a New User

**POST** `/api/auth/register`

```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "1234567890",
  "password": "Test@123",
  "confirmPassword": "Test@123",
  "role": 4
}
```

**Roles:**
- `1` = Admin
- `2` = VenueOwner
- `3` = GameOwner
- `4` = User (default)

### 2. Login

**POST** `/api/auth/login`

```json
{
  "email": "admin@playball.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "fullName": "System Admin",
    "email": "admin@playball.com",
    "phoneNumber": "1234567890",
    "role": 1,
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-02-18T00:00:00Z"
  }
}
```

### 3. Use JWT Token in Swagger

1. Copy the `token` value from the login response
2. Click the **Authorize** button (🔒) in Swagger UI
3. Enter: `Bearer {your-token-here}`
4. Click **Authorize**
5. Now you can call protected endpoints

### 4. Get User Profile

**GET** `/api/auth/profile`

Headers:
```
Authorization: Bearer {your-token}
```

---

## 🏗️ Project Structure

```
Playball/
├── Domain/
│   ├── Entities/          # 11 domain entities
│   ├── Enums/             # 6 enumerations
│   └── Constants/         # Business constants
├── Application/
│   ├── Services/          # AuthService
│   ├── Interfaces/        # IAuthService
│   ├── DTOs/
│   │   ├── Request/       # RegisterUserRequest, LoginRequest
│   │   └── Response/      # AuthResponse, UserProfileResponse, ApiResponse
│   └── Validators/        # FluentValidation validators
├── Infrastructure/
│   ├── Data/              # ApplicationDbContext, Migrations
│   └── Repositories/      # Repository pattern implementation
├── API/
│   ├── Middleware/        # ExceptionHandlingMiddleware
│   └── Filters/           # AuthorizeRolesAttribute
├── Common/
│   ├── Exceptions/        # Custom exceptions
│   ├── Helpers/           # JwtHelper, PasswordHasher
│   └── Extensions/        # ServiceCollectionExtensions
└── Controllers/           # AuthController
```

---

## 🔑 Key Features Implemented

### 1. **JWT Authentication**
- Secure token-based authentication
- 24-hour token expiry
- Claims-based authorization

### 2. **Password Security**
- BCrypt hashing with salt
- Strong password validation (min 8 chars, uppercase, lowercase, number, special char)

### 3. **Role-Based Authorization**
- Custom `AuthorizeRoles` attribute
- 4 distinct user roles
- Role claims in JWT tokens

### 4. **Exception Handling**
- Global exception handling middleware
- Custom exception types
- Structured error responses

### 5. **Repository Pattern**
- Generic repository for CRUD operations
- Specific repositories for specialized queries
- Async/await throughout

### 6. **Validation**
- FluentValidation for DTOs
- Data annotations on entities
- Model state validation

### 7. **API Response Format**
```json
{
  "success": true/false,
  "message": "...",
  "data": { ... },
  "errors": { ... }
}
```

---

## 📦 NuGet Packages

- `Microsoft.EntityFrameworkCore` (10.0.1)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.1)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.1)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.1)
- `System.IdentityModel.Tokens.Jwt` (8.3.1)
- `FluentValidation.AspNetCore` (11.3.0)
- `BCrypt.Net-Next` (4.0.3)
- `Swashbuckle.AspNetCore` (6.8.1)

---

## 🗃️ Database Entities

1. **User** - User accounts with roles
2. **Wallet** - User wallet for payments (1:1 with User)
3. **Transaction** - Wallet transaction history
4. **Venue** - Sports venues owned by users
5. **Court** - Courts/fields within venues
6. **Discount** - Time-bound discounts
7. **Booking** - Slot bookings
8. **Game** - Games created by users
9. **GameParticipant** - Players in games
10. **Waitlist** - Waitlist for full games
11. **Rating** - Ratings for venues, courts, and players

---

## 🔧 Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm!",
    "Issuer": "PlayballAPI",
    "Audience": "PlayballClients",
    "ExpiryHours": "24"
  }
}
```

**⚠️ Important**: Change the JWT secret in production!

---

## 🐛 Troubleshooting

### Database Connection Issues

If you get `Unable to locate a Local Database Runtime installation`:

1. **Install SQL Server LocalDB**:
   - Download from [Microsoft SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - Or use SQL Server Express/Full edition

2. **Or use Docker**:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
   ```
   
   Update connection string:
   ```json
   "DefaultConnection": "Server=localhost,1433;Database=PlayballDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
   ```

### Build Issues

If you encounter build errors:
```bash
dotnet clean
dotnet restore Assignment_Example_HU.csproj
dotnet build Assignment_Example_HU.csproj
```

---

## ✅ Milestone 1 Checklist

- [x] Setup boilerplate with .NET
- [x] Run migrations (ready to apply when DB is available)
- [x] Implement authentication (Register/Login with token)
- [x] Implement role model and assignment (4 roles)
- [x] Add role-based authorization middleware
- [x] Swagger integration + JWT documentation
- [x] Proper project structure with clean architecture

---

## 🚀 Next Steps - Milestone 2

Milestone 2 will include:
1. Venue registration by owners
2. Admin approval workflow
3. Court/field creation per venue
4. Operating hours and base pricing
5. Discount model and APIs
6. Game management system
7. Game auto-cancel service

---

## 📞 Support

For issues or questions:
- Check the Swagger documentation at the app root
- Review error logs in the console
- Verify SQL Server is running and accessible

**Happy Coding! 🎾⚽🏀**
