using System;
using System.Threading.Tasks;
using Xunit;
using BA.WebAPI.Controllers;
using BA.WebAPI.Model;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BA.WebAPI.UnitTests
{
    public class QueryFilterToPredicateExpressionCompilerFacts
    {
        private ITestOutputHelper helper;
        private XunitLogger<IBikingEntryService> logger;
        public QueryFilterToPredicateExpressionCompilerFacts(ITestOutputHelper helper)
        {
            this.helper = helper;
            this.logger = new XunitLogger<IBikingEntryService>(helper);
        }

        [Fact]
        public void FilterIsValid()
        {
            AssertValid("distanceMeters eq 100");
            AssertValid("distanceMeters ne 100");
            AssertValid("distanceMeters gt 100");
            AssertValid("distanceMeters lt 100");
            AssertValid("(distanceMeters lt 100) and (distanceMeters gt 0)");
            AssertValid("(distanceMeters lt 100) or (distanceMeters gt 0)");
            AssertValid("distanceMeters     eq     100");
            AssertValid("    distanceMeters     eq     100");
            AssertValid("(distanceMeters     lt 100) and      (distanceMeters gt 0)");
            AssertValid("startTime lt '2020-10-10'");
            AssertValid("startTime gt '2020-10-01'");
            AssertValid("startTime gt \"2020-10-01\"");
            AssertValid("startTime eq \"2020-10-01\"");
            AssertValid("Latitude eq 30.30");  // Should traverse to this property.
            AssertValid("Longitude eq 40.30");
        }

        [Fact]
        public void FilterIsNotValid()
        {
            AssertNotValid("");
            AssertNotValid("distanceMeters e_q 100");
            AssertNotValid("eq 100");
            AssertNotValid("unknownprop eq 100");
            AssertNotValid("distanceMeters ne");
            AssertNotValid("distanceMeters gst 100");
            AssertNotValid("distanceMeters ltlt 100");
            AssertNotValid("(distanceMeters lt 100 and (distanceMeters gt 0)");
            AssertNotValid("(distanceMeters lt 100) andand (distanceMeters gt 0)");
            AssertNotValid("(distanceMeters lt ) or (distanceMeters gt 0)");
        }

        private void AssertValid(string filter)
        {

            var c = new QueryFilterToPredicateExpressionCompiler<BikingEntry>(filter, logger);
            c.Compile().Wait();
            Assert.True(c.IsValid(), $"Failed for '{filter}': {c.GetPredicate()}");
            Assert.NotNull(c.GetPredicate());
        }

        private void AssertNotValid(string filter)
        {
            var c = new QueryFilterToPredicateExpressionCompiler<BikingEntry>(filter, logger);
            c.Compile().Wait();
            Assert.False(c.IsValid(), $"Failed for '{filter}': {c.GetPredicate()}");
        }

// - The API filtering should allow using parenthesis for defining operations precedence and use any combination of the available fields. The supported operations should at least include or, and, eq (equals), ne (not equals), gt (greater than), lt (lower than).
// - Example -> (date eq '2016-05-01') AND ((distance gt 20) OR (distance lt 10)).
    }
}
