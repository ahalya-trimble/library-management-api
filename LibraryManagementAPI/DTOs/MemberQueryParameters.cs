namespace LibraryManagementAPI.DTOs;

public class MemberQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? SortBy { get; set; } = "name";

    public string SortOrder { get; set; } = "asc";

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
