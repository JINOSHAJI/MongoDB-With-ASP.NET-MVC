using MongoDB.Driver;
using RealEstate.App_Start;
using RealEstate.Model.Rentals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RealEstate.Controllers.WebApiControllers
{
    public class RentalsController : ApiController
    {
        public readonly RealEstateContext context = new RealEstateContext();

        public HttpResponseMessage Get()
        {
            var rentalDatas = context.Rentals.Find(FilterDefinition<Rental>.Empty).ToList();
            if (rentalDatas.Any())
                return Request.CreateResponse(HttpStatusCode.OK, rentalDatas);
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No rentals data found.");
        }
        

        // GET: api/Rentals/5
        public HttpResponseMessage Get(string id)
        {
            var rentalData = context.Rentals.Find(u=>u.Id ==id).FirstOrDefault();
            if (rentalData!=null)
                return Request.CreateResponse(HttpStatusCode.OK, rentalData);
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No rentals data found.");
        }

        


        // POST: api/Rentals
        public HttpResponseMessage Post(PostRental postRental)
        {
            var rental = new Rental(postRental);
            context.Rentals.InsertOne(rental);
            return Request.CreateResponse(HttpStatusCode.OK, rental);
        }

        // PUT: api/Rentals/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Rentals/5
        public void Delete(int id)
        {
        }
    }
}
