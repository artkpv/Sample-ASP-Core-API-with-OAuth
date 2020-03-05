using System;

namespace BA.WebAPI.Model
{
    public class BikingEntry
    {
        public long Id { get; set; }
        public DateTime? StartTime { get; set; }
        public uint? DistanceMeters { get; set; } 
        public uint? DurationSeconds { get; set; }
        public Coordinates Location { get; set; } = Coordinates.Empty;
        public string Weather { get; set; } 
        public string UserId { get; set; }

        public static BikingEntry FromUserTimeSecondsMeters(
            string userId,
            DateTime startTime,
            uint runSeconds,
            uint runMeters)
            => new BikingEntry() {
                StartTime = startTime,
                DurationSeconds = runSeconds,
                DistanceMeters = runMeters,
                UserId = userId 
            };
    
    }
}
