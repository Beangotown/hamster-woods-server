using HamsterWoods.Grains.State;
using Orleans;

namespace HamsterWoods.Grains.Grain;

public class UnlockAcornsGrain: Grain<UnlockAcornsState>, IUnlockAcornsGrain
{
    public Task Unlock()
    {
        throw new NotImplementedException();
    }
}