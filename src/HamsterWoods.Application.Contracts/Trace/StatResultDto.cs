namespace HamsterWoods.Trace;

public class StatResultDto
{
    public int RegisterCountByDay { get; set; }
    public double RegisterConvertRateByDay { get; set; }
    public int OneGoCountByDay { get; set; }
    public double ActivityRateByDay { get; set; }
    public int FiveGoCountByDay { get; set; }
    public int LoginCountByDay { get; set; }
    public int OneGoLoginCountByDay { get; set; }
            
    public int RegisterCountByWeek { get; set; }
    public double RegisterConvertRateByWeek { get; set; }
    public int OneGoCountByWeek { get; set; }
    public double ActivityRateByWeek { get; set; }
    public int FiveGoCountByWeek { get; set; }
    public int LoginCountByWeek { get; set; }
    public double RetentionRateByWeek { get; set; }
    public int OneGoAddressCountByWeek { get; set; }
    public int LoginAddressCountByWeek { get; set; }
    public int OneGoLoginCountByWeek { get; set; }
}