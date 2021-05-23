using System.Collections.Generic;
using VacationRental.Api.Models;

namespace VacationRental.Api.Data
{
    public interface IRentalsRepository
    {
        int GetLastResourceId();
        void AddRental(RentalViewModel rental);
        RentalViewModel GetRental(int rentalId);
        bool DoesRentalExist(int rentalId);
    }
    
    public class RentalsRepository : IRentalsRepository
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;

        public RentalsRepository(IDictionary<int, RentalViewModel> rentals)
        {
            _rentals = rentals;
        }
        
        public int GetLastResourceId() => _rentals.Keys.Count + 1;
        
        public void AddRental(RentalViewModel rental) => _rentals.Add(rental.Id, rental);
        
        public RentalViewModel GetRental(int rentalId)
        {
            return _rentals[rentalId];
        }

        public bool DoesRentalExist(int rentalId)
        {
            return _rentals.ContainsKey(rentalId);
        }
    }
}