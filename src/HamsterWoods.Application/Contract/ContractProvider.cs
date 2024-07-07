using System.Threading.Tasks;
using AElf;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Contracts.HamsterWoods;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using HamsterWoods.Commons;
using HamsterWoods.Monitor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Contract;

public class ContractProvider : IContractProvider, ISingletonDependency
{
    private readonly ChainOptions _chainOptions;
    private readonly AElfClientFactory _factory;
    private readonly ILogger<ContractProvider> _logger;
    private readonly IIndicatorScope _indicatorScope;

    public ContractProvider(IOptionsSnapshot<ChainOptions> chainOptions,
        AElfClientFactory factory,
        ILogger<ContractProvider> logger,
        IIndicatorScope indicatorScope)
    {
        _chainOptions = chainOptions.Value;
        _factory = factory;
        _logger = logger;
        _indicatorScope = indicatorScope;
    }


    public async Task<GetBalanceOutput> GetBalanceAsync(string symbol, string address, string chainId)
    {
        if (!_chainOptions.ChainInfos.TryGetValue(chainId, out _)) return null;

        var getBalanceParam = new GetBalanceInput
        {
            Symbol = symbol,
            Owner = Address.FromBase58(address)
        };

        return await CallTransactionAsync<GetBalanceOutput>(chainId,
            _chainOptions.ChainInfos[chainId].TokenContractAddress, AElfConstants.GetBalance, getBalanceParam);
    }

    public async Task<Hash> GetRandomHash(long targetHeight, string chainId)
    {
        if (!_chainOptions.ChainInfos.TryGetValue(chainId, out _)) return null;

        var height = new Int64Value
        {
            Value = targetHeight
        };
        return await CallTransactionAsync<Hash>(chainId,
            _chainOptions.ChainInfos[chainId].ConsensusContract, AElfConstants.GetRandomHash, height);
    }


    public async Task<long> GetBlockHeightAsync(string chainId)
    {
        var client = _factory.GetClient(chainId);

        var interIndicator = _indicatorScope.Begin(MonitorTag.AelfClient,
            MonitorAelfClientType.GetBlockHeightAsync.ToString());
        var blockHeight = await client.GetBlockHeightAsync();
        _indicatorScope.End(interIndicator);

        return blockHeight;
    }

    public async Task<TokenInfo> GetTokenInfo(string symbol, string chainId)
    {
        if (!_chainOptions.ChainInfos.TryGetValue(chainId, out _)) return null;

        var getTokenInfo = new GetTokenInfoInput()
        {
            Symbol = symbol
        };

        return await CallTransactionAsync<TokenInfo>(chainId,
            _chainOptions.ChainInfos[chainId].TokenContractAddress, AElfConstants.GetTokenInfo, getTokenInfo);
    }

    public async Task<SendTransactionOutput> SendTransferAsync(string symbol, string amount, string address,
        string chainId)
    {
        var transferParam = new TransferInput
        {
            Symbol = symbol,
            Amount = long.Parse(amount),
            To = Address.FromBase58(address)
        };

        return await SendTransactionAsync(chainId, _chainOptions.ChainInfos[chainId].TokenContractAddress,
            AElfConstants.Transfer, transferParam);
    }


    private async Task<T> CallTransactionAsync<T>(string chainId, string contractAddress, string methodName,
        IMessage param
    ) where T : class, IMessage<T>, new()
    {
        var key = _chainOptions.ChainInfos[chainId].PrivateKey;

        var client = _factory.GetClient(chainId);

        var generateIndicator = _indicatorScope.Begin(MonitorTag.AelfClient,
            MonitorAelfClientType.GenerateTransactionAsync.ToString());
        var transaction =
            await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(key), contractAddress, methodName,
                param);
        _indicatorScope.End(generateIndicator);

        _logger.LogDebug("Call tx methodName is: {methodName} param is: {transaction}", methodName, transaction);

        var txWithSign = client.SignTransaction(key, transaction);

        var interIndicator = _indicatorScope.Begin(MonitorTag.AelfClient,
            MonitorAelfClientType.ExecuteTransactionAsync.ToString());
        var result = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSign.ToByteArray().ToHex()
        });
        _indicatorScope.End(interIndicator);

        var value = new T();
        value.MergeFrom(ByteArrayHelper.HexStringToByteArray(result));

        return value;
    }

    private async Task<SendTransactionOutput> SendTransactionAsync(string chainId, string contractAddress,
        string method, IMessage param)
    {
        var key = _chainOptions.ChainInfos[chainId].PrivateKey;

        var client = _factory.GetClient(chainId);

        var address = client.GetAddressFromPrivateKey(key);

        var generateIndicator = _indicatorScope.Begin(MonitorTag.AelfClient,
            MonitorAelfClientType.GenerateTransactionAsync.ToString());
        var transaction = await client.GenerateTransactionAsync(address, contractAddress, method, param);
        _indicatorScope.End(generateIndicator);

        var txWithSign = client.SignTransaction(key, transaction);

        var rawTransaction = txWithSign.ToByteArray().ToHex();

        var interIndicator = _indicatorScope.Begin(MonitorTag.AelfClient,
            MonitorAelfClientType.SendTransactionAsync.ToString());
        var result = await client.SendTransactionAsync(new SendTransactionInput
        {
            RawTransaction = rawTransaction
        });
        _indicatorScope.End(interIndicator);

        return result;
    }


    public async Task<TransactionResultDto> GetTransactionResultAsync(string chainId, string transactionId)
    {
        var client = _factory.GetClient(chainId);

        var interIndicator = _indicatorScope.Begin(MonitorTag.AelfClient,
            MonitorAelfClientType.GetTransactionResultAsync.ToString());
        var result = await client.GetTransactionResultAsync(transactionId);
        _indicatorScope.End(interIndicator);

        return result;
    }

    public async Task<CurrentRaceInfo> GetCurrentRaceInfoAsync(string chainId)
    {
        if (!_chainOptions.ChainInfos.TryGetValue(chainId, out _)) return null;

        return await CallTransactionAsync<CurrentRaceInfo>(chainId,
            _chainOptions.ChainInfos[chainId].HamsterWoodsAddress, AElfConstants.GetCurrentRaceInfo, new Empty());
    }
}