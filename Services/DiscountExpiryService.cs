using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Assignment_Example_HU.Services;

public class DiscountExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DiscountExpiryService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6); // Run every 6 hours

    public DiscountExpiryService(
        IServiceProvider serviceProvider,
        ILogger<DiscountExpiryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DiscountExpiryService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RemoveExpiredDiscountsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DiscountExpiryService");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("DiscountExpiryService stopped");
    }

    private async Task RemoveExpiredDiscountsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var discountRepository = scope.ServiceProvider.GetRequiredService<IRepository<Discount>>();

        // Find all expired discounts
        var expiredDiscounts = await discountRepository.FindAsync(d =>
            d.ValidTo < DateTime.UtcNow);

        if (expiredDiscounts.Any())
        {
            foreach (var discount in expiredDiscounts)
            {
                try
                {
                    await discountRepository.DeleteAsync(discount);
                    _logger.LogInformation($"Removed expired discount {discount.DiscountId} ('{discount.Scope}') - expired on {discount.ValidTo}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error removing expired discount {discount.DiscountId}");
                }
            }

            await discountRepository.SaveChangesAsync();
            _logger.LogInformation($"Removed {expiredDiscounts.Count()} expired discounts");
        }
    }
}
