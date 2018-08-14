using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Model.Rentals
{
    public class RentalsList
    {
        public IEnumerable<RentalViewModel> RentalsVM { get; set; }
        public IEnumerable<Rental> Rentals { get; set; }
        public RentalFilters Filters { get; set; }
    }

    public class RentalViewModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal Price { get; set; }
        public List<string> Address { get; set; }
    }
}