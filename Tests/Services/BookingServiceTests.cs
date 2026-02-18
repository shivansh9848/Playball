using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Infrastructure.Data;
using Assignment_Example_HU.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace Playball.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IRepository<Court>> _mockCourtRepository;
        private readonly Mock<IRepository<Venue>> _mockVenueRepository;
        private readonly Mock<IRepository<Wallet>> _mockWalletRepository;
        private readonly Mock<IRepository<Transaction>> _mockTransactionRepository;
        private readonly Mock<IPricingService> _mockPricingService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockCourtRepository = new Mock<IRepository<Court>>();
            _mockVenueRepository = new Mock<IRepository<Venue>>();
            _mockWalletRepository = new Mock<IRepository<Wallet>>();
            _mockTransactionRepository = new Mock<IRepository<Transaction>>();
            _mockPricingService = new Mock<IPricingService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _mockCourtRepository.Object,
                _mockVenueRepository.Object,
                _mockWalletRepository.Object,
                _mockTransactionRepository.Object,
                _mockPricingService.Object,
                _mockCacheService.Object,
                _mockContext.Object
            );
        }

        [Fact]
        public async Task LockSlotAsync_ShouldLockSlot_WhenSlotIsAvailableAndWithinHours()
        {
            // Arrange
            var userId = 1;
            var courtId = 1;
            var slotStart = DateTime.UtcNow.AddHours(1);
            var slotEnd = slotStart.AddHours(1);
            var request = new LockSlotRequest
            {
                CourtId = courtId,
                SlotStartTime = slotStart,
                SlotEndTime = slotEnd
            };

            var court = new Court { CourtId = courtId, VenueId = 1, SlotDurationMinutes = 60, OpenTime = "06:00", CloseTime = "22:00", BasePrice = 100 };
            var pricing = new PricingBreakdown { FinalPrice = 150 };

            _mockCourtRepository.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _mockCacheService.Setup(c => c.SetIfNotExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>())).ReturnsAsync(true);
            _mockBookingRepository.Setup(r => r.IsSlotAvailableAsync(courtId, slotStart, slotEnd)).ReturnsAsync(true);
            _mockPricingService.Setup(p => p.GetPricingBreakdownAsync(courtId, slotStart, slotEnd)).ReturnsAsync(pricing);
            _mockBookingRepository.Setup(r => r.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.LockSlotAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Pending.ToString(), result.Status);
            Assert.Equal(pricing.FinalPrice, result.PriceLocked);
            _mockCacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once); // Lock should be removed after DB insert
        }

        [Fact]
        public async Task LockSlotAsync_ShouldThrow_WhenSlotAlreadyLockedInCache()
        {
             // Arrange
            var userId = 1;
            var courtId = 1;
            var slotStart = DateTime.UtcNow.AddHours(1);
            var request = new LockSlotRequest { CourtId = courtId, SlotStartTime = slotStart, SlotEndTime = slotStart.AddHours(1) };
            var court = new Court { CourtId = courtId, SlotDurationMinutes = 60, OpenTime = "00:00", CloseTime = "23:59" };

            _mockCourtRepository.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            // Simulate lock acquired by someone else
            _mockCacheService.Setup(c => c.SetIfNotExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>())).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() => _bookingService.LockSlotAsync(userId, request));
        }

        [Fact]
        public async Task ConfirmBookingAsync_ShouldConfirm_WhenBalanceIsSufficient()
        {
            // Arrange
            var userId = 1;
            var bookingId = 100;
            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = userId,
                CourtId = 1,
                Status = BookingStatus.Pending,
                PriceLocked = 500,
                SlotStartTime = DateTime.UtcNow.AddHours(2),
                SlotEndTime = DateTime.UtcNow.AddHours(3),
                LockExpiryTime = DateTime.UtcNow.AddMinutes(5)
            };

            var wallet = new Wallet { UserId = userId, Balance = 1000 };
            var mockTransaction = new Mock<IDbContextTransaction>();

            _mockBookingRepository.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
            // No overlapping bookings
            _mockBookingRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>())).ReturnsAsync(new List<Booking>()); 
            _mockWalletRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(new List<Wallet> { wallet });
            
            // Mock transaction
            _mockContext.Setup(c => c.Database.BeginTransactionAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(mockTransaction.Object);

            // Act
            var result = await _bookingService.ConfirmBookingAsync(userId, new ConfirmBookingRequest { BookingId = bookingId });

            // Assert
            Assert.Equal(BookingStatus.Confirmed.ToString(), result.Status);
            Assert.Equal(500, wallet.Balance); // Deducted
            _mockBookingRepository.Verify(r => r.UpdateAsync(booking), Times.Once);
            _mockTransactionRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
            mockTransaction.Verify(t => t.CommitAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmBookingAsync_ShouldThrow_WhenBalanceInsufficient()
        {
            // Arrange
            var userId = 1;
            var bookingId = 100;
            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = userId,
                Status = BookingStatus.Pending,
                PriceLocked = 500,
                LockExpiryTime = DateTime.UtcNow.AddMinutes(5)
            };
            var wallet = new Wallet { UserId = userId, Balance = 100 }; // Insufficient

            _mockBookingRepository.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
             _mockBookingRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>())).ReturnsAsync(new List<Booking>());
            _mockWalletRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(new List<Wallet> { wallet });

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() => _bookingService.ConfirmBookingAsync(userId, new ConfirmBookingRequest { BookingId = bookingId }));
        }

        [Fact]
        public async Task CancelBookingAsync_ShouldRefund_WhenWithinPolicy()
        {
            // Arrange
            var userId = 1;
            var bookingId = 100;
            var paidAmount = 1000m;
            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = userId,
                Status = BookingStatus.Confirmed,
                AmountPaid = paidAmount,
                SlotStartTime = DateTime.UtcNow.AddHours(48) // > 24h = 100% refund
            };
            var wallet = new Wallet { UserId = userId, Balance = 0 };

            _mockBookingRepository.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
            _mockWalletRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(new List<Wallet> { wallet });

            // Act
            var result = await _bookingService.CancelBookingAsync(userId, bookingId, "Changed mind");

            // Assert
            Assert.Equal(BookingStatus.Cancelled.ToString(), result.Status);
            Assert.Equal(paidAmount, wallet.Balance); // Full refund
            _mockTransactionRepository.Verify(t => t.AddAsync(It.IsAny<Transaction>()), Times.Once);
        }
    }
}
