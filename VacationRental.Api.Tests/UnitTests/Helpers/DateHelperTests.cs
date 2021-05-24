using System;
using VacationRental.Api.Helpers;
using Xunit;

namespace VacationRental.Api.Tests.UnitTests.Helpers
{
    public class DateHelperTests
    {
        private readonly DateHelper _dateHelper;
        
        public DateHelperTests()
        {
            _dateHelper = new DateHelper();
        }
        
        [Theory]
        [InlineData("2021-06-01", "2021-06-03", false)]
        [InlineData("2021-06-23", "2021-06-30", false)]
        [InlineData("2021-06-20", "2021-06-23", false)] // The night of the new booking's start date is available.
        [InlineData("2021-06-05", "2021-06-07", false)] // This booking ends on the 7th making the unit available by the night of 7th.
        [InlineData("2021-06-05", "2021-06-12", true)]
        [InlineData("2021-06-10", "2021-06-15", true)]
        [InlineData("2021-06-18", "2021-06-22", true)]
        [InlineData("2021-06-10", "2021-06-20", true)]
        [InlineData("2021-06-07", "2021-06-12", true)]
        public void IsOverlapping_ValidDates_ReturnsBoolean(DateTime starDate, DateTime endDate, bool isOverlapping)
        {
            // Arrange
            DateTime bookedStartDate = new DateTime(2021, 06, 07);
            DateTime bookedEndDate = new DateTime(2021, 06, 20);
            
            // Act
            bool overlappingResult = _dateHelper.IsOverlapping(starDate, endDate, bookedStartDate, bookedEndDate);

            // Assert
            if (isOverlapping)
            {
                Assert.True(overlappingResult);
            }
            else
            {
                Assert.False(overlappingResult);
            }
        }
        
        [Theory]
        [InlineData("2021-06-07", true)]  // Can not be booked since the night of 7th is not available.
        [InlineData("2021-06-20", false)] // Can be booked since the night of 20th is available.
        [InlineData("2021-06-08", true)]
        [InlineData("2021-06-10", true)]
        [InlineData("2021-06-19", true)]
        [InlineData("2021-06-21", false)]
        public void IsDateBooked_ValidDates_ReturnsBoolean(DateTime date, bool isBooked)
        {
            // Arrange
            DateTime bookedStartDate = new DateTime(2021, 06, 07);
            DateTime bookedEndDate = new DateTime(2021, 06, 20);
            
            // Act
            bool bookedResult = _dateHelper.IsDateBooked(date, bookedStartDate, bookedEndDate);

            // Assert
            if (isBooked)
            {
                Assert.True(bookedResult);
            }
            else
            {
                Assert.False(bookedResult);
            }
        }
    }
}