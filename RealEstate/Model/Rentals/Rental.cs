using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace RealEstate.Model.Rentals
{
    public class Rental
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Description { get; set; }
        public int NumberOfRooms { get; set; }
        public List<string> Address = new List<string>();
        //private PostRental PostRental;
        [BsonRepresentation(BsonType.Double)]
        public decimal Price { get; set; }
        public string ImageId { get; set; }

        public List<PriceAdjustment> Adjustments = new List<PriceAdjustment>();

        public Rental()
        {
        }

        public Rental(PostRental postRental)
        {
            this.Description = postRental.Description;
            this.NumberOfRooms = postRental.NumberOfRooms;
            this.Price = postRental.Price;
            this.Address = (postRental.Address ?? string.Empty).Split('\n').ToList();
        }

        public void AdjustPrice(AdjustPrice adjustPrice)
        {
            var adjustment = new PriceAdjustment(adjustPrice,Price);
            Adjustments.Add(adjustment);
            Price = adjustPrice.NewPrice;
        }

        public bool HasImage()
        {
            return !String.IsNullOrWhiteSpace(ImageId);
        }

    }
}