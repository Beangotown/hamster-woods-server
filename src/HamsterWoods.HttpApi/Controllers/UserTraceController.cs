using System.Threading.Tasks;
using HamsterWoods.Trace;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Route("api/app/trace/")]
public class UserTraceController : HamsterWoodsBaseController
{
    private readonly ITraceService _traceService;

    public UserTraceController(ITraceService traceService)
    {
        _traceService = traceService;
    }
    
    [HttpPost]
    [Route("user-action")]
    public async Task CreateAsync(GetUserActionDto input)
    {
        await _traceService.CreateAsync(input);
    }
}