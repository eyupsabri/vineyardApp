using Business.FilterAndSort;
using Entities;

namespace Business.Filters
{
    public class PumpFilter : BaseFilter, IFilterAndSort<Pump>
    {
        public bool? ActualState { get; set; }
        public bool? NeedsAttention { get; set; }
        public IQueryable<Pump> Filter(IQueryable<Pump> list)
        {
            list = list.Where(p => p.IoTDevice != null);
            if (ActualState.HasValue)
            {
                list = list.Where(p => p.ActualState == ActualState.Value);
            }
            if (NeedsAttention.HasValue)
            {
                list = list.Where(p => p.NeedsAttention == NeedsAttention.Value);
            }
            return list;
        }

        public IQueryable<Pump> Sort(IQueryable<Pump> list)
        {
            switch (SortBy)
            {
                case "ActualState":
                    return list.OrderBy(p => p.ActualState);
                case "NeedsAttention":
                    return list.OrderBy(p => p.NeedsAttention);
                default:
                    return list.OrderBy(p => p.Id); // Default sorting by Id
            }
        }
    }
}
