using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BA.WebAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BA.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikingEntryController : AuthorizedControllerBase
    {
        private readonly BikingDbContext _context;

        private readonly IBikingEntryService _service;

        private readonly ILogger<BikingEntryController> _logger;

        private bool IsAuthorized(BikingEntry dbEntry)
        {
            if (IsAdminRole())
                return true;
            if (IsEntryMine(dbEntry.UserId))
                return true;
            return false;
        }

        public BikingEntryController(BikingDbContext context, IBikingEntryService service, ILogger<BikingEntryController> logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }

        // GET: api/BikingEntry/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BikingEntryDto>> GetBikingEntry(long id)
        {
            var runEntry = await _context.BikingEntries.FindAsync(id);

            if (runEntry == null)
            {
                return NotFound();
            }

            return runEntry.MakeDto();
        }

        // PUT: api/BikingEntry/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBikingEntry(long id, BikingEntryDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            BikingEntry dbEntry = await _context.BikingEntries.FindAsync(id);

            if (dbEntry == null)
                return NotFound();

            if (!IsAuthorized(dbEntry))
                return Unauthorized();

            dto.CopyToDbEntry(dbEntry);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BikingEntryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BikingEntry
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<BikingEntry>> PostBikingEntry(BikingEntryDto dto)
        {
            long id = await _service.RecordBikingEntry(GetUserId(), dto);
            dto.Id = id;

            return CreatedAtAction(
                nameof(GetBikingEntry),
                new { id = id },
                dto);
        }

        // DELETE: api/BikingEntry/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BikingEntry>> DeleteBikingEntry(long id)
        {
            var runEntry = await _context.BikingEntries.FindAsync(id);
            if (runEntry == null)
            {
                return NotFound();
            }

            if (!IsAuthorized(runEntry))
                return Unauthorized();

            _context.BikingEntries.Remove(runEntry);
            await _context.SaveChangesAsync();

            return runEntry;
        }

        // GET: api/BikingEntry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BikingEntryDto>>> GetBikingEntries(
            int? page = null,
            int? pageSize = null,
            string filter = null)
        {
            var query = new GetEntriesQuery
            {
                UserId = GetUserId(),
                Page = page,
                PageSize = pageSize,
                Filter = filter
            };
            IEnumerable<BikingEntry> entries = await _service.GetEntries(query);

            List<BikingEntryDto> list = entries.Select(el => el.MakeDto()).ToList();
            return list;
        }

        private bool BikingEntryExists(long id)
        {
            return _context.BikingEntries.Any(e => e.Id == id);
        }
    }
}
