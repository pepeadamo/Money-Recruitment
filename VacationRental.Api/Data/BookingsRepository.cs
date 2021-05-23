using System;
using System.Collections.Generic;
using System.Linq;
using VacationRental.Api.Models;

namespace VacationRental.Api.Data
{
    public interface IBookingsRepository
    {
        int GetLastResourceId();
        void AddBooking(BookingViewModel booking);
        BookingViewModel GetBooking(int bookingId);
        bool DoesBookingExist(int bookingId);
        List<BookingViewModel> GetBookings(int rentalId, DateTime date);
        List<BookingViewModel> GetBookingsByRental(int rentalId);
    }
    
    public class BookingsRepository : IBookingsRepository
    {
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BookingsRepository( IDictionary<int, BookingViewModel> bookings)
        {
            _bookings = bookings;
        }
        
        public int GetLastResourceId() => _bookings.Keys.Count + 1;
        
        public void AddBooking(BookingViewModel booking) => _bookings.Add(booking.Id, booking);

        public BookingViewModel GetBooking(int bookingId) => _bookings[bookingId];

        public bool DoesBookingExist(int bookingId) => _bookings.ContainsKey(bookingId);

        public List<BookingViewModel> GetBookings(int rentalId, DateTime date)
        {
            List<BookingViewModel> bookingsForRental = _bookings.Values
                .Where(booking => booking.RentalId == rentalId && booking.Start.AddDays(booking.Nights) > date)
                .ToList();
            
            return bookingsForRental;
        }

        public List<BookingViewModel> GetBookingsByRental(int rentalId)
        {
            List<BookingViewModel> bookingsForRental = _bookings.Values
                .Where(s => s.RentalId == rentalId)
                .ToList();
            
            return bookingsForRental;
        }
    }
}