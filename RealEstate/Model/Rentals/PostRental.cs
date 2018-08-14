using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.Model.Rentals
{
    public class PostRental
    {
        public string Description { get; set; }
        [Display(Name = "Number Of Rooms")]
        public int NumberOfRooms { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
    }
}