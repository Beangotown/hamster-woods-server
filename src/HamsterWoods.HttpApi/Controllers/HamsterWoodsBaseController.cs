using HamsterWoods.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace HamsterWoods.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class HamsterWoodsBaseController : AbpControllerBase
{
    protected HamsterWoodsBaseController()
    {
        LocalizationResource = typeof(HamsterWoodsResource);
    }
}