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

    public async Task<long> GetCaHolderCreateTimeAsync(BeanPassInput beanPassInput)
    {
        long timeStamp = 0;
        var paramStr = BuildParamStr(new Dictionary<string, string>
        {
            { "caAddress", beanPassInput.CaAddress }
        });
        var url = string.Concat(_portkeyOptions.BaseUrl, _getCaHolderCreateTimeUrl, "?", paramStr);
        try
        {
            var result = await _httpClientProvider.GetAsync(null, url);
            long.TryParse(result, out timeStamp);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetCaHolderCreateTimeAsync error {CaAddress}", beanPassInput.CaAddress);
            throw new UserFriendlyException(BeangoTownConstants.SyncingMessage, BeangoTownConstants.SyncingCode);
        }

        return timeStamp;
    }

    public async Task<long> GetTokenBalanceAsync(BeanPassInput beanPassInput)
    {
        long balance = 0;
        var paramStr = BuildParamStr(new Dictionary<string, string>
        {
            { "caAddress", beanPassInput.CaAddress },
            { "symbol", _elf }
        });
        var url = string.Concat(_portkeyOptions.BaseUrl, _tokenBalanceUrl, "?", paramStr);

        try
        {
            var res = await _httpClientProvider.GetAsync(null, url);
            long.TryParse(JsonConvert.DeserializeObject<TokenInfoDto>(res).Balance, out balance);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetCaHolderCreateTimeAsync error {CaAddress}", beanPassInput.CaAddress);
        }

        return (long)ToPrice(balance, 8);
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