using BLL.DTO.ScheduleDtos;

namespace BLL.Contracts;

public interface IAcoScheduleGeneratorService
{
    Task<BllScheduleGenerateResult> GenerateAcoScheduleAsync(int orgId, BllAcoScheduleGenerateRequest request);
}