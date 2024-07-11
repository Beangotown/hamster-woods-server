using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Contracts.HamsterWoods;
using Google.Protobuf;
using HamsterWoods.Contract;
using HamsterWoods.NFT;
using HamsterWoods.Rank;
using HamsterWoods.Reward.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using AddressHelper = HamsterWoods.Commons.AddressHelper;

namespace HamsterWoods.Reward;

public interface ISendAcornsProvider
{
    Task<KingHamsterClaimDto> SendAsync(HamsterPassInput input);
}

public class SendAcornsProvider : ISendAcornsProvider, ISingletonDependency
{
    private readonly ChainOptions _chainOptions;
    private readonly AElfClientFactory _factory;
    private readonly ILogger<SendAcornsProvider> _logger;
    private readonly IRankProvider _rankProvider;

    public SendAcornsProvider(IOptionsSnapshot<ChainOptions> chainOptions,
        AElfClientFactory factory,
        ILogger<SendAcornsProvider> logger, IRankProvider rankProvider)
    {
        _chainOptions = chainOptions.Value;
        _factory = factory;
        _logger = logger;
        _rankProvider = rankProvider;
    }

    public async Task<KingHamsterClaimDto> SendAsync(HamsterPassInput input)
    {
        try
        {
            var chainId = _chainOptions.ChainInfos.Keys.First();
            var weekNum = 5;
            var caAddress = AddressHelper.ToFullAddress(input.CaAddress, chainId);
            var transferParam = new UnlockAcornsInput
            {
                Addresses = {  new[] { Address.FromBase58(input.CaAddress) } },
                WeekNum = 1
            };
            
            
            var output = await SendTransactionAsync(chainId, _chainOptions.ChainInfos[chainId].HamsterWoodsAddress,
                "BatchUnlockAcorns", transferParam);
            _logger.LogInformation("BatchUnlockAcorns success, transactionId:{transactionId}, adds:{adds}",
                output.TransactionId, input.CaAddress);

            // var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, caAddress, 0,
            //     1000);
            // var needSendAddresses =
            //     rankInfos.RankingList.Select(t => AddressHelper.ToShortAddress(t.CaAddress)).ToList();
            //
            // var adds = new List<string>();
            // foreach (var item in needSendAddresses)
            // {
            //     if (adds.Count > 10)
            //     {
            //         await BatchTransfer(adds, chainId);
            //         adds.Clear();
            //     }
            //
            //     adds.Add(item);
            // }
            //
            // if (adds.Count > 0)
            // {
            //     await BatchTransfer(adds, chainId);
            // }

            return new KingHamsterClaimDto()
            {
                Claimable = true
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "BatchUnlockAcorns Error, message:{message}", e.Message);
            throw;
        }
    }

    private async Task BatchTransfer(List<string> adds, string chainId)
    {
        var itemsParam = GetUnlockAcornsInput(adds);
        var itemsOutput = await SendTransactionAsync(chainId,
            _chainOptions.ChainInfos[chainId].HamsterWoodsAddress,
            "BatchUnlockAcorns", itemsParam);

        var ads = GetAddresses(adds);
        _logger.LogInformation("BatchUnlockAcorns success, transactionId:{transactionId}, adds:{adds}",
            itemsOutput.TransactionId, ads);
    }

    private UnlockAcornsInput GetUnlockAcornsInput(List<string> list)
    {
        var res = new List<Address>();
        var itemsParam = new UnlockAcornsInput
        {
            Addresses = {  },
            WeekNum = 5
        };

        foreach (var item in list)
        {
            itemsParam.Addresses.Add(Address.FromBase58(item));
        }

        return itemsParam;
    }

    private string GetAddresses(List<string> list)
    {
        var res = "";
        foreach (var item in list)
        {
            res = res + $", {item}";
        }

        return res;
    }

    private async Task<SendTransactionOutput> SendTransactionAsync(string chainId, string contractAddress,
        string method, IMessage param)
    {
        var key = _chainOptions.ChainInfos[chainId].PrivateKey;
        var client = _factory.GetClient(chainId);

        var address = client.GetAddressFromPrivateKey(key);

        var transaction = await client.GenerateTransactionAsync(address, contractAddress, method, param);

        var txWithSign = client.SignTransaction(key, transaction);

        var rawTransaction = txWithSign.ToByteArray().ToHex();

        var result = await client.SendTransactionAsync(new SendTransactionInput
        {
            RawTransaction = rawTransaction
        });

        return result;
    }
}