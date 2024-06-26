using System;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace HamsterWoods.Entities.Es;

public class UserDeviceIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string AppId { get; set; }
    [Keyword] public string UserId { get; set; }
    [Keyword] public string RegistrationToken { get; set; }
    [Keyword] public string DeviceId { get; set; }
    [Keyword] public string AppStatus { get; set; }
    public long RefreshTime { get; set; }
    public DateTime ModificationTime { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
    // public List<string> Topics { get; set; }
}

public class DeviceInfo
{
    [Keyword] public string DeviceType { get; set; }
    [Keyword] public string DeviceBrand { get; set; }
    [Keyword] public string OperatingSystemVersion { get; set; }
}