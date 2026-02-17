using Assignment_Example_HU.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Assignment_Example_HU.Services;

public class SlotLockExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public SlotLockExpiryService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                await bookingService.ExpirePendingBookingsAsync();
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
