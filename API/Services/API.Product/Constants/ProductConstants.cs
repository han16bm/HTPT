namespace API.Product.Constants;

public static class ProductConstants
{
    public const string ServiceName = "API.Product";
    public const string DefaultOrderBy = "created_at";
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 100;

    public static class OrderSource
    {
        public const string Online = "ONLINE";
        public const string Pos = "POS";
    }

    public static class TransactionType
    {
        public const string Import = "IMPORT";
        public const string Export = "EXPORT";
        public const string Adjustment = "ADJUSTMENT";
        public const string Return = "RETURN";
    }
}
