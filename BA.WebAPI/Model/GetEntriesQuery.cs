namespace BA.WebAPI.Model
{
    public class GetEntriesQuery
    {
        private static readonly int DefaultPageSize = 10;

        private static readonly int DefaultPageNumber = 1;

        public string UserId { get; set; }

        public int? Page { get; set; } = DefaultPageNumber;

        public int? PageSize { get; set; } = DefaultPageSize;

        public string Filter { get; set; }
    }
}
