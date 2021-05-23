using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using VacationRental.Api.ExtensionMethods;
using VacationRental.Api.Helpers;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public interface ICalendarService
    {
        ServiceResponseViewModel GetCalendar(int rentalId, DateTime start, int nights);
    }
    
    public class CalendarService : ICalendarService
    {
        private readonly ILogger<RentalsService> _logger;
        private readonly IRentalsService _rentalsService;
        private readonly IBookingsService _bookingService;
        private readonly IDateHelper _dateHelper;

        public CalendarService(ILogger<RentalsService> logger, IRentalsService rentalsService, IBookingsService bookingService, IDateHelper dateHelper)
        {
            _logger = logger;
            _rentalsService = rentalsService;
            _bookingService = bookingService;
            _dateHelper = dateHelper;
        }

        public ServiceResponseViewModel GetCalendar(int rentalId, DateTime startDate, int numberOfNights)
        {
            (bool isValid, ServiceResponseViewModel response) = ValidateModel(rentalId, numberOfNights);
            if (!isValid)
            {
                return response;
            }

            _logger.LogDebug($"{nameof(GetCalendar)}: the request values were successfully validated.");
            
            CalendarViewModel calendar = GenerateCalendarResponse(rentalId, startDate, numberOfNights);
            
            ServiceResponseViewModel serviceResponse = new ServiceResponseViewModel
            {
                HttpCodeResponse = HttpStatusCode.OK,
                Response = calendar
            };

            return serviceResponse;
        }

        private (bool isValid, ServiceResponseViewModel response) ValidateModel(int rentalId, int numberOfNights)
        {
            bool nightsNumberIsValid = numberOfNights.IsGreaterThanZero();
            if (!nightsNumberIsValid)
            {
                {
                    return (false, new ServiceResponseViewModel
                    {
                        HttpCodeResponse = HttpStatusCode.BadRequest,
                        Response = "Nights must be positive"
                    });
                }
            }

            bool rentalExists = _rentalsService.DoesRentalExists(rentalId);
            if (!rentalExists)
            {
                {
                    return (false, new ServiceResponseViewModel
                    {
                        HttpCodeResponse = HttpStatusCode.NotFound,
                        Response = "Rental not found"
                    });

                }
            }

            return (true, null);
        }
        
        private CalendarViewModel GenerateCalendarResponse(int rentalId, DateTime startDate, int numberOfNights)
        {
            List<BookingViewModel> bookingsForRentals = _bookingService.GetBookingsByRental(rentalId);
            
            CalendarViewModel calendarDates = new CalendarViewModel 
            {
                RentalId = rentalId,
                Dates = new List<CalendarDateViewModel>() 
            };
            
            for (var night = 0; night < numberOfNights; night++)
            {
                CalendarDateViewModel calendarDate = new CalendarDateViewModel
                {
                    Date = startDate.AddDays(night),
                    Bookings = new List<CalendarBookingViewModel>()
                };
            
                foreach (BookingViewModel booking in bookingsForRentals)
                {
                    bool dateIsBooked = _dateHelper.IsDateBooked(calendarDate.Date, booking.Start, booking.Start.AddDays(booking.Nights));
                    if (dateIsBooked)
                    {
                        calendarDate.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id, Unit = booking.Unit});
                    }

                    RentalViewModel rental = _rentalsService.GetRental(booking.RentalId).Response as RentalViewModel;

                    if (calendarDate.Date >= booking.Start.AddDays(booking.Nights))
                    {
                        bool unitIsUnderPreparationTime = _dateHelper.IsDateBooked(calendarDate.Date, booking.Start, booking.Start.AddDays(booking.Nights + rental.PreparationTimeInDays.Value));
                        if (unitIsUnderPreparationTime)
                        {
                            calendarDate.PreparationTimes.Add(new PreparationTime { Unit = booking.Unit});
                        }
                    }

                }
            
                calendarDates.Dates.Add(calendarDate);
            }

            return calendarDates;
        }
    }
}