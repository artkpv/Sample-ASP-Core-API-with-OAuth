
using System;
using System.Threading.Tasks;
using Xunit;
using BA.WebAPI.Model;
using System.Linq;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

namespace BA.WebAPI.UnitTests
{
    public class WeeklyReportServiceFacts
    {
        private ITestOutputHelper testHelper;

        private BikingDbHelper db;

        public WeeklyReportServiceFacts(ITestOutputHelper helper)
        {
            this.testHelper = helper;
            this.db = new BikingDbHelper(helper);
        }

        [Fact]
        public async Task GeneratesOneWeeklyReports()
        {
            string userId = "user1";
            await AssertWeeklyReports(userId,
                new [] { "2020-02-01 3600s 12000m" },
                new [] { "2020-01-27 12000m 300mpkm" });
        }

        [Fact]
        public async Task GeneratesManyWeeklyReports()
        {
            string userId = "user1";
            await AssertWeeklyReports(userId,
                new [] { 
                "2020-01-29 600s 2000m",
                "2020-01-30 600s 2000m",
                "2020-01-31 600s 2000m",
                "2020-02-01 600s 2000m",
                // New week:
                "2020-02-03 3600s 12000m",
                "2020-02-04 3600s 12000m",
                "2020-02-05 3600s 12000m",
                "2020-02-06 3600s 12000m",
                },
                new [] { 
                "2020-01-27 2000m 300mpkm",
                "2020-02-03 12000m 300mpkm" 
                });
        }

        [Fact]
        public async Task RemovesPreviousReports()
        {
            string userId = "user1";

            using (BikingDbContext context = db.GetTestDb())
            {
                WeeklyReport wr 
                    = ParseTestReport("2020-02-03 0m 0mpkm", userId);
                context.WeeklyReports.Add(wr);
                context.SaveChanges();
            }

            await AssertWeeklyReports(userId,
                new [] { 
                "2020-02-03 3600s 12000m",
                "2020-02-04 3600s 12000m",
                "2020-02-05 3600s 12000m",
                "2020-02-06 3600s 12000m",
                },
                new [] { "2020-02-03 12000m 300mpkm" });
        }

        private async Task AssertWeeklyReports(
            string userId,
            string[] runs,
            string[] reports)
        {
            AddBikingEntries(userId, runs);

            await RunAction(async (service) =>
            {
                await service.GenerateWeeklyReports(userId);
            });

            ParseAndAssertReports(userId, reports);
        }

        private void ParseAndAssertReports(string userId, string[] reports)
        {
            using (BikingDbContext context = db.GetTestDb())
            {
                List<WeeklyReport> found = context.WeeklyReports.Where(wr => wr.UserId == userId).OrderBy(wr => wr.Date).ToList();

                IEnumerable<WeeklyReport> expected
                    = reports.Select(r => ParseTestReport(r, userId))
                    .OrderBy(wr => wr.Date).ToList();
                Assert.Equal(expected.Count(), found.Count());
                Assert.Equal(expected, found);
            }
        }

        private void AddBikingEntries(string userId, string[] runs)
        {
            using (BikingDbContext context = db.GetTestDb())
            {
                foreach (string run in runs)
                {
                    BikingEntry re = ParseBikingEntry(userId, run);
                    context.BikingEntries.Add(re);
                }
                context.SaveChanges();
            }
        }

        private WeeklyReport ParseTestReport(string report, string userId)
        {
            Match m = Regex.Match(report,
                @"(\d+-\d+-\d+)\s+(\d+)m\s+(\d+)mpkm");
            Trace.Assert(m.Success);

            var wr = WeeklyReport.FromUserMetersSecondsDate(
                userId,
                 uint.Parse(m.Groups[2].Value),
                 uint.Parse(m.Groups[3].Value),
                 DateTime.Parse(m.Groups[1].Value,
                     DateTimeFormatInfo.CurrentInfo,
                     DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));
            return wr;
        }

        private static BikingEntry ParseBikingEntry(string userId, string run)
        {
            Match m = Regex.Match(run,
                @"(\d+-\d+-\d+)\s+(\d+)s\s+(\d+)m");
            Trace.Assert(m.Success);

            BikingEntry re = BikingEntry.FromUserTimeSecondsMeters(
                userId,
                 DateTime.Parse(m.Groups[1].Value,
                     DateTimeFormatInfo.CurrentInfo,
                     DateTimeStyles.AssumeUniversal),
                 uint.Parse(m.Groups[2].Value),
                 uint.Parse(m.Groups[3].Value));
            return re;
        }

        private async Task RunAction(Func<IWeeklyReportService, Task> action)
        {
            using (BikingDbContext context = db.GetTestDb())
            {
                IWeeklyReportService service =
                    new WeeklyReportService(
                        context,
                        new XunitLogger<IWeeklyReportService>(testHelper));
                await action(service);
                context.SaveChanges();
            }
        }
    }
}
