using System.ComponentModel.DataAnnotations;

namespace HamsterWoods.Trace;

public class GetUserActionDto
{
    [Required]
    public string CaAddress { get; set; }
    public string CaHash { get; set; }
    public string ChainId { get; set; }
    public int ActionType { get; set; }
}