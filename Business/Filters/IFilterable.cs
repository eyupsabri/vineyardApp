namespace Business.Filters
{
    public interface IFilterable<T> where T : class
    {
        IQueryable<T> Filter(IQueryable<T> list);

    }
}
