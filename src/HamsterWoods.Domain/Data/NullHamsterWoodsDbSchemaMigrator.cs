using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Data;

/* This is used if database provider does't define
 * IIMDbSchemaMigrator implementation.
 */
public class NullHamsterWoodsDbSchemaMigrator : IHamsterWoodsDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
