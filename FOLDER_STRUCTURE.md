# Sports Booking Platform - Folder Structure

This document explains the folder structure and organization of the Sports Booking Platform project.

## 📁 Project Structure

```
Playball/
│
├── 📂 Domain/                          # Core business logic & entities
│   ├── Entities/                       # Database entities/models
│   │   ├── User.cs
│   │   ├── Venue.cs
│   │   ├── Court.cs
│   │   ├── Booking.cs
│   │   ├── Slot.cs
│   │   ├── Game.cs
│   │   ├── Wallet.cs
│   │   ├── Transaction.cs
│   │   ├── Discount.cs
│   │   ├── Rating.cs
│   │   └── Waitlist.cs
│   │
│   ├── Enums/                          # Enumerations
│   │   ├── BookingStatus.cs
│   │   ├── ApprovalStatus.cs
│   │   ├── GameStatus.cs
│   │   ├── TransactionType.cs
│   │   ├── UserRole.cs
│   │   └── SportType.cs
│   │
│   └── Constants/                      # Application constants
│       ├── PricingConstants.cs
│       ├── RefundConstants.cs
│       └── SystemConstants.cs
│
├── 📂 Application/                     # Application layer (business logic)
│   ├── Services/                       # Service implementations
│   │   ├── AuthService.cs
│   │   ├── VenueService.cs
│   │   ├── BookingService.cs
│   │   ├── WalletService.cs
│   │   ├── GameService.cs
│   │   ├── PricingService.cs
│   │   ├── RatingService.cs
│   │   └── WaitlistService.cs
│   │
│   ├── Interfaces/                     # Service interfaces
│   │   ├── IAuthService.cs
│   │   ├── IVenueService.cs
│   │   ├── IBookingService.cs
│   │   ├── IWalletService.cs
│   │   ├── IGameService.cs
│   │   ├── IPricingService.cs
│   │   ├── IRatingService.cs
│   │   └── IWaitlistService.cs
│   │
│   ├── DTOs/                           # Data Transfer Objects
│   │   ├── Request/                    # Request DTOs
│   │   │   ├── RegisterUserRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   ├── CreateVenueRequest.cs
│   │   │   ├── CreateCourtRequest.cs
│   │   │   ├── LockSlotRequest.cs
│   │   │   ├── ConfirmBookingRequest.cs
│   │   │   ├── CreateGameRequest.cs
│   │   │   └── CreateRatingRequest.cs
│   │   │
│   │   └── Response/                   # Response DTOs
│   │       ├── AuthResponse.cs
│   │       ├── VenueResponse.cs
│   │       ├── SlotAvailabilityResponse.cs
│   │       ├── BookingResponse.cs
│   │       ├── WalletResponse.cs
│   │       └── GameResponse.cs
│   │
│   └── Validators/                     # FluentValidation validators
│       ├── CreateVenueValidator.cs
│       ├── CreateBookingValidator.cs
│       └── CreateGameValidator.cs
│
├── 📂 Infrastructure/                  # Infrastructure layer (data access)
│   ├── Data/                           # Database context & migrations
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   │
│   ├── Repositories/                   # Repository implementations
│   │   ├── IRepository.cs              # Generic repository interface
│   │   ├── Repository.cs               # Generic repository
│   │   ├── IVenueRepository.cs
│   │   ├── VenueRepository.cs
│   │   ├── IBookingRepository.cs
│   │   ├── BookingRepository.cs
│   │   ├── IWalletRepository.cs
│   │   ├── WalletRepository.cs
│   │   └── ... (other repositories)
│   │
│   └── Caching/                        # Caching implementations
│       ├── ICacheService.cs
│       ├── RedisCacheService.cs
│       └── MemoryCacheService.cs
│
├── 📂 API/                             # API/Presentation layer
│   ├── Middleware/                     # Custom middleware
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── AuthenticationMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   │
│   └── Filters/                        # Action filters
│       ├── ValidationFilter.cs
│       └── AuthorizationFilter.cs
│
├── 📂 Controllers/                     # API Controllers (existing)
│   ├── AuthController.cs
│   ├── VenuesController.cs
│   ├── CourtsController.cs
│   ├── BookingsController.cs
│   ├── SlotsController.cs
│   ├── WalletController.cs
│   ├── GamesController.cs
│   ├── RatingsController.cs
│   └── WaitlistController.cs
│
├── 📂 BackgroundServices/              # Background/hosted services
│   ├── SlotLockExpiryService.cs        # Expire slot locks after 5 mins
│   ├── GameAutoCancelService.cs        # Auto-cancel games if min players not met
│   ├── RefundProcessorService.cs       # Process pending refunds
│   ├── DiscountExpiryService.cs        # Expire outdated discounts
│   └── HistoricalDemandService.cs      # Compute historical popularity
│
├── 📂 Common/                          # Shared utilities
│   ├── Exceptions/                     # Custom exceptions
│   │   ├── BusinessException.cs
│   │   ├── NotFoundException.cs
│   │   ├── UnauthorizedException.cs
│   │   └── ValidationException.cs
│   │
│   ├── Extensions/                     # Extension methods
│   │   ├── ServiceCollectionExtensions.cs
│   │   ├── DateTimeExtensions.cs
│   │   └── EnumExtensions.cs
│   │
│   └── Helpers/                        # Helper classes
│       ├── JwtHelper.cs
│       ├── PasswordHasher.cs
│       └── IdempotencyHelper.cs
│
├── 📂 Tests/                           # Test projects
│   ├── UnitTests/                      # Unit tests
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Helpers/
│   │
│   └── IntegrationTests/               # Integration tests
│       ├── Controllers/
│       └── Repositories/
│
├── 📂 Properties/                      # Project properties (existing)
│   └── launchSettings.json
│
├── 📂 bin/                             # Build outputs (existing)
├── 📂 obj/                             # Build intermediates (existing)
│
├── Program.cs                          # Application entry point (existing)
├── appsettings.json                    # Configuration (existing)
├── appsettings.Development.json        # Dev configuration (existing)
├── Assignment_Example_HU.csproj        # Project file (existing)
└── dockerfile                          # Docker configuration (existing)
```

