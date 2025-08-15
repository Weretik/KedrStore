namespace Presentation.Shared.States;

public class CatalogState : IState
{
    private bool _suppressNotify;
    private int _pageNumber = 1;
    private int _pageSize = 12;
    private int? _categoryId;
    private string? _searchTerm;
    private string? _sortBy;
    private AppSortDirection _sortDirection = AppSortDirection.Asc;
    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (_pageNumber == value) return;
            _pageNumber = value <= 0 ? 1 : value;
            NotifyChanged();
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if(_pageSize == value) return;
            _pageSize = value < 12 ? 12 : value;
            NotifyChanged();
        }
    }

    public int? CategoryId
    {
        get => _categoryId;
        set
        {
            if (_categoryId == value) return;
            _categoryId = value;
            _pageNumber = 1;
            NotifyChanged();
        }
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm == value) return;
            if (string.IsNullOrWhiteSpace(value)) _searchTerm = null;
            else _searchTerm = value.Trim();
            _pageNumber = 1;
            NotifyChanged();
        }
    }

    public string? SortBy
    {
        get => _sortBy;
        set
        {
            if(_sortBy == value) return;
            if (string.IsNullOrWhiteSpace(value)) _sortBy = null;
            else _sortBy = value.Trim();
            _pageNumber = 1;
            NotifyChanged();
        }
    }

    public AppSortDirection SortDirection
    {
        get => _sortDirection;
        set
        {
            if (_sortDirection == value) return;
            _sortDirection = value == AppSortDirection.Desc ? value : AppSortDirection.Asc;
            _pageNumber = 1;
            NotifyChanged();
        }
    }

    public void SetPageNumber(int pageNumber) => PageNumber = pageNumber;
    public void SetPageSize(int pageSize) => PageSize = pageSize;
    public void SetCategoryId(int? categoryId) => CategoryId = categoryId;
    public void SetSearchTerm(string? searchTerm) => SearchTerm = searchTerm;
    public void SetSortBy(string? sortBy) => SortBy = sortBy;
    public void SetSortDirection(AppSortDirection sortDirection) => SortDirection = sortDirection;

    public event Action? OnChange;
    private void NotifyChanged()
    {
        if (!_suppressNotify)
        {
            OnChange?.Invoke();
        }
    }

    public void Reset()
    {
        _suppressNotify = true;

        PageNumber = 1;
        PageSize = 12;
        CategoryId = null;
        SearchTerm = null;
        SortBy = null;
        SortDirection = AppSortDirection.Asc;

        _suppressNotify = false;
        NotifyChanged();
    }

    public object Snapshot() => new {
        PageNumber,
        PageSize,
        CategoryId,
        SearchTerm,
        SortBy,
        SortDirection };
}
