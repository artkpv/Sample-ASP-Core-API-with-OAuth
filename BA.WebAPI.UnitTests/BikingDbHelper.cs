
using System;
using BA.WebAPI.Model;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using System.Reflection;
using System.Collections.Generic;

namespace BA.WebAPI.UnitTests
{
    public class BikingDbHelper
    {
        private ITestOutputHelper testHelper;

        public BikingDbHelper(ITestOutputHelper helper)
        {
            this.testHelper = helper;
        }

        public BikingDbContext GetTestDb() 
        {
            var options = new DbContextOptionsBuilder<BikingDbContext>()
                .UseInMemoryDatabase(databaseName: GetTestName())
                .Options;
            return new BikingDbContext(options);
        }

        public void AddEntries(IEnumerable<BikingEntry> entries)
        {
            using (BikingDbContext context = GetTestDb())
            {
                foreach (BikingEntry re in entries)
                    context.BikingEntries.Add(re);
                context.SaveChanges();
            }
        }

        private string GetTestName()
        {
            Type type = testHelper.GetType();
            FieldInfo testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            ITest test = (ITest)testMember.GetValue(testHelper);
            return test.DisplayName;
        }
    }
}
