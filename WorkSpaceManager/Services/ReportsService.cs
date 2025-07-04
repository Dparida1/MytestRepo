using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkSpaceManager.Data;
using WorkSpaceManager.Models;

namespace WorkSpaceManager.Services
{
    public class ReportsService : IReportsService
    {
        private readonly WorkSpaceContext _context;
        private readonly ILogger<ReportsService> _logger;

        public ReportsService(WorkSpaceContext context, ILogger<ReportsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var model = new DashboardViewModel
                {
                    TotalWorkSpaces = await _context.WorkSpaces.CountAsync(),
                    RunningWorkSpaces = await _context.WorkSpaces.CountAsync(w => w.State == "AVAILABLE"),
                    StoppedWorkSpaces = await _context.WorkSpaces.CountAsync(w => w.State == "STOPPED"),
                    PendingRequests = await _context.WorkSpaceRequests.CountAsync(r => r.RequestStatus == "Pending" || r.RequestStatus == "Processing"),
                    CompletedRequestsToday = await _context.WorkSpaceRequests.CountAsync(r => r.CompletedAt >= today && r.RequestStatus == "Completed"),
                    FailedCreationsToday = await _context.WorkSpaceRequests
                        .Where(r => r.RequestedAt >= today)
                        .SumAsync(r => r.FailedCreations),

                    WorkSpacesByRegion = await GetWorkSpacesByRegionAsync(),
                    WorkSpacesByState = await GetWorkSpacesByStateAsync(),
                    WorkSpacesByBundle = await GetWorkSpacesByBundleAsync(),
                    RecentActivity = await GetRecentActivityAsync(10)
                };

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard data");
                return new DashboardViewModel();
            }
        }

        public async Task<ReportsViewModel> GetReportsDataAsync(DateTime? dateFrom, DateTime? dateTo, string? region, string? status)
        {
            try
            {
                var model = new ReportsViewModel
                {
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Region = region,
                    Status = status,
                    AvailableRegions = await _context.WorkSpaces.Select(w => w.Region).Distinct().ToListAsync()
                };

                model.WorkSpaceReport = await GetWorkSpaceReportAsync(dateFrom, dateTo, region);
                model.RequestReport = await GetRequestReportAsync(dateFrom, dateTo, region);

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reports data");
                return new ReportsViewModel();
            }
        }

        public async Task<List<WorkSpaceReportDto>> GetWorkSpaceReportAsync(DateTime? dateFrom, DateTime? dateTo, string? region)
        {
            try
            {
                var query = _context.WorkSpaces.AsQueryable();

                if (dateFrom.HasValue)
                    query = query.Where(w => w.CreatedAt >= dateFrom.Value);

                if (dateTo.HasValue)
                    query = query.Where(w => w.CreatedAt <= dateTo.Value.AddDays(1));

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(w => w.Region == region);

                var data = await query
                    .GroupBy(w => new { w.Region, w.BundleName })
                    .Select(g => new WorkSpaceReportDto
                    {
                        Region = g.Key.Region,
                        BundleName = g.Key.BundleName,
                        TotalCount = g.Count(),
                        RunningCount = g.Count(w => w.State == "AVAILABLE"),
                        StoppedCount = g.Count(w => w.State == "STOPPED"),
                        ErrorCount = g.Count(w => !string.IsNullOrEmpty(w.ErrorMessage)),
                        TotalCostEstimate = 0 // You can implement cost calculation logic here
                    })
                    .OrderBy(r => r.Region)
                    .ThenBy(r => r.BundleName)
                    .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating workspace report");
                return new List<WorkSpaceReportDto>();
            }
        }

        public async Task<List<RequestReportDto>> GetRequestReportAsync(DateTime? dateFrom, DateTime? dateTo, string? region)
        {
            try
            {
                var query = _context.WorkSpaceRequests.AsQueryable();

                if (dateFrom.HasValue)
                    query = query.Where(r => r.RequestedAt >= dateFrom.Value);

                if (dateTo.HasValue)
                    query = query.Where(r => r.RequestedAt <= dateTo.Value.AddDays(1));

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(r => r.Region == region);

                var data = await query
                    .GroupBy(r => r.RequestedAt.Date)
                    .Select(g => new RequestReportDto
                    {
                        Date = g.Key,
                        TotalRequests = g.Count(),
                        CompletedRequests = g.Count(r => r.RequestStatus == "Completed"),
                        FailedRequests = g.Count(r => r.RequestStatus == "Failed"),
                        PendingRequests = g.Count(r => r.RequestStatus == "Pending" || r.RequestStatus == "Processing"),
                        TotalWorkSpacesCreated = g.Sum(r => r.NumberOfWorkSpaces),
                        SuccessfulCreations = g.Sum(r => r.SuccessfulCreations),
                        FailedCreations = g.Sum(r => r.FailedCreations)
                    })
                    .OrderBy(r => r.Date)
                    .ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating request report");
                return new List<RequestReportDto>();
            }
        }

