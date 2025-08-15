namespace Application.Common.Pagination
{
    public readonly record struct AppSortDirection
    {
        public static readonly AppSortDirection Asc = new("Asc");
        public static readonly AppSortDirection Desc = new("Desc");

        public string Value { get; }

        private AppSortDirection(string value)
        {
            Value = value;
        }

        public bool IsAscending => Value == Asc.Value;
        public bool IsDescending => Value == Desc.Value;

        public override string ToString() => Value;

        public static AppSortDirection FromString(string? value) =>
            value?.ToLowerInvariant() switch
            {
                "asc" => Asc,
                "desc" => Desc,
                _ => throw new ArgumentException($"Invalid sort direction: '{value}'")
            };

        public static implicit operator string(AppSortDirection direction) => direction.Value;
        public static explicit operator AppSortDirection(string value) => FromString(value);
    }
}

