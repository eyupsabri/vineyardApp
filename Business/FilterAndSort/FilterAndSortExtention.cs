namespace Business.FilterAndSort
{
    public static class FilterAndSortExtention
    {
        public static IQueryable<T> FilterAndSort<T>(this IQueryable<T> list, IFilterAndSort<T> filter) where T : class
        {
            var filtered = filter.Filter(list);
            return filter.Sort(filtered);
        }
    }
}
