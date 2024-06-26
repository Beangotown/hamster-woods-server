namespace HamsterWoods.Commons;

public static class AddressHelper
{
    private const string FullAddressPrefix = "ELF";
    private const char FullAddressSeparator = '_';

    public static string ToFullAddress(string address, string chainId)
    {
        return string.Join(FullAddressSeparator, FullAddressPrefix, address, chainId);
    }

    public static string ToShortAddress(string address)
    {
        if (string.IsNullOrEmpty(address)) return address;
        var parts = address.Split(FullAddressSeparator);
        return parts.Length < 3 ? parts[parts.Length - 1] : parts[1];
    }
}