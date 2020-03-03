using System;

namespace BA.WebAPI.Model
{
    public class WeeklyReport
    {
        // TODO. Warning. Key can be optimized to use UserId+Date composite key. 
        // Hence speeding up removal of the same reports.

        public long Id { get; set; }

        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public uint? AverageSpeedSecondsPerKM { get; set; }
        public uint? AverageDistanceMeters { get; set; }

        public override bool Equals(object obj)
        {
            return obj is WeeklyReport report &&
                   UserId == report.UserId &&
                   Date == report.Date &&
                   AverageSpeedSecondsPerKM == report.AverageSpeedSecondsPerKM &&
                   AverageDistanceMeters == report.AverageDistanceMeters;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, Date, AverageSpeedSecondsPerKM, AverageDistanceMeters);
        }

        public static WeeklyReport FromUserMetersSecondsDate(
            string userId, 
            uint meters,
            uint seconds,
            DateTime dateTime)
            => new WeeklyReport {
                UserId = userId,
                AverageDistanceMeters = meters,
                AverageSpeedSecondsPerKM = seconds,
                Date = dateTime
            };
    }
}
