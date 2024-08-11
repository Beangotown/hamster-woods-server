using System;

namespace HamsterWoods.Commons;

public static class CommonConstant
{
    public const string JwtPrefix = "Bearer";
    public const string AuthHeader = "Authorization";
    public static DateTimeOffset DefaultAbsoluteExpiration = DateTime.Parse("2099-01-01 12:00:00");
    public const string AppIdKeyName = "AppId";

    public const string DefaultTitle = "title";
    public const string DefaultContent = "content";
    public const string ElfSymbol = "ELF";
    public const string AcornsSymbol = "ACORNS";
    public const int UsedTokenDecimals = 8;

    public const string DataPriceCacheKey = "DataPriceCache";
    public const int DefaultSkipCount = 0;
    public const int DefaultMaxResultCount = 100;
    public const int DefaultQueryMaxResultCount = 1000;
    
    public const string TradeRepeated = "TradeRepeated";
}

public static class ContractConstant
{
    public const string Mined = "MINED";
    public const string Pending = "PENDING";
    public const string Notexisted = "NOTEXISTED";
}