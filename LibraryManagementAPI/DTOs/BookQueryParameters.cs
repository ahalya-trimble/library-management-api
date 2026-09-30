namespace LibraryManagementAPI.DTOs;

public class BookQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public string? Author { get; set; }

    public bool? Available { get; set; }

    public string? SortBy { get; set; } = "title";

    public string SortOrder { get; set; } = "asc";

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
