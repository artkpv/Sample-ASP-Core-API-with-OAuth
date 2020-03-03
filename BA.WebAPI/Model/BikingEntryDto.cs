using System;
using System.ComponentModel.DataAnnotations;

namespace BA.WebAPI.Model
{
    public class BikingEntryDto
    {
        public long Id { get; set; }

        public DateTimeOffset? StartTime { get; set; }

        public uint? DistanceMeters { get; set; }

        public uint? DurationSeconds { get; set; }

        [Range(-90.0, 90.0)]
        public double? Latitude { get; set; }

        [Range(-180.0, 180.0)]
        public double? Longitude { get; set; }

        public string Weather { get; set; }
    }
}
