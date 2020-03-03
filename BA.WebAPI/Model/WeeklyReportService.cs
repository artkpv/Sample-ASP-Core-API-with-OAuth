using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Model
{
    public class WeeklyReportService : IWeeklyReportService
    {
        private readonly BikingDbContext _context;

        private readonly ILogger<IWeeklyReportService> _logger;

        // TODO. Warning. First week day from a user profile.
        DayOfWeek WeekStartDay = DayOfWeek.Monday;

        const int DaysInWeek = 7;

        const int Kilo = 1000;

        public WeeklyReportService(
            BikingDbContext context,
            ILogger<IWeeklyReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GenerateWeeklyReports(string userId)
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(userId));

            // TODO. Warning. Consider doing from last report if perfomance issues.
            DateTimeOffset? greaterThanDate = null;

            DateTimeOffset? weekStart = GetNextWeekStartDate(userId, greaterThanDate);

            while (weekStart != null)
            {
                DateTimeOffset weekEnd = weekStart.Value.AddDays(DaysInWeek);

                WeeklyReport wr =
                    GenerateWeeklyReport(userId, weekStart.Value, weekEnd);

                if (wr != null)
                {
                    RemoveSameReports(wr);
                    _context.WeeklyReports.Add(wr);
                }

                weekStart = GetNextWeekStartDate(userId, weekEnd);
            }

            await _context.SaveChangesAsync();
        }

        private void RemoveSameReports(WeeklyReport wr)
        {
            List<long> theSameIds = _context.WeeklyReports.Where(dbwr => dbwr.UserId == wr.UserId && dbwr.Date == wr.Date).Select(dbwr => dbwr.Id).ToList();

            foreach (long id in theSameIds)
            {
                _context.WeeklyReports.Remove(new WeeklyReport { Id = id });
            }
        }

        private WeeklyReport GenerateWeeklyReport(
            string userId,
            DateTimeOffset start,
            DateTimeOffset end)
        {
            Trace.Assert(start.AddDays(DaysInWeek) == end);

            Func<BikingEntry, bool> reFilter = re =>
                re.UserId == userId &&
                re.DistanceMeters != null &&
                re.DurationSeconds != null &&
                start <= re.StartTime && re.StartTime <= end;

            IEnumerable<BikingEntry> query = _context.BikingEntries.Where(reFilter);

            int number = query.Count();
            if (number == 0)
                return null;

            long distanceSum = query.Select(re => (long)re.DistanceMeters).Sum();

            double speedSum = query.Select(re =>
                CalcSpeed(re.DurationSeconds.Value, re.DistanceMeters.Value)
            ).Sum();

            WeeklyReport wr = WeeklyReport.FromUserMetersSecondsDate(
                userId,
                (uint)(distanceSum / number),
                (uint)(speedSum / number),
                start.DateTime);

            return wr;
        }

        private double CalcSpeed(uint seconds, uint meters)
            => seconds / ((double)meters / Kilo);

        private DateTimeOffset? GetNextWeekStartDate(
                string userId,
                DateTimeOffset? greaterThanDate)
        {
            DateTimeOffset? d = (from re in _context.BikingEntries
                                 where
                                     re.UserId == userId
                                     && (
                                         greaterThanDate == null
                                         || greaterThanDate.Value < re.StartTime
                                     )
                                 orderby re.StartTime
                                 select re.StartTime).FirstOrDefault();

            return d?.DateTime.StartOfWeek(WeekStartDay);
        }
    }
}

