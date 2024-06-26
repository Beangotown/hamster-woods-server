using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HamsterWoods.Commons;

public static class SerializeHelper
{
    public static string Serialize(object val)
    {
        var serializeSetting = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return JsonConvert.SerializeObject(val, Formatting.None, serializeSetting);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}