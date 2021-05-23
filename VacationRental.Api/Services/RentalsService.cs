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
    public interface IRentalsService
    {
        ServiceResponseViewModel CreateRental(RentalBindingModel model);
        ServiceResponseViewModel ModifyRental(int rentalId, RentalBindingModel model);
        ServiceResponseViewModel GetRental(int rentalId);
        bool DoesRentalExists(int rentalId);
        RentalViewModel GetUnitsForRental(int modelRentalId);
    }
    
    public class RentalsService : IRentalsService
    {
        private readonly ILogger<RentalsService> _logger;
        private readonly IRentalsRepository _rentalRepository;
        private readonly IBookingsRepository _bookingRepository;
        private readonly IDateHelper _dateHelper;

        public RentalsService(ILogger<RentalsService> logger, IRentalsRepository rentalRepository, IBookingsRepository bookingRepository, IDateHelper dateHelper)
        {
            _logger = logger;
            _rentalRepository = rentalRepository;
            _bookingRepository = bookingRepository;
            _dateHelper = dateHelper;
        }

        public ServiceResponseViewModel CreateRental(RentalBindingModel model)
        {
            (bool isValid, ServiceResponseViewModel serviceModel) = ValidateModel(model);
            if (!isValid)
            {
                return serviceModel;
            }

            _logger.LogDebug($"{nameof(CreateRental)}: the model was successfully validated.");
            
            int resourceId = _rentalRepository.GetLastResourceId();

            RentalViewModel rental = new RentalViewModel
            {
                Id = resourceId,
                Units = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            };

            _rentalRepository.AddRental(rental);

            ServiceResponseViewModel serviceResponse = new ServiceResponseViewModel
            {
                HttpCodeResponse = HttpStatusCode.Created,
                Response = new ResourceIdViewModel { Id = resourceId }
            };
            
            _logger.LogDebug($"{nameof(CreateRental)}: the rental was successfully created.");
            
            return serviceResponse;
        }
        
        public ServiceResponseViewModel ModifyRental(int rentalId, RentalBindingModel model)
        {
            (bool isValid, ServiceResponseViewModel serviceModel) = ValidateModel(model);
            if (!isValid)
            {
                return serviceModel;
            }
            
            _logger.LogDebug($"{nameof(ModifyRental)}: the model was successfully validated.");
            
            bool rentalExists = DoesRentalExists(rentalId);
            if (!rentalExists)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.NotFound,
                    Response = "Rental not found"
                };
            }
            
            _logger.LogDebug($"{nameof(ModifyRental)}: the rental Id was successfully validated.");
            
            List<BookingViewModel> bookings = _bookingRepository.GetBookings(rentalId, _dateHelper.GetNowDate());
            RentalViewModel rental = _rentalRepository.GetRental(rentalId);

            bool preparationTimeIsConflictingWithBookings = DoesPreparationTimeConflictWithBookings(model, bookings, rental);
            if (preparationTimeIsConflictingWithBookings)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.Conflict,
                    Response = "The preparation time value can not be modified due to conflicts with existing bookings."
                };
            }

            bool numberOfUnitsIsConflictingWithBookings = DoesNumberOfUnitsConflictWithBookings(bookings, rental, model);
            if (numberOfUnitsIsConflictingWithBookings)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.Conflict,
                    Response = "The number of units can not be modified due to conflicts with existing bookings."
                };
            }
            
            _logger.LogDebug($"{nameof(ModifyRental)}: the rental does not cause conflicts with bookings.");
            
            rental.Units = model.Units;
            rental.PreparationTimeInDays = model.PreparationTimeInDays;

            ServiceResponseViewModel serviceResponse = new ServiceResponseViewModel
            {
                HttpCodeResponse = HttpStatusCode.NoContent,
                Response = null
            };
            
            _logger.LogDebug($"{nameof(ModifyRental)}: the rental was successfully modified.");
            
            return serviceResponse;
        }

        private bool DoesNumberOfUnitsConflictWithBookings(List<BookingViewModel> bookings, RentalViewModel rental, RentalBindingModel model)
        {
            if (model.Units >= rental.Units)
            {
                return false;
            }
            
            DateTime lastDateOfBooking = bookings
                .OrderByDescending(b => b.Start.AddDays(b.Nights + rental.PreparationTimeInDays.Value))
                .Select(b => b.Start.AddDays(b.Nights + rental.PreparationTimeInDays.Value))
                .First();

            DateTime firstDateOfBooking = bookings
                .OrderByDescending(b => b.Start)
                .Last().Start;
            
            for (DateTime date = firstDateOfBooking; date <= lastDateOfBooking; date = date.AddDays(1))
            {
                List<BookingViewModel> bookingsForDate = bookings
                    .Where(b => _dateHelper.IsDateBooked(date, b.Start, b.Start.AddDays(b.Nights + rental.PreparationTimeInDays.Value)))
                    .ToList();

                bool rentalIsFullyBooked = bookingsForDate.Count == rental.Units;
                if (rentalIsFullyBooked)
                {
                    return true;
                }
            }

            return false;
        }

        private bool DoesPreparationTimeConflictWithBookings(RentalBindingModel model, List<BookingViewModel> bookings, RentalViewModel rental)
        {
            foreach (BookingViewModel referenceBooking in bookings)
            {
                DateTime referenceStartDate = referenceBooking.Start;
                DateTime referenceEndDate = referenceBooking.Start.AddDays(referenceBooking.Nights + rental.PreparationTimeInDays.Value);
                
                List<BookingViewModel> possibleOverlappedBookings = bookings.Where(booking => _dateHelper.IsOverlapping(referenceStartDate,
                        referenceEndDate, booking.Start,
                        booking.Start.AddDays(booking.Nights + rental.PreparationTimeInDays.Value)))
                    .ToList();
            
                bool bookingIsOverlappedInUnit = possibleOverlappedBookings.Any(b => b.Id != referenceBooking.Id && b.Unit == referenceBooking.Unit );
                if (bookingIsOverlappedInUnit )
                {
                    return true;
                }
            }

            return false;
        }

        public ServiceResponseViewModel GetRental(int rentalId)
        {
            bool rentalIdIsValid = rentalId.IsGreaterThanZero();
            if (!rentalIdIsValid)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.BadRequest,
                    Response = "The rental Id is invalid."
                };
            }

            _logger.LogDebug($"{nameof(GetRental)}: the rental Id was successfully validated.");
            
            bool rentalExists = DoesRentalExists(rentalId);
            if (!rentalExists)
            {
                return new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.NotFound,
                    Response = "Rental not found"
                };
            }
            
            _logger.LogDebug($"{nameof(GetRental)}: the rental exists.");

            RentalViewModel rental = _rentalRepository.GetRental(rentalId);
            
            _logger.LogDebug($"{nameof(GetRental)}: the rental was successfully retrieved.");
            
            ServiceResponseViewModel serviceResponse = new ServiceResponseViewModel
            {
               HttpCodeResponse = HttpStatusCode.OK,
               Response = rental
            };
            
            return serviceResponse;
        }

        public bool DoesRentalExists(int rentalId)
        {
            bool rentalExists = _rentalRepository.DoesRentalExist(rentalId);
            return rentalExists;
        }

        public RentalViewModel GetUnitsForRental(int modelRentalId)
        {
            RentalViewModel unitsForRental = _rentalRepository.GetRental(modelRentalId);
            return unitsForRental;
        }
        
        private (bool isValid, ServiceResponseViewModel serviceModel) ValidateModel(RentalBindingModel model)
        {
            bool numberIsValid = model.Units.IsGreaterThanZero();
            if (!numberIsValid)
            {
                return (false, new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.BadRequest,
                    Response = "The rental units value needs to be higher than 0."
                });
            }

            bool preparationDaysIsValid = model.PreparationTimeInDays.Value.IsGreaterThanZero();
            if (!preparationDaysIsValid)
            {
                return (false, new ServiceResponseViewModel
                {
                    HttpCodeResponse = HttpStatusCode.BadRequest,
                    Response = "The preparation time in days needs to be higher than 0."
                });
            }

            return (true, null);
        }
    }
}