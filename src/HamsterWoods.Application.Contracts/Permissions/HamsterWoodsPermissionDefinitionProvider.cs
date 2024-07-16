using HamsterWoods.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace HamsterWoods.Permissions;

public class HamsterWoodsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(HamsterWoodsPermissions.GroupName);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HamsterWoodsResource>(name);
    }
}