        public async Task<List<WorkSpacesByRegionDto>> GetWorkSpacesByRegionAsync()
        {
            try
            {
                return await _context.WorkSpaces
                    .GroupBy(w => w.Region)
                    .Select(g => new WorkSpacesByRegionDto
                    {
                        Region = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.Region)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspaces by region");
                return new List<WorkSpacesByRegionDto>();
            }
        }

        public async Task<List<WorkSpacesByStateDto>> GetWorkSpacesByStateAsync()
        {
            try
            {
                return await _context.WorkSpaces
                    .GroupBy(w => w.State)
                    .Select(g => new WorkSpacesByStateDto
                    {
                        State = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(s => s.State)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspaces by state");
                return new List<WorkSpacesByStateDto>();
            }
        }

        public async Task<List<WorkSpacesByBundleDto>> GetWorkSpacesByBundleAsync()
        {
            try
            {
                return await _context.WorkSpaces
                    .GroupBy(w => w.BundleName)
                    .Select(g => new WorkSpacesByBundleDto
                    {
                        BundleName = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(b => b.BundleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workspaces by bundle");
                return new List<WorkSpacesByBundleDto>();
            }
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(int count = 10)
        {
            try
            {
                var activities = new List<RecentActivityDto>();

                // Recent workspace creations
                var recentWorkSpaces = await _context.WorkSpaces
                    .OrderByDescending(w => w.CreatedAt)
                    .Take(count / 2)
                    .Select(w => new RecentActivityDto
                    {
                        Activity = $"WorkSpace {w.WorkSpaceId} created for user {w.UserName}",
                        Timestamp = w.CreatedAt,
                        User = w.CreatedBy,
                        Type = "WorkSpace"
                    })
                    .ToListAsync();

                activities.AddRange(recentWorkSpaces);

                // Recent requests
                var recentRequests = await _context.WorkSpaceRequests
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(count / 2)
                    .Select(r => new RecentActivityDto
                    {
                        Activity = $"Request '{r.RequestName}' for {r.NumberOfWorkSpaces} WorkSpaces",
                        Timestamp = r.RequestedAt,
                        User = r.RequestedBy,
                        Type = "Request"
                    })
                    .ToListAsync();

                activities.AddRange(recentRequests);

                return activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity");
                return new List<RecentActivityDto>();
            }
        }

        public async Task<byte[]> ExportWorkSpacesToExcelAsync(string? region, string? state, string? userId, string? bundleId)
        {
            try
            {
                var query = _context.WorkSpaces.AsQueryable();

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(w => w.Region == region);

                if (!string.IsNullOrEmpty(state))
                    query = query.Where(w => w.State == state);

                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(w => w.UserName.Contains(userId));

                if (!string.IsNullOrEmpty(bundleId))
                    query = query.Where(w => w.BundleId == bundleId);

                var workspaces = await query.OrderBy(w => w.Region).ThenBy(w => w.UserName).ToListAsync();

                // Create a simple CSV format for now
                // You can implement Excel export using EPPlus or similar library
                var csv = "WorkSpace ID,User Name,Bundle,Region,State,IP Address,Computer Name,Created At,Created By\n";
                
                foreach (var ws in workspaces)
                {
                    csv += $"{ws.WorkSpaceId},{ws.UserName},{ws.BundleName},{ws.Region},{ws.State},{ws.IpAddress},{ws.ComputerName},{ws.CreatedAt:yyyy-MM-dd},{ws.CreatedBy}\n";
                }

                return System.Text.Encoding.UTF8.GetBytes(csv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting workspaces to Excel");
                throw;
            }
        }

        public async Task<byte[]> ExportRequestsToExcelAsync(DateTime? dateFrom, DateTime? dateTo, string? region)
        {
            try
            {
                var query = _context.WorkSpaceRequests.AsQueryable();

                if (dateFrom.HasValue)
                    query = query.Where(r => r.RequestedAt >= dateFrom.Value);

                if (dateTo.HasValue)
                    query = query.Where(r => r.RequestedAt <= dateTo.Value.AddDays(1));

                if (!string.IsNullOrEmpty(region))
                    query = query.Where(r => r.Region == region);

                var requests = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();

                // Create a simple CSV format for now
                var csv = "Request Name,Number of WorkSpaces,Region,Bundle,Status,Requested At,Requested By,Successful,Failed\n";
                
                foreach (var req in requests)
                {
                    csv += $"{req.RequestName},{req.NumberOfWorkSpaces},{req.Region},{req.BundleName},{req.RequestStatus},{req.RequestedAt:yyyy-MM-dd HH:mm},{req.RequestedBy},{req.SuccessfulCreations},{req.FailedCreations}\n";
                }

                return System.Text.Encoding.UTF8.GetBytes(csv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting requests to Excel");
                throw;
            }
        }
    }
}