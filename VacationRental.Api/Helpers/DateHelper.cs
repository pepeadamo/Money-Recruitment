using System;

namespace VacationRental.Api.Helpers
{
    public interface IDateHelper
    {
        bool IsOverlapping(DateTime startDateToBook, DateTime endDateToBook, DateTime startDateBooked, DateTime endDateBooked);
        bool IsDateBooked(DateTime dateToCheck, DateTime bookingStartDate, DateTime bookingEndDate);
        DateTime GetNowDate();
    }

    public class DateHelper : IDateHelper
    {
        public bool IsOverlapping(DateTime startDateToBook, DateTime endDateToBook, DateTime startDateBooked, DateTime endDateBooked)
        {
            return (startDateToBook.Date < endDateBooked.Date && endDateToBook.Date >= startDateBooked.Date);
        }

        public bool IsDateBooked(DateTime dateToCheck, DateTime bookingStartDate, DateTime bookingEndDate)
        {
            return bookingStartDate <= dateToCheck.Date && bookingEndDate > dateToCheck.Date;
        }
        
        public DateTime GetNowDate() => DateTime.Now.Date;
    }
}