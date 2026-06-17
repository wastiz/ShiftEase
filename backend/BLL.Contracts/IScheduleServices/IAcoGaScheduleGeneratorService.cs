using BLL.DTO.ScheduleDtos;

namespace BLL.Contracts;

public interface IAcoGaScheduleGeneratorService
{
    Task<BllScheduleGenerateResult> GenerateAcoGaScheduleAsync(int orgId, BllAcoGaScheduleGenerateRequest request);
}
