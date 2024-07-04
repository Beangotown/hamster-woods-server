using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Commons;
using HamsterWoods.NFT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Portkey;

public class PortkeyProvider : IPortkeyProvider, ISingletonDependency
{
    private readonly string _elf = "ELF";
    private readonly string _getCaHolderCreateTimeUrl = "/api/app/user/activities/getCaHolderCreateTime";
    private readonly IHttpClientProvider _httpClientProvider;
    private readonly ILogger<PortkeyProvider> _logger;
    private readonly PortkeyOptions _portkeyOptions;
    private readonly string _tokenBalanceUrl = "/api/app/user/assets/tokenBalance";

    public PortkeyProvider(IHttpClientProvider httpClientProvider, ILogger<PortkeyProvider> logger,
        IOptionsSnapshot<PortkeyOptions> portkeyOptions)
    {
        _httpClientProvider = httpClientProvider;
        _logger = logger;
        _portkeyOptions = portkeyOptions.Value;
    }

    public async Task<long> GetCaHolderCreateTimeAsync(HamsterPassInput hamsterPassInput)
    {
        long timeStamp = 0;
        var paramStr = BuildParamStr(new Dictionary<string, string>
        {
            { "caAddress", hamsterPassInput.CaAddress }
        });
        var url = string.Concat(_portkeyOptions.BaseUrl, _getCaHolderCreateTimeUrl, "?", paramStr);
        try
        {
            var result = await _httpClientProvider.GetAsync(null, url);
            long.TryParse(result, out timeStamp);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetCaHolderCreateTimeAsync error {CaAddress}", hamsterPassInput.CaAddress);
            throw new UserFriendlyException(HamsterWoodsConstants.SyncingMessage, HamsterWoodsConstants.SyncingCode);
        }

        return timeStamp;
    }

    public async Task<TokenInfoDto> GetHolderTokenInfoAsync(string caAddress, string symbol)
    {
        var paramStr = BuildParamStr(new Dictionary<string, string>
        {
            { "caAddress", caAddress },
            { "symbol", symbol }
        });
        var url = string.Concat(_portkeyOptions.BaseUrl, _tokenBalanceUrl, "?", paramStr);

        try
        {
            var res = await _httpClientProvider.GetAsync(null, url);
            return JsonConvert.DeserializeObject<TokenInfoDto>(res);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetHolderTokenInfoAsync error {CaAddress}", caAddress);
            return new TokenInfoDto();
        }
    }

    private static string BuildParamStr(Dictionary<string, string> paramDict)
    {
        var keyValuePairs = paramDict.Select(kvp => kvp.Key + "=" + kvp.Value).ToList();
        return string.Join("&&", keyValuePairs);
    }

    private static decimal ToPrice(long amount, int decimals)
    {
        return amount / (decimal)Math.Pow(10, decimals);
    }
}