using Volo.Abp.Settings;

namespace HamsterWoods.Settings;

public class HamsterWoodsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(MessagePushSettings.MySetting1));
    }
}
