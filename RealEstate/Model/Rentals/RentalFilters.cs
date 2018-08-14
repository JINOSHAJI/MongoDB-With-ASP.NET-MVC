using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.Model.Rentals
{
    public class RentalFilters
    {
        public decimal? PriceLimit { get; set; }
        public int? MinimumRoom { get; set; }
    }
}