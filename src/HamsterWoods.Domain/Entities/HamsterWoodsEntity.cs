using System;
using Volo.Abp.Domain.Entities;

namespace HamsterWoods.Entities;

[Serializable]
public abstract class HamsterWoodsEntity <TKey> : Entity, IEntity<TKey>
{
    /// <inheritdoc/>
    public virtual TKey Id { get; set; }

    protected HamsterWoodsEntity()
    {

    }

    protected HamsterWoodsEntity(TKey id)
    {
        Id = id;
    }

    public override object[] GetKeys()
    {
        return new object[] {Id};
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Id = {Id}";
    }
}