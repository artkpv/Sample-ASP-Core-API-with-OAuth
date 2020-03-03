
using System;
using System.Threading.Tasks;
using Xunit;
using BA.WebAPI.Controllers;
using BA.WebAPI.Model;
using System.Linq;
using Xunit.Abstractions;
using Moq;
using System.Collections.Generic;

namespace BA.WebAPI.UnitTests
{
    public class BikingEntryServiceFacts
    {
        private ITestOutputHelper testHelper;
        
        private BikingDbHelper db;

        public BikingEntryServiceFacts(ITestOutputHelper helper)
        {
            this.testHelper = helper;
            this.db = new BikingDbHelper(helper);
        }

        [Fact]
        public async Task UserGetsOnlyItsRecords()
        {
            string userId = "user1";
            db.AddEntries(new[] {
                new BikingEntry { UserId = userId },
                new BikingEntry { UserId = "user2" }
            });

            IEnumerable<BikingEntry> records = null;
            var query = new GetEntriesQuery { UserId = userId };
            await RunAction(async (service) => {
                records = await service.GetEntries(query);
            });

            Assert.True(records.Count() == 1);
            Assert.True(records.All(r => r.UserId == userId));
        }

        [Fact]
        public async Task RecordsUserId()
        {
            string userId = "UserId123";
            await RunAction(async (service) => {
                await service.RecordBikingEntry(userId, new BikingEntryDto());
            });

            using (BikingDbContext context = db.GetTestDb())
            {
                Assert.Equal(1, context.BikingEntries.Count());
                Assert.Equal(context.BikingEntries.Single().UserId, userId);
            }
        }


        [Fact]
        public async Task RecordsStartTimeIfNon()
        {
            await RunAction(async (service) => {
                await service.RecordBikingEntry("userId123", new BikingEntryDto());
            });

            using (BikingDbContext context = db.GetTestDb())
            {
                Assert.Equal(1, context.BikingEntries.Count());
                Assert.NotNull(context.BikingEntries.Single().StartTime);
            }
        }

        private async Task RunAction(Func<IBikingEntryService, Task> action)
        {
            using (BikingDbContext context = db.GetTestDb())
            {
                IBikingEntryService service = 
                    new BikingEntryService(
                        context,
                        new WeatherAPIAdapterMock(),
                        new XunitLogger<IBikingEntryService>(testHelper));
                await action(service);
                context.SaveChanges();
            }
        }
    }
}
