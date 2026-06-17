namespace BLL.Contracts;

public interface IScheduleExportService
{
    Task<byte[]> ExportScheduleToExcelAsync(int scheduleId);
}
