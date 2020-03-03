
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BA.WebAPI.Model
{
    [Owned]
    public class Coordinates 
    {
        public static Coordinates FromLatLong(double latitude, double longitude)
        {
            return new Coordinates
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        public static Coordinates Empty => new Coordinates() { Latitude = null, Longitude = null };

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
