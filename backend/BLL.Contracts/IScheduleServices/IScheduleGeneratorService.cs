using BLL.DTO.ScheduleDtos;

namespace BLL.Contracts;

public interface IScheduleGeneratorService
{
    Task<BllScheduleGenerateResult> GenerateGreedyScheduleAsync(int orgId, BllScheduleGenerateRequest request);
}
