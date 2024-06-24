using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("HamsterWoods")]
[Route("api/v1/messagePush")]
public class HamsterWoodsController : HamsterWoodsBaseController
{

}