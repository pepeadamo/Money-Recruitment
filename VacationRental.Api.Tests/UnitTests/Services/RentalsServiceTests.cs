using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VacationRental.Api.Data;
using VacationRental.Api.Helpers;
using VacationRental.Api.Models;
using VacationRental.Api.Services;
using Xunit;

namespace VacationRental.Api.Tests.UnitTests.Services
{
    public class RentalsServiceTests
    {
        private readonly Mock<IRentalsRepository> _mockRentalRepository;
        private readonly Mock<IBookingsRepository> _mockBookingRepository;
        private readonly Mock<IDateHelper> _dateHelper;
        private readonly RentalsService _service;

        public RentalsServiceTests()
        {
            ILogger<RentalsService> logger = new Logger<RentalsService>(new NullLoggerFactory());
            
            _mockRentalRepository = new Mock<IRentalsRepository>();
            _mockBookingRepository = new Mock<IBookingsRepository>();
            _dateHelper = new Mock<IDateHelper>();

            _service = new RentalsService(logger, _mockRentalRepository.Object,
                _mockBookingRepository.Object, _dateHelper.Object);
        }
        
        [Fact]
        public void CreateRental_WhenUnitIsNotGreaterThanZero_ReturnsBadRequest()
        {
            // Arrange
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 0
            };
            
            // Act
            ServiceResponseViewModel response = _service.CreateRental(model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.BadRequest);
            Assert.Contains("The rental units value needs to be higher than 0", response.Response.ToString());
        }
        
        [Fact]
        public void CreateRental_WhenPreparationTimeInDaysIsNegative_ReturnsBadRequest()
        {
            // Arrange
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = -1
            };
            
