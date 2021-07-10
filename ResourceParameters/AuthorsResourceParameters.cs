namespace CourseLibraryApi.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        private const int MaxPageSize = 20;

        private int _defaultPageSize = 10;

        public string MainCategory { get; set; }
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _defaultPageSize;
            set => _defaultPageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
