using HamsterWoods.Localization;
using Volo.Abp.Application.Services;

namespace HamsterWoods;

/* Inherit your application services from this class.
 */
public abstract class HamsterWoodsBaseService : ApplicationService
{
    protected HamsterWoodsBaseService()
    {
        LocalizationResource = typeof(HamsterWoodsResource);
    }
}
