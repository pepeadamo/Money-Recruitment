using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VacationRental.Api.Data;
using VacationRental.Api.ExtensionMethods;
using VacationRental.Api.Helpers;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public interface IBookingsService
    {
        ServiceResponseViewModel CreateBooking(BookingBindingModel model);
        ServiceResponseViewModel GetBooking(int bookingId);
        List<BookingViewModel> GetBookings(int rentalId, DateTime date);
        List<BookingViewModel> GetBookingsByRental(int rentalId);
    }
    
    public class BookingsService : IBookingsService
    {
        private readonly ILogger<RentalsService> _logger;
        private readonly IBookingsRepository _bookingsRepository;
        private readonly IRentalsService _rentalsService;
        private readonly IDateHelper _dateHelper;

        public BookingsService(ILogger<RentalsService> logger, IBookingsRepository bookingsRepository, IRentalsService rentalsService, IDateHelper dateHelper)
        {
            _logger = logger;
            _bookingsRepository = bookingsRepository;
            _rentalsService = rentalsService;
            _dateHelper = dateHelper;
        }

        public ServiceResponseViewModel CreateBooking(BookingBindingModel model)
        {
            (bool isValid, ServiceResponseViewModel response) = ValidateModel(model);
            if (!isValid)
            {
                return response;
            }

            _logger.LogDebug($"{nameof(CreateBooking)}: the model was successfully validated.");

            (bool bookingIsAllowed, int nextUnitAvailable) = CheckBookingAvailability(model);
            if (!bookingIsAllowed)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.Conflict,
                    Response = "The rental is fully booked for at least one day of the given booking period."
                };
            }
            
            int resourceId = _bookingsRepository.GetLastResourceId();

            BookingViewModel booking = new BookingViewModel
            {
                Id = resourceId,
                Nights = model.Nights,
                RentalId = model.RentalId,
                Start = model.Start.Date,
                Unit = nextUnitAvailable
            };

            _bookingsRepository.AddBooking(booking);

            ServiceResponseViewModel serviceResponse = new ServiceResponseViewModel
            {
                HttpCodeResponse = HttpStatusCode.Created,
                Response = new ResourceIdViewModel { Id = resourceId }
            };
            
            _logger.LogDebug($"{nameof(CreateBooking)}: the booking was successfully created.");
            
            return serviceResponse;
        }

        public ServiceResponseViewModel GetBooking(int bookingId)
        {
            bool bookingIdIsValid = bookingId.IsGreaterThanZero();
            if (!bookingIdIsValid)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.BadRequest,
                    Response = "The booking Id is invalid."
                };
            }

            _logger.LogDebug($"{nameof(GetBooking)}: the booking Id was successfully validated.");
            
            bool bookingExists = _bookingsRepository.DoesBookingExist(bookingId);
            if (!bookingExists)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.NotFound,
                    Response = "Booking not found"
                };
            }
            
            _logger.LogDebug($"{nameof(GetBooking)}: the booking exists.");

            BookingViewModel booking = _bookingsRepository.GetBooking(bookingId);
            
            _logger.LogDebug($"{nameof(GetBooking)}: the rental was successfully retrieved.");
            
            ServiceResponseViewModel serviceResponse = new ServiceResponseViewModel
            {
                HttpCodeResponse = HttpStatusCode.OK,
                Response = booking
            };
            
            return serviceResponse;
        }

        public List<BookingViewModel> GetBookings(int rentalId, DateTime date)
        {
            List<BookingViewModel> bookingsForRentals = _bookingsRepository.GetBookings(rentalId, date);
            return bookingsForRentals;
        }
        
        public List<BookingViewModel> GetBookingsByRental(int rentalId)
        {
            List<BookingViewModel> bookingsForRentals = _bookingsRepository.GetBookingsByRental(rentalId);
            return bookingsForRentals;
        }

        private (bool isValid, ServiceResponseViewModel response) ValidateModel(BookingBindingModel model)
        {
            bool nightsNumberIsValid = model.Nights.IsGreaterThanZero();
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

            bool rentalExists = _rentalsService.DoesRentalExists(model.RentalId);
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
            
            bool startDateIsInTheFuture = model.Start.IsInTheFuture();
            if (!startDateIsInTheFuture)
            {
                {
                    return (false, new ServiceResponseViewModel
                    {
                        HttpCodeResponse = HttpStatusCode.BadRequest,
                        Response = "Start date must be in the future."
                    });
                }
            }

            return (true, null);
        }
        
        private (bool isBookable, int nextUnitAvailable) CheckBookingAvailability(BookingBindingModel model)
        {
            DateTime endDateToBook = model.Start.AddDays(model.Nights);
            
            RentalViewModel rental = _rentalsService.GetUnitsForRental(model.RentalId);
            
            List<BookingViewModel> bookingsForRentals = GetBookings(model.RentalId, model.Start);
            
            List<BookingViewModel> unitsBookedForGivenDate = bookingsForRentals
                .Where(booking => _dateHelper.IsOverlapping(model.Start, endDateToBook, booking.Start, booking.Start.AddDays(booking.Nights + rental.PreparationTimeInDays.Value)))
                .ToList();
            
            _logger.LogDebug($"{nameof(CheckBookingAvailability)}: the following booking Ids {string.Join(", ", unitsBookedForGivenDate.Select(b => b.Id))} are overlapping with {unitsBookedForGivenDate.Count} units. The rental allows up to {rental.Units} units.");
            
            bool isBookable =  unitsBookedForGivenDate.Count < rental.Units;
            if (!isBookable)
            {
                return (isBookable: false, 0);
            }

            List<int> unitIdsBooked = unitsBookedForGivenDate.Select(b => b.Unit).ToList();
            int nextUnitAvailable = GetNextUnitAvailable(unitIdsBooked, rental.Units);
            
            return (isBookable: true, nextUnitAvailable);
        }

        private int GetNextUnitAvailable(List<int> unitIdsBooked, int rentalUnits)
        {
            int nextUnitId = 1;
            while (unitIdsBooked.Any(unitBookedId => unitBookedId == nextUnitId))
            {
                nextUnitId++;
            }

            return nextUnitId;
        }
    }
}