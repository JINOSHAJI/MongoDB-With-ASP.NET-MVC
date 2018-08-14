using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using RealEstate.App_Start;
using RealEstate.Model.Rentals;
namespace RealEstate.Controllers
{
     
    public class RentalsController : Controller
    {
        public readonly RealEstateContext context = new RealEstateContext();

        public ActionResult Index(RentalFilters rentalFilters)
        {
            var rentals = FilterRentals(rentalFilters);
            RentalsList rentalsList = new RentalsList
            {
                Rentals = rentals,
                Filters = rentalFilters
            };
            return View(rentalsList);
        }

        // filteration using linq
        private IMongoQueryable<Rental> FilterRentalsLINQ(RentalFilters filters)
        {
            var rentals = context.Rentals.AsQueryable();

            if (filters.MinimumRoom.HasValue)
            {
                rentals = rentals
                    .Where(r => r.NumberOfRooms >= filters.MinimumRoom);
            }

            if (filters.PriceLimit.HasValue)
            {
                rentals = rentals
                    .Where(r => r.Price <= filters.PriceLimit);
            }

            return rentals;
        }

        // filteration method to filter rentals
        private List<Rental> FilterRentals(RentalFilters rentalFilters)
        {
            if (rentalFilters.PriceLimit.HasValue && rentalFilters.MinimumRoom.HasValue)
            {
                var filter = Builders<Rental>.Filter.Where(u=>u.NumberOfRooms> rentalFilters.MinimumRoom && u.Price<= rentalFilters.PriceLimit);
                return context.Rentals.Find(filter).SortBy(u => u.Price).ToList();
            }

            if (rentalFilters.MinimumRoom.HasValue)
            {
                return context.Rentals.AsQueryable().Where(u => u.NumberOfRooms >= rentalFilters.MinimumRoom).OrderBy(u=>u.NumberOfRooms).ToList();
            }
            if (rentalFilters.PriceLimit.HasValue)
            {
                var filter = Builders<Rental>.Filter.Lte(s => s.Price, rentalFilters.PriceLimit);
                return context.Rentals.Find(filter).SortBy(u => u.Price).ToList();
            }
            return context.Rentals.Find(FilterDefinition<Rental>.Empty).SortBy(u => u.Price).ToList();
        }
        
        // GET: Rentals
        public ActionResult Post()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Post(PostRental postRental)
        {
            var rental = new Rental(postRental);
              context.Rentals.InsertOne(rental);
            return RedirectToAction("Index","Rentals");
        }


        public ActionResult AdjustPrice(string id)
        {
            var rental = GetRental(id);
            return View(rental);
        }
        public ActionResult IncrementPrice(string id)
        {
            var rental = GetRental(id);
            return View(rental);
        }
        
        [HttpPost]
        public async Task<ActionResult> IncrementPrice(string id, AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            adjustPrice.Reason = "Incremental Update : " + adjustPrice.NewPrice;
            var adjustment = new PriceAdjustment(adjustPrice, rental.Price);
            //var modificationUpdate = Builders<Rental>.Update
            //    .Push(r => r.Adjustments, adjustment)
            //    .Set(r => r.Price, adjustPrice.NewPrice);
            var modificationUpdate = Builders<Rental>.Update.Push(r => r.Adjustments, adjustment)
                .Inc("Price", adjustPrice.NewPrice);
            //Context.Rentals.Update(Query.EQ("_id", new ObjectId(id)), modificationUpdate);
            await context.Rentals.UpdateOneAsync(r => r.Id == id, modificationUpdate);
            return RedirectToAction("Index", "Rentals");
        }


        [HttpPost]
        public async Task<ActionResult> AdjustPrice_(string id,AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            rental.AdjustPrice(adjustPrice);
         //   var filter = Builders<Rental>.Filter.Eq(s => s.Id, id);
            //  var foundtable = context.Rentals.ReplaceOne(filter,rental);
            await context.Rentals.ReplaceOneAsync(r => r.Id == id, rental);

            return RedirectToAction("Index","Rentals");
        }

        public async Task<ActionResult> AdjustPrice_WithUpdateOptions(string id, AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            rental.AdjustPrice(adjustPrice); 

            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = true; // true if a new document should be inserted if there are no matches to the query filter
            updateOptions.BypassDocumentValidation = true; // If true, allows the write to opt-out of document level validation.
            await context.Rentals.ReplaceOneAsync(r => r.Id == id, rental, updateOptions);
            return RedirectToAction("Index", "Rentals");
        }

