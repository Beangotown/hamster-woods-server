namespace HamsterWoods.Entities;

public class MultiChainEntity<TKey> : HamsterWoodsEntity<TKey>, IMultiChain
{
    public virtual int ChainId { get; set; }


    protected MultiChainEntity()
    {
    }

    protected MultiChainEntity(TKey id)
        : base(id)
    {
    }
}