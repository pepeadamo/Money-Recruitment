using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VacationRental.Api.Data;
using VacationRental.Api.Services;
using Moq;
using VacationRental.Api.Helpers;
using VacationRental.Api.Models;
using Xunit;

namespace VacationRental.Api.Tests.Services
{
    public class RentalsServiceTests
    {
        private Mock<IRentalsRepository> _mockRentalRepository;
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
        public void CreateRental_UnitIsNotGreaterThanZero_ReturnsBadRequest()
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
        public void CreateRental_PreparationTimeInDays_ReturnsBadRequest()
        {
            // Arrange
            RentalBindingModel model = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 0
            };
            
            // Act
            ServiceResponseViewModel response = _service.CreateRental(model);

            // Assert
            Assert.True(response.HttpCodeResponse == HttpStatusCode.BadRequest);
            Assert.Contains("The preparation time in days needs to be higher than 0", response.Response.ToString());
        }
        
        [Fact]
        public void CreateRental_ValidInput_ReturnsCreatedResponse()
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
    }
}