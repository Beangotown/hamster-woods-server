using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.Points.Dtos;

namespace HamsterWoods.Points;

public interface IPointHubService
{
    Task RequestPointsList(PointsListRequestDto request);
    Task<FluxPointResultDto> GetFluxPointsAsync(string address, string connectionId);
}