using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Grains.State.UserPoints;
using HamsterWoods.UserPoints.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Grains.Grain.UserPoints;

public class UserPointsGrain : Grain<UserPointsState>, IUserPointsGrain
{
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<UserPointsGrain> _logger;
    private readonly ChainOptions _options;

    public UserPointsGrain(IObjectMapper objectMapper, ILogger<UserPointsGrain> logger,
        IOptionsSnapshot<ChainOptions> options)
    {
        _objectMapper = objectMapper;
        _logger = logger;
        _options = options.Value;
    }

    public override async Task OnActivateAsync()
    {
        await ReadStateAsync();
        await base.OnActivateAsync();
    }

    public override async Task OnDeactivateAsync()
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync();
    }

    public async Task<GrainResultDto<SetHopDto>> SetHop(List<string> ids)
    {
        if (State.Id.IsNullOrEmpty())
        {
            State.Id = this.GetPrimaryKeyString();
            State.Address = AddressHelper.ToShortAddress(this.GetPrimaryKeyString());
            State.ChainId = _options.ChainInfos.Keys.First();
        }

        if (!State.Hop.ContainsKey(GetHopKey()))
        {
            State.Hop.Add(GetHopKey(), new List<string>());
        }

        var hops = State.Hop[GetHopKey()];
        var lastCount = hops.Count;
        foreach (var id in ids)
        {
            if (hops.Contains(id)) continue;
            hops.Add(id);
        }

        await WriteStateAsync();
        return new GrainResultDto<SetHopDto>(new SetHopDto
        {
            CurrentCount = hops.Count,
            LastCount = lastCount
        });
    }

    private string GetHopKey() => DateTime.UtcNow.ToString("yyyy-MM-dd");
}