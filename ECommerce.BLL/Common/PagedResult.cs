namespace ECommerce.BLL.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 1;

    public int TotalItems { get; set; }

    public int TotalPages => TotalItems == 0
        ? 1
        : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}
