using BLL.DTO.ScheduleDtos;

namespace BLL.Contracts;

public interface IGaScheduleGeneratorService
{
    Task<BllScheduleGenerateResult> GenerateGaScheduleAsync(int orgId, BllGaScheduleGenerateRequest request);
}
