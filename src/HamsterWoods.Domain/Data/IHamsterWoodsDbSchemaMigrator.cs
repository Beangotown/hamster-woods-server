using System.Threading.Tasks;

namespace HamsterWoods.Data;

public interface IHamsterWoodsDbSchemaMigrator
{
    Task MigrateAsync();
}
