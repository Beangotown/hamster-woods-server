using HamsterWoods.Grains.State.Unlock;
using Orleans;

namespace HamsterWoods.Grains.Grain.Unlock;

public interface IUnlockAddressGrain : IGrainWithStringKey
{
    Task<GrainResultDto<List<string>>> SetAddresses(int weekNum, List<string> addresses);
}

public class UnlockAddressGrain : Grain<UnlockAddressState>, IUnlockAddressGrain
{
    public async Task<GrainResultDto<List<string>>> SetAddresses(int weekNum, List<string> addresses)
    {
        var resultDto = new GrainResultDto<List<string>>();
        if (addresses.IsNullOrEmpty())
        {
            return resultDto.Error("addresses is empty.");
        }

        if (State.Id.IsNullOrEmpty())
        {
            State.Id = this.GetPrimaryKeyString();
            State.WeekNum = weekNum;
            State.Addresses = addresses;

            await WriteStateAsync();
            return new GrainResultDto<List<string>>(State.Addresses);
        }

        if (State.WeekNum != weekNum)
        {
            return resultDto.Error("invalid weekNum.");
        }

        var addressList = addresses.Except(State.Addresses).ToList();
        if (!addressList.IsNullOrEmpty())
        {
            State.Addresses.AddRange(addressList);
            await WriteStateAsync();
        }

        return new GrainResultDto<List<string>>(addressList);
    }
}