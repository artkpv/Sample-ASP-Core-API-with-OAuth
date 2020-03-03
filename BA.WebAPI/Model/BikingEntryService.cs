using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Model
{
    public class BikingEntryService : IBikingEntryService
    {
        private readonly BikingDbContext _context;

        private IWeatherAPIAdapter _weatherAPIAdapter;

        private readonly ILogger<IBikingEntryService> _logger;

        public BikingEntryService(
            BikingDbContext context, 
            IWeatherAPIAdapter weatherAPIAdapter,
            ILogger<IBikingEntryService> logger)
        {
            _context = context;
            _weatherAPIAdapter = weatherAPIAdapter;
            _logger = logger;
        }

        public async Task<long> RecordBikingEntry(string userId, BikingEntryDto input)
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(userId));
            if (input.StartTime == null)
                input.StartTime = DateTimeOffset.UtcNow;

            var dbEntry = new BikingEntry();
            dbEntry.UserId = userId;
            
            input.CopyToDbEntry(dbEntry);

            _context.BikingEntries.Add(dbEntry);
            await _context.SaveChangesAsync();
            Trace.Assert(dbEntry.Id != 0);

            // TODO. Warning. This slows the response as we wait for weather API to respond.
            // Consider returning immediately and updating the weather conditions in separate thread.
            // EF lifetime scope should be considered (it is AddScoped now).
            if (dbEntry.Location != Coordinates.Empty)
                await UpdateEntryWithWeathInfo(dbEntry.Id, dbEntry.Location);

            return dbEntry.Id;
        }

        private async Task UpdateEntryWithWeathInfo(long id, Coordinates coordinates)
        {
            Trace.Assert(coordinates != null);
            Trace.Assert(id != 0);
            _logger.LogTrace(" Issuing weather API request");
            string weatherConditionsJson = await _weatherAPIAdapter.GetConditionsAsync(coordinates);
            _logger.LogTrace(" Getting entry " + id);
            BikingEntry entry = _context.BikingEntries.Find(id);
            entry.Weather = weatherConditionsJson;  // Or parse JSON and store what is intersting.
            _logger.LogTrace("Got answer. Saving.");
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogTrace(" BikingEntry saved");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Failed to store weather: ${ex}");
            }
        }

        public async Task<IEnumerable<BikingEntry>> GetEntries(GetEntriesQuery query)
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(query.UserId));

            IQueryable<BikingEntry> entries =
                from re in _context.BikingEntries
                where re.UserId == query.UserId
                orderby re.StartTime
                select re;

            entries = await TryAddFilter(entries, query.Filter);
            entries = TryAddPagination(entries, query.Page, query.PageSize);

            return await entries.ToListAsync();
        }

        private async Task<IQueryable<BikingEntry>> TryAddFilter(
            IQueryable<BikingEntry> query, 
            string filter)
        {
            if (HasFilter(filter))
            {
                return await CompileAndAddPredicate(query, filter);
            }
            return query;
        }

        private async Task<IQueryable<BikingEntry>> CompileAndAddPredicate(
            IQueryable<BikingEntry> query,
            string filter)
        {
            _logger.LogDebug($"Filtering with '{filter}'");
            var converter  = new QueryFilterToPredicateExpressionCompiler<BikingEntry>(filter, _logger);
            await converter.Compile();
            if (!converter.IsValid())
            {
                _logger.LogError($"Invalid filter: '{filter}'");
                return query;
            }
            _logger.LogDebug($"Filter: converter {converter.GetPredicate()}");
            return query.Where(converter.GetPredicate());
        }

        private bool HasFilter(string filter)
            => !string.IsNullOrWhiteSpace(filter);

        private IQueryable<T> TryAddPagination<T>(IQueryable<T> query, int? page, int? pageSize)
        {
            if (IsValidPagination(page, pageSize))
            {
                int ps = pageSize.Value;
                int p = page.Value;
                return query.Skip(ps * (p - 1)).Take(ps);
            }
            return query;
        }

        private bool IsValidPagination(int? page, int? pageSize)
        {
            return page.HasValue && pageSize.HasValue &&
                1 <= page.Value && 0 <= pageSize.Value;
        }
    }
}

