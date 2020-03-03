using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BA.WebAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeeklyReportController : AuthorizedControllerBase
    {
        private readonly BikingDbContext _context;

        private readonly IWeeklyReportService _service;

        private readonly ILogger<WeeklyReportController> _logger;

        public WeeklyReportController(BikingDbContext context, IWeeklyReportService service, ILogger<WeeklyReportController> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }

        [HttpGet("{date}")]
        public async Task<ActionResult<WeeklyReport>> GetReport(DateTime date)
        {
            WeeklyReport report = await 
                _context.WeeklyReports
                .Where(wr => wr.UserId == GetUserId() && wr.Date <= date).OrderBy(wr => wr.Date).LastOrDefaultAsync();

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        [HttpPost()]
        public async Task<ActionResult<int>> PostReportsForUser(string userId)
        {
            if (!IsAdminRole())
                return Unauthorized();

            foreach (long id in await _context.WeeklyReports.Where(wr => wr.UserId == userId).Select(wr => wr.Id).ToListAsync())
            {
                _context.WeeklyReports.Remove(new WeeklyReport { Id = id } );
            }
            await _context.SaveChangesAsync();

            await _service.GenerateWeeklyReports(userId);

            int createdNumber = await _context.WeeklyReports.Where(wr => wr.UserId == userId).CountAsync();

            return  createdNumber;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeeklyReport>>> GetReport(
            int? page = null,
            int? pageSize = null,
            string filter = null)
        {
            IQueryable<WeeklyReport> entries =
                _context.WeeklyReports
                .Where(wr => wr.UserId == GetUserId())
                .OrderBy(wr => wr.Date);

            entries = await TryAddFilter(entries, filter);
            entries = TryAddPagination(entries, page, pageSize);

            List<WeeklyReport> reports = await entries.ToListAsync();

            return reports;
        }

        private async Task<IQueryable<WeeklyReport>> TryAddFilter(
            IQueryable<WeeklyReport> query, 
            string filter)
        {
            if (HasFilter(filter))
            {
                return await CompileAndAddPredicate(query, filter);
            }
            return query;
        }

        private async Task<IQueryable<WeeklyReport>> CompileAndAddPredicate(
            IQueryable<WeeklyReport> query,
            string filter)
        {
            var converter  = new QueryFilterToPredicateExpressionCompiler<WeeklyReport>(filter, _logger);
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
