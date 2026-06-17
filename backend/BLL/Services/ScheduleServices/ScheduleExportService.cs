using BLL.Contracts;
using ClosedXML.Excel;
using DTOs.EmployeeDtos;

namespace BLL.Services;

public class ScheduleExportService : IScheduleExportService
{
    private readonly IScheduleService _service;

    public ScheduleExportService(IScheduleService service)
    {
        _service = service;
    }

    public async Task<byte[]> ExportScheduleToExcelAsync(int scheduleId)
    {
        var schedule = await _service.GetScheduleByIdAsync(scheduleId);
        
        if (schedule == null)
            throw new KeyNotFoundException($"Schedule with id {scheduleId} not found");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Schedule");

        // Title
        worksheet.Cell(1, 1).Value = $"Schedule: {schedule.StartDate:yyyy-MM-dd} - {schedule.EndDate:yyyy-MM-dd}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Get all unique dates
        var dates = schedule.Shifts
            .Select(s => s.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // Get all unique employees
        var employees = schedule.Shifts
            .SelectMany(s => s.Employees ?? new())
            .DistinctBy(e => e.Id)
            .OrderBy(e => e.Name)
            .ToList();

        // Headers
        int headerRow = 3;
        worksheet.Cell(headerRow, 1).Value = "Employee";
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;
        worksheet.Cell(headerRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < dates.Count; i++)
        {
            var date = dates[i];
            worksheet.Cell(headerRow, i + 2).Value = date.ToString("dd MMM (ddd)");
            worksheet.Cell(headerRow, i + 2).Style.Font.Bold = true;
            worksheet.Cell(headerRow, i + 2).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Cell(headerRow, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        int row = headerRow + 1;
        foreach (var employee in employees)
        {
            worksheet.Cell(row, 1).Value = employee.Name;
            worksheet.Cell(row, 1).Style.Font.Bold = true;

            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates[i];
                var shift = schedule.Shifts
                    .FirstOrDefault(s => s.Date == date && 
                                       s.Employees != null && 
                                       s.Employees.Any(e => e.Id == employee.Id));

                if (shift != null)
                {
                    var cellValue = $"{shift.ShiftTypeName}\n{shift.StartTime:hh\\:mm}-{shift.EndTime:hh\\:mm}";
                    var cell = worksheet.Cell(row, i + 2);
                    cell.Value = cellValue;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Alignment.WrapText = true;
                    
                    // Color from shift type
                    if (!string.IsNullOrEmpty(shift.Color))
                    {
                        try
                        {
                            var color = XLColor.FromHtml(shift.Color);
                            cell.Style.Fill.BackgroundColor = color;
                        }
                        catch
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        }
                    }
                }
                else
                {
                    worksheet.Cell(row, i + 2).Value = "-";
                    worksheet.Cell(row, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }

            row++;
        }

        // Borders
        var dataRange = worksheet.Range(headerRow, 1, row - 1, dates.Count + 1);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // Column widths
        worksheet.Column(1).Width = 20;
        for (int i = 2; i <= dates.Count + 1; i++)
        {
            worksheet.Column(i).Width = 15;
        }

        // Row heights
        for (int i = headerRow + 1; i < row; i++)
        {
            worksheet.Row(i).Height = 35;
        }

        // Save to memory stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
