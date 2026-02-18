using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Assignment_Example_HU.Infrastructure.Data;
using Assignment_Example_HU.Infrastructure.Repositories;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.API.Middleware;
using Assignment_Example_HU.Common.Extensions;
using Assignment_Example_HU.Common.Helpers;
using Assignment_Example_HU.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Serialize all UTC DateTime values as IST (UTC+5:30) in JSON responses.
        // Database continues to store UTC — only the API output is converted.
        opts.JsonSerializerOptions.Converters.Add(new IstDateTimeConverter());
        opts.JsonSerializerOptions.Converters.Add(new IstNullableDateTimeConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Override default model validation error response with clean ApiResponse format
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e =>
                        string.IsNullOrEmpty(e.ErrorMessage)
                            ? $"Invalid value provided for '{kvp.Key}'. Please check the type and format."
                            : e.ErrorMessage
                    ).ToArray()
                );

            var firstError = errors.Values.FirstOrDefault()?.FirstOrDefault()
                             ?? "One or more validation errors occurred.";

            var response = new Assignment_Example_HU.Application.DTOs.Response.ApiResponse<object>
            {
                Success = false,
                Message = firstError,
                Data = null,
                Errors = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Assignment_Example_HU")));

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Authorization
builder.Services.AddAuthorization();

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

// Register Caching Service
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Assignment_Example_HU.Infrastructure.Caching.ICacheService, Assignment_Example_HU.Infrastructure.Caching.MemoryCacheService>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<ICourtService, CourtService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IWaitlistService, WaitlistService>();
builder.Services.AddScoped<IPlayerProfileService, PlayerProfileService>();

// Register background services
builder.Services.AddHostedService<Assignment_Example_HU.Services.GameAutoCancelService>();
builder.Services.AddHostedService<Assignment_Example_HU.Services.SlotLockExpiryService>();
builder.Services.AddHostedService<Assignment_Example_HU.Services.RefundProcessorService>();
builder.Services.AddHostedService<Assignment_Example_HU.Services.DiscountExpiryService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Playball Sports Booking Platform API",
        Version = "v1",
        Description = "API for Sports Venue Discovery and Slot Booking System",
        Contact = new OpenApiContact
        {
            Name = "Playball Team",
            Email = "support@playball.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token below (without 'Bearer ' prefix - Swagger adds it automatically).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Playball API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Authentication & Authorization must be in this order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed default admin on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Assignment_Example_HU.Infrastructure.Data.ApplicationDbContext>();
    await Assignment_Example_HU.Infrastructure.Data.DbSeeder.SeedAdminAsync(db);
}

app.Run();

