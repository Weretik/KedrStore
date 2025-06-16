namespace Application.Common.Pagination
{
    public readonly record struct SortDirection
    {
        public static readonly SortDirection Asc = new("Asc");
        public static readonly SortDirection Desc = new("Desc");

        public string Value { get; }

        private SortDirection(string value)
        {
            Value = value;
        }

        public bool IsAscending => Value == Asc.Value;
        public bool IsDescending => Value == Desc.Value;

        public override string ToString() => Value;

        public static SortDirection FromString(string? value) =>
            value?.ToLowerInvariant() switch
            {
                "asc" => Asc,
                "desc" => Desc,
                _ => throw new ArgumentException($"Invalid sort direction: '{value}'")
            };

        public static implicit operator string(SortDirection direction) => direction.Value;
        public static explicit operator SortDirection(string value) => FromString(value);
    }
}

