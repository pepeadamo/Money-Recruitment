using System;

namespace VacationRental.Api.ExtensionMethods
{
    public static class DateExtensions
    {
        public static bool IsInTheFuture(this DateTime date) => date > DateTime.Now.Date;
    }
}