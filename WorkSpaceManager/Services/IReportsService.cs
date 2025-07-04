using WorkSpaceManager.Models;

namespace WorkSpaceManager.Services
{
    public interface IReportsService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<ReportsViewModel> GetReportsDataAsync(DateTime? dateFrom, DateTime? dateTo, string? region, string? status);
        Task<List<WorkSpaceReportDto>> GetWorkSpaceReportAsync(DateTime? dateFrom, DateTime? dateTo, string? region);
        Task<List<RequestReportDto>> GetRequestReportAsync(DateTime? dateFrom, DateTime? dateTo, string? region);
        Task<List<WorkSpacesByRegionDto>> GetWorkSpacesByRegionAsync();
        Task<List<WorkSpacesByStateDto>> GetWorkSpacesByStateAsync();
        Task<List<WorkSpacesByBundleDto>> GetWorkSpacesByBundleAsync();
        Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 10);
        Task<byte[]> ExportWorkSpacesToExcelAsync(string? region, string? state, string? userId, string? bundleId);
        Task<byte[]> ExportRequestsToExcelAsync(DateTime? dateFrom, DateTime? dateTo, string? region);
    }
}