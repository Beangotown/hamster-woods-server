#nullable enable
using System.Threading.Tasks;

namespace HamsterWoods.Commons;

public interface IHttpClientProvider
{
    Task<string> GetAsync(string? token, string url);
}