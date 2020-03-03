using BA.WebAPI.Model;

namespace BA.WebAPI.Model
{
    public static class Mapping
    {
        public static void CopyToDbEntry(this BikingEntryDto dto, BikingEntry dbEntry)
        {
            dbEntry.StartTime = dto.StartTime?.ToUniversalTime() ?? null;
            dbEntry.DistanceMeters = dto.DistanceMeters;
            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
                dbEntry.Location =
                    Coordinates.FromLatLong(dto.Latitude.Value, dto.Longitude.Value);
            dbEntry.DurationSeconds = dto.DurationSeconds;
        }

        public static BikingEntryDto MakeDto(this BikingEntry dbEntry)
        {
            return new BikingEntryDto()
            {
                Id = dbEntry.Id,
                StartTime = dbEntry.StartTime,
                DistanceMeters = dbEntry.DistanceMeters,
                Latitude = dbEntry.Location?.Latitude,
                Longitude = dbEntry.Location?.Longitude,
                DurationSeconds = dbEntry.DurationSeconds,
                Weather = dbEntry.Weather
            };
        }
    }
}
