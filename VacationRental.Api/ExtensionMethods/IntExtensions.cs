namespace VacationRental.Api.ExtensionMethods
{
    public static class IntExtensions
    {
        public static bool IsGreaterThanZero(this int value) => value > 0;
        public static bool IsGreaterOrEqualThanZero(this int value) => value >= 0;
    }
}