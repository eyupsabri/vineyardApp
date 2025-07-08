namespace Business.Filters
{
    public class BaseFilter
    {
        public string SortBy { get; set; } = "Id";
        public bool SortDescending { get; set; } = false;
    }
}