            // Act
            ServiceResponseViewModel response = _service.CreateRental(model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.BadRequest);
            Assert.Contains("The preparation time in days needs to be greater or equal than 0", response.Response.ToString());
        }
        
        [Fact]
        public void CreateRental_WhenValidInput_ReturnsCreatedResponse()
        {
            // Arrange
            int expectedRentalCreatedId = 1;
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 1
            };

            _mockRentalRepository
                .Setup(s => s.GetLastResourceId())
                .Returns(1);
            
            // Act
            ServiceResponseViewModel response = _service.CreateRental(model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.Created);
            Assert.True(((ResourceIdViewModel) response.Response).Id == expectedRentalCreatedId);
        }
        
        [Fact]
        public void GetRental_WhenRentalIdIsInvalid_ReturnsBadRequestResponse()
        {
            // Arrange
            int invalidRentalId = -1;

            // Act
            ServiceResponseViewModel response = _service.GetRental(invalidRentalId);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.BadRequest);
            Assert.Contains("The rental Id is invalid.", response.Response.ToString());
        }
        
        [Fact]
        public void GetRental_WhenRentalIdDoesNotExist_ReturnsNotFoundResponse()
        {
            // Arrange
            int rentalId = 5;

            _mockRentalRepository
                .Setup(s => s.DoesRentalExist(rentalId))
                .Returns(false);

            // Act
            ServiceResponseViewModel response = _service.GetRental(rentalId);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.NotFound);
            Assert.Contains("Rental not found", response.Response.ToString());
        }
        
        [Fact]
        public void GetRental_WhenRentalIdIsValidAndExist_ReturnsOkResponse()
        {
            // Arrange
            int validRentalId = 5;

            _mockRentalRepository
                .Setup(s => s.DoesRentalExist(validRentalId))
                .Returns(true);
            
            _mockRentalRepository
                .Setup(s => s.GetRental(validRentalId))
                .Returns(new RentalViewModel { Id = 5, PreparationTimeInDays = 1, Units = 1});

            // Act
            ServiceResponseViewModel response = _service.GetRental(validRentalId);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.OK);
            Assert.True(((RentalViewModel) response.Response).Id == validRentalId);
        }
        
        [Fact]
        public void ModifyRental_WhenUnitIsNotGreaterThanZero_ReturnsBadRequest()
        {
            // Arrange
            int rentalId = 1;
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 0
            };
            
            // Act
            ServiceResponseViewModel response = _service.ModifyRental(rentalId, model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.BadRequest);
            Assert.Contains("The rental units value needs to be higher than 0", response.Response.ToString());
        }
        
        [Fact]
        public void ModifyRental_WhenPreparationTimeInDaysIsNegative_ReturnsBadRequest()
        {
            // Arrange
            int rentalId = 1;
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = -1
            };
            
            // Act
            ServiceResponseViewModel response = _service.ModifyRental(rentalId, model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.BadRequest);
            Assert.Contains("The preparation time in days needs to be greater or equal than 0", response.Response.ToString());
        }
        
        [Fact]
        public void ModifyRental_WhenRentalIdDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            int rentalId = 100;
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 1
            };

            _mockRentalRepository
                .Setup(s => s.DoesRentalExist(rentalId))
                .Returns(false);
            
            // Act
            ServiceResponseViewModel response = _service.ModifyRental(rentalId, model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.NotFound);
            Assert.Contains("Rental not found", response.Response.ToString());
        }
        
        [Fact]
        public void ModifyRental_WhenPreparationTimeValueConflictsWithBookings_ReturnsBadRequest()
        {
            // Arrange
            int rentalId = 100;
            DateTime nowDate = DateTime.Now.Date;
            int yearInTheFuture = nowDate.AddYears(10).Year;
            
            RentalBindingModel updatedModel = new RentalBindingModel
            {
                Units = 2,
                PreparationTimeInDays = 3
            };

            RentalViewModel expectedRentalModel = new RentalViewModel
            {
                Id = 1,
                Units = 2,
                PreparationTimeInDays = 2
            };

            List<BookingViewModel> expectedBookings = new List<BookingViewModel>()
            {
                new BookingViewModel
                {
                    Id = 1, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 10), Unit = 1
                },
                new BookingViewModel
                {
                    Id = 2, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 11), Unit = 2
                },
                new BookingViewModel
                {
                    Id = 3, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 18), Unit = 1
                }
            };
            
            _dateHelper
                .Setup(h => h.GetNowDate())
                .Returns(nowDate);

            _dateHelper
                .Setup(h => h.IsOverlapping(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(true);

            _mockRentalRepository
                .Setup(s => s.DoesRentalExist(rentalId))
                .Returns(true);
            
            _mockRentalRepository
                .Setup(s => s.GetRental(rentalId))
                .Returns(expectedRentalModel);
            
            _mockBookingRepository
                .Setup(s => s.GetBookings(rentalId, nowDate))
                .Returns(expectedBookings);
            
            // Act
            ServiceResponseViewModel response = _service.ModifyRental(rentalId, updatedModel);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.Conflict);
            Assert.Contains("The preparation time value can not be modified due to conflicts with existing bookings", response.Response.ToString());
        }
        
        [Fact]
        public void ModifyRental_WhenNumberOfUnitsIsReducedAndConflictsWithBookings_ReturnsBadRequest()
        {
            // Arrange
            int rentalId = 100;
            DateTime nowDate = DateTime.Now.Date;
            int yearInTheFuture = nowDate.AddYears(10).Year;
            
            RentalBindingModel updatedModel = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 3
            };

            RentalViewModel expectedRentalModel = new RentalViewModel
            {
                Id = 1,
                Units = 2,
                PreparationTimeInDays = 3
            };

            List<BookingViewModel> expectedBookings = new List<BookingViewModel>()
            {
                new BookingViewModel
                {
                    Id = 1, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 10), Unit = 1
                },
                new BookingViewModel
                {
                    Id = 2, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 11), Unit = 2
                }
            };
            
            _dateHelper
                .Setup(h => h.GetNowDate())
                .Returns(nowDate);

            _dateHelper
                .Setup(h => h.IsDateBooked(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(true);

            _dateHelper
                .Setup(h => h.IsOverlapping(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(false);
            
            _mockRentalRepository
                .Setup(s => s.DoesRentalExist(rentalId))
                .Returns(true);
            
            _mockRentalRepository
                .Setup(s => s.GetRental(rentalId))
                .Returns(expectedRentalModel);
            
            _mockBookingRepository
                .Setup(s => s.GetBookings(rentalId, nowDate))
                .Returns(expectedBookings);
            
            // Act
            ServiceResponseViewModel response = _service.ModifyRental(rentalId, updatedModel);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.Conflict);
            Assert.Contains("The number of units can not be modified due to conflicts with existing bookings.", response.Response.ToString());
        }
        
        [Fact]
        public void ModifyRental_WhenNumberOfUnitsIsIncremented_RentalIsUpdatedAndReturnsNoContent()
        {
            // Arrange
            int rentalId = 100;
            DateTime nowDate = DateTime.Now.Date;
            int yearInTheFuture = nowDate.AddYears(10).Year;
            
            RentalBindingModel updatedModel = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 10
            };

            RentalViewModel expectedRentalModel = new RentalViewModel
            {
                Id = 1,
                Units = 2,
                PreparationTimeInDays = 3
            };

            List<BookingViewModel> expectedBookings = new List<BookingViewModel>()
            {
                new BookingViewModel
                {
                    Id = 1, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 10), Unit = 1
                },
                new BookingViewModel
                {
                    Id = 2, Nights = 5, RentalId = 1, Start = new DateTime(yearInTheFuture, 06, 11), Unit = 2
                }
            };
            
            _dateHelper
                .Setup(h => h.GetNowDate())
                .Returns(nowDate);

            _dateHelper
                .Setup(h => h.IsDateBooked(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(false);

            _dateHelper
                .Setup(h => h.IsOverlapping(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(false);
            
            _mockRentalRepository
                .Setup(s => s.DoesRentalExist(rentalId))
                .Returns(true);
            
            _mockRentalRepository
                .Setup(s => s.GetRental(rentalId))
                .Returns(expectedRentalModel);
            
            _mockBookingRepository
                .Setup(s => s.GetBookings(rentalId, nowDate))
                .Returns(expectedBookings);
            
            // Act
            ServiceResponseViewModel response = _service.ModifyRental(rentalId, updatedModel);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.NoContent);
        }
    }
}