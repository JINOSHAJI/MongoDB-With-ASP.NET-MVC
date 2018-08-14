using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using RealEstate.Properties;
using RealEstate.Model.Rentals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.App_Start
{
    public class RealEstateContext
    {
        public IMongoDatabase Database;
        public GridFSBucket ImagesBucket { get; set; }

        public RealEstateContext()
        {
            var connectionString = Settings.Default.RealEstateConnectionString;
            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

            var client = new MongoClient(settings);
           
            Database = client.GetDatabase(Settings.Default.RealEstateDatabaseName);
            ImagesBucket = new GridFSBucket(Database);

        }

        public IMongoCollection<Rental> Rentals => Database.GetCollection<Rental>("rentals");
    }
}