using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Data;
using Assignment_Example_HU.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Assignment_Example_HU.Services;

public class RefundProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefundProcessorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(30); // Run every 30 minutes

    public RefundProcessorService(
        IServiceProvider serviceProvider,
        ILogger<RefundProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RefundProcessorService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRefundsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefundProcessorService");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("RefundProcessorService stopped");
    }

    private async Task ProcessRefundsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var courtRepository = scope.ServiceProvider.GetRequiredService<IRepository<Court>>();
        var walletRepository = scope.ServiceProvider.GetRequiredService<IRepository<Wallet>>();
        var transactionRepository = scope.ServiceProvider.GetRequiredService<IRepository<Transaction>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Find all bookings for deactivated courts that need full refund
        var courts = await courtRepository.FindAsync(c => c.IsActive == false);

        foreach (var court in courts)
        {
            // Get confirmed bookings for this deactivated court that haven't started yet
            var affectedBookings = await bookingRepository.FindAsync(b =>
                b.CourtId == court.CourtId &&
                b.Status == BookingStatus.Confirmed &&
                b.SlotStartTime > DateTime.UtcNow);

            foreach (var booking in affectedBookings)
            {
                try
                {
                    await using var transaction = await context.Database.BeginTransactionAsync();

                    // Get user wallet
                    var wallets = await walletRepository.FindAsync(w => w.UserId == booking.UserId);
                    var userWallet = wallets.FirstOrDefault();

                    if (userWallet != null && booking.AmountPaid > 0)
                    {
                        // Issue 100% refund (court deactivated by owner)
                        var refundAmount = booking.AmountPaid;

                        userWallet.Balance += refundAmount;
                        await walletRepository.UpdateAsync(userWallet);

                        // Create transaction record
                        var walletTransaction = new Transaction
                        {
                            WalletId = userWallet.WalletId,
                            Type = TransactionType.Credit,
                            Amount = refundAmount,
                            BalanceAfter = userWallet.Balance,
                            Description = $"Full refund - Court #{court.CourtId} deactivated by owner",
                            BookingId = booking.BookingId,
                            ReferenceId = $"refund_court_deactivated_{booking.BookingId}_{DateTime.UtcNow.Ticks}",
                            CreatedAt = DateTime.UtcNow
                        };
                        await transactionRepository.AddAsync(walletTransaction);

                        // Cancel booking
                        booking.Status = BookingStatus.Cancelled;
                        booking.CancellationReason = $"Court deactivated by venue owner - Full refund issued";
                        booking.CancelledAt = DateTime.UtcNow;
                        await bookingRepository.UpdateAsync(booking);

                        await bookingRepository.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Processed full refund for booking {booking.BookingId} due to court deactivation");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing refund for booking {booking.BookingId}");
                }
            }
        }
    }
}