        [HttpPost]
        public async Task<ActionResult> AdjustPrice(string id, AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            var adjustment = new PriceAdjustment(adjustPrice, rental.Price);
            var modificationUpdate = Builders<Rental>.Update
                .Push(r => r.Adjustments, adjustment)
                .Set(r => r.Price, adjustPrice.NewPrice);
            //Context.Rentals.Update(Query.EQ("_id", new ObjectId(id)), modificationUpdate);
            await context.Rentals.UpdateOneAsync(r => r.Id == id, modificationUpdate);
            return RedirectToAction("Index", "Rentals");
        }
        
         
        [HttpPost]
        public ActionResult AdjustPriceUsingModifications(string id,AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            var adjustment = new PriceAdjustment(adjustPrice, rental.Price);
            //var modificationUpdate = Update<Rental>
            //    .Push(r => r.Adjustments, adjustment)
            //    .Set(r => r.Price, adjustment.NewPrice);
            var filter = Builders<Rental>.Filter.Eq(s => s.Id, id);
            var updateDefinition = Builders<Rental>.Update.Push(r => r.Adjustments, adjustment)
                .Set(r => r.Price, adjustment.NewPrice);
            context.Rentals.UpdateOne(filter, updateDefinition);
            return RedirectToAction("Index","Rentals");
        }

       
        public ActionResult Delete(string id)
        {
            var filter = Builders<Rental>.Filter.Eq(s => s.Id, id);
            context.Rentals.DeleteOne(filter);
            return RedirectToAction("Index","Rentals");
        }

        public ActionResult QueryRental(string id)
        {
            return Json(Query<Rental>.GT(p => p.Price, Convert.ToDecimal(id)).ToJson(),JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public string PriceDistribution()
        {
            return new QueryPriceDistribution()
                .RunAggregationFluent(context.Rentals)
                .ToJson();
        }


        #region Images GridFs

        public ActionResult AttachImage(string id)
        {
            var rental = GetRental(id);
            return View(rental);
        }

        [HttpPost]
        public async Task<ActionResult> AttachImage(string id, HttpPostedFileBase file)
        {
            var rental = GetRental(id);
            if (rental.HasImage())
            {
                await DeleteImageAsync(rental);
            }
            await StoreImageAsync(file, id);
            return RedirectToAction("Index");
        }

        private async Task DeleteImageAsync(Rental rental)
        {
            await context.ImagesBucket.DeleteAsync(new ObjectId(rental.ImageId));
            await SetRentalImageIdAsync(rental.Id, null);
        }

        private async Task StoreImageAsync(HttpPostedFileBase file, string rentalId)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument("contentType", file.ContentType)
            };
            var imageId = await context.ImagesBucket
                .UploadFromStreamAsync(file.FileName, file.InputStream, options);
            await SetRentalImageIdAsync(rentalId, imageId.ToString());
        }

        private async Task SetRentalImageIdAsync(string rentalId, string imageId)
        {
            var setRentalImageId = Builders<Rental>.Update.Set(r => r.ImageId, imageId);
            await context.Rentals.UpdateOneAsync(r => r.Id == rentalId, setRentalImageId);
        }

        public ActionResult GetImage(string id)
        {
            try
            {
                var stream = context.ImagesBucket.OpenDownloadStream(new ObjectId(id));
                var contentType = stream.FileInfo.ContentType
                    ?? stream.FileInfo.Metadata["contentType"].AsString;
                return File(stream, contentType);
            }
            catch (GridFSFileNotFoundException)
            {
                return HttpNotFound();
            }
        }


        #endregion


        public async Task<ActionResult> IndexProjection(RentalFilters filters)
        {
            var rentals = await FilterRentalsLINQ(filters)
                .Select(r => new RentalViewModel
                {
                    Id = r.Id,
                    Address = r.Address,
                    Description = r.Description,
                    NumberOfRooms = r.NumberOfRooms,
                    Price = r.Price
                })
                .OrderBy(r => r.Price)
                .ThenByDescending(r => r.NumberOfRooms)
                .ToListAsync();

            var model = new RentalsList
            {
                RentalsVM = rentals,
                Filters = filters
            };
            return View("Index", model);
        }


        public Rental GetRental(string id)
        {
            // var rental = context.Rentals.Find(_ => _.Id == id).Single();
            //var filter = Builders<Rental>.Filter.Eq(s => s.Id, id);
            //var rental = context.Rentals.Find(filter).Single();
            var rental = context.Rentals
                .Find(r => r.Id == id)
                .FirstOrDefault();
            return rental;
        }
    }
}