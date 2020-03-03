using System.Collections.Generic;
using System.Threading.Tasks;

namespace BA.WebAPI.Model
{
    public interface IBikingEntryService
    {
        Task<long> RecordBikingEntry(string userId, BikingEntryDto input);
        Task<IEnumerable<BikingEntry>> GetEntries(GetEntriesQuery query);
    }
}
