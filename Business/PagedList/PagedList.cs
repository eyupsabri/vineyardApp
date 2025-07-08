namespace Business.PagedList
{
    public class PagedList<T> where T : class
    {
        public int PageIndex { get; private set; }
        public int PageCount { get; private set; }
        public List<T> FinalList { get; private set; }
        public PagedList(int pageIndex)
        {
            PageIndex = pageIndex;
            FinalList = new List<T>();
        }

        public void ToPagedList(IQueryable<T> list)
        {
            var numberOfElements = list.Count();
            PageCount = numberOfElements / 6 + 1;
            if (numberOfElements % 6 == 0)
            {
                PageCount = PageCount - 1;
            }
            FinalList = list.Skip(PageIndex * 6).Take(6).ToList();

        }
    }
}
