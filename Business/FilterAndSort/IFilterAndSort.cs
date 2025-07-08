using Business.Filters;
using Business.Sorting;

namespace Business.FilterAndSort
{
    public interface IFilterAndSort<T> : IFilterable<T>, ISortable<T> where T : class
    {
    }
}
