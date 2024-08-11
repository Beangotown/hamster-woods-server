using System.Linq;
using System.Threading.Tasks;
using AElf.Types;
using Contracts.HamsterWoods;
using Google.Protobuf;
using HamsterWoods.Commons;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Options;
using HamsterWoods.Points.Dtos;
using HamsterWoods.Points.Etos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Points;

public interface IPointSettleService
{
    Task BatchSettleAsync(PointSettleDto dto);
}

public class PointSettleService : IPointSettleService, ISingletonDependency
{
    private readonly ILogger<PointSettleService> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IOptionsMonitor<PointTradeOptions> _pointTradeOptions;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObjectMapper _objectMapper;

    public PointSettleService(ILogger<PointSettleService> logger, IClusterClient clusterClient,
        IOptionsMonitor<PointTradeOptions> pointTradeOptions, IDistributedEventBus distributedEventBus,
        IObjectMapper objectMapper)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _pointTradeOptions = pointTradeOptions;
        _distributedEventBus = distributedEventBus;
        _objectMapper = objectMapper;
    }

    public async Task BatchSettleAsync(PointSettleDto dto)
    {
        AssertHelper.NotEmpty(dto.BizId, "Invalid bizId.");
        _logger.LogInformation("[BatchSettle] bizId:{bizId}", dto.BizId);
        var actionName = _pointTradeOptions.CurrentValue.GetActionName(dto.PointName);
        AssertHelper.NotEmpty(actionName, "Invalid actionName.");
        var chainInfo = _pointTradeOptions.CurrentValue.GetChainInfo(dto.ChainId);
        AssertHelper.NotNull(chainInfo, "Invalid chainInfo.");
        var userPoints = dto.UserPointsInfos
            .Where(item => item.PointAmount > 0)
            .Select(item => new Contracts.HamsterWoods.UserPoints
            {
                UserAddress = Address.FromBase58(item.Address),
                UserPoints_ = DecimalHelper.ConvertToLong(item.PointAmount, 0)
            }).ToList();
        var batchSettleInput = new BatchSettleInput()
        {
            ActionName = actionName,
            UserPointsList = { userPoints }
        };
        var input = new ContractInvokeGrainDto
        {
            ChainId = dto.ChainId,
            BizId = dto.BizId,
            BizType = dto.PointName,
            ContractAddress = chainInfo.SchrodingerContractAddress,
            ContractMethod = chainInfo.ContractMethod,
            Param = batchSettleInput.ToByteString().ToBase64()
        };
        var contractInvokeGrain = _clusterClient.GetGrain<IContractInvokeGrain>(dto.BizId);
        var result = await contractInvokeGrain.CreateAsync(input);
        if (!result.Success)
        {
            _logger.LogError(
                "[BatchSettle] Create Contract Invoke fail, bizId: {dto.BizId}.", dto.BizId);
            throw new UserFriendlyException($"Create Contract Invoke fail, bizId: {dto.BizId}.");
        }

        await _distributedEventBus.PublishAsync(
            _objectMapper.Map<ContractInvokeGrainDto, ContractInvokeEto>(result.Data));
    }
}