## 🎯 Layer Responsibilities

### **Domain Layer**
- Contains core business entities and value objects
- No dependencies on other layers
- Pure business logic and models

### **Application Layer**
- Contains business logic and use cases
- Depends only on Domain layer
- Defines interfaces for infrastructure
- DTOs for data transfer
- Service implementations

### **Infrastructure Layer**
- Implements interfaces defined in Application layer
- Database context and migrations
- Repository pattern implementations
- Caching, external services, file storage

### **API Layer**
- Controllers, middleware, filters
- Request/response handling
- Authentication & authorization
- Swagger/OpenAPI documentation

### **Background Services**
- Long-running background tasks
- Scheduled jobs
- Timer-based operations

### **Common Layer**
- Shared utilities across all layers
- Custom exceptions
- Extension methods
- Helper classes

## 📝 Naming Conventions

- **Entities**: PascalCase (e.g., `User.cs`, `Booking.cs`)
- **Interfaces**: Prefixed with `I` (e.g., `IVenueService.cs`)
- **Services**: Suffixed with `Service` (e.g., `BookingService.cs`)
- **DTOs**: Suffixed with purpose (e.g., `CreateVenueRequest.cs`, `VenueResponse.cs`)
- **Validators**: Suffixed with `Validator` (e.g., `CreateVenueValidator.cs`)
- **Repositories**: Suffixed with `Repository` (e.g., `VenueRepository.cs`)

## 🔄 Data Flow

```
Request → Controller → Service → Repository → Database
                ↓
              Cache
                ↓
         Background Service
```

## ⚠️ Important Notes

- **Do not delete existing files** unless explicitly replacing them
- Keep `Controllers/` folder at root as API may reference it
- Existing configuration files remain at root
- New controllers should go in `Controllers/` folder
- Migration files stay in `Infrastructure/Data/Migrations/`

## 🚀 Next Steps

1. Create domain entities based on specification
2. Set up DbContext and migrations
3. Implement repository pattern
4. Create service layer with business logic
5. Build controllers with proper validation
6. Add background services
7. Write unit and integration tests
