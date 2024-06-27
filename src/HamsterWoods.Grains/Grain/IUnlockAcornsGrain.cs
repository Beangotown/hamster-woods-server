using Orleans;

namespace HamsterWoods.Grains.Grain;

public interface IUnlockAcornsGrain: IGrainWithStringKey
{
    Task Unlock();
}