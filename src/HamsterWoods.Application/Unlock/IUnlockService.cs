using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Types;
using Contracts.HamsterWoods;
using Google.Protobuf;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Grains.Grain.Unlock;
using HamsterWoods.Points.Etos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Unlock;

public interface IUnlockService
{
    Task BatchUnlockAsync(UnlockInfoGrainDto dto);
}

public class UnlockService : IUnlockService, ISingletonDependency
{
    private readonly ILogger<UnlockService> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IOptionsMonitor<ChainOptions> _chainOptions;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObjectMapper _objectMapper;

    public UnlockService(ILogger<UnlockService> logger, IClusterClient clusterClient,
        IDistributedEventBus distributedEventBus,
        IObjectMapper objectMapper, IOptionsMonitor<ChainOptions> chainOptions)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _distributedEventBus = distributedEventBus;
        _objectMapper = objectMapper;
        _chainOptions = chainOptions;
    }

    public async Task BatchUnlockAsync(UnlockInfoGrainDto dto)
    {
        AssertHelper.NotEmpty(dto.BizId, "Invalid bizId.");
        _logger.LogInformation("[BatchUnlock] bizId:{bizId}", dto.BizId);
        var addresses = dto.Addresses.Where(t => !t.IsNullOrEmpty()).Select(Address.FromBase58).ToList();
        var unlockAcornsInput = new UnlockAcornsInput()
        {
            WeekNum = dto.WeekNum,
            Addresses = { addresses }
        };

        var chainInfo = _chainOptions.CurrentValue.ChainInfos.First().Value;
        var input = new ContractInvokeGrainDto
        {
            ChainId = chainInfo.ChainId,
            BizId = dto.BizId,
            BizType = CommonConstant.BatchUnlockAcorns,
            ContractAddress = chainInfo.HamsterWoodsAddress,
            ContractMethod = CommonConstant.BatchUnlockAcorns,
            Param = unlockAcornsInput.ToByteString().ToBase64()
        };
        var contractInvokeGrain = _clusterClient.GetGrain<IContractInvokeGrain>(dto.BizId);
        var result = await contractInvokeGrain.CreateAsync(input);
        if (!result.Success)
        {
            _logger.LogError(
                "[BatchUnlock] Create Contract Invoke fail, bizId: {dto.BizId}.", dto.BizId);
            throw new UserFriendlyException($"Create Contract Invoke fail, bizId: {dto.BizId}.");
        }

        _logger.LogError(
            "[BatchUnlock] Create Contract Invoke success, bizId: {dto.BizId}.", dto.BizId);
        await _distributedEventBus.PublishAsync(
            _objectMapper.Map<ContractInvokeGrainDto, ContractInvokeEto>(result.Data));
    }
}