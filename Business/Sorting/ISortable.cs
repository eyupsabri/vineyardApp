namespace Business.Sorting
{
    public interface ISortable<T> where T : class
    {
        IQueryable<T> Sort(IQueryable<T> list);

    }
}
