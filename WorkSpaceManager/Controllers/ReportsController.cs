using Microsoft.AspNetCore.Mvc;
using WorkSpaceManager.Models;
using WorkSpaceManager.Services;

namespace WorkSpaceManager.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;
        private readonly IWorkSpaceService _workSpaceService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportsService reportsService,
            IWorkSpaceService workSpaceService,
            ILogger<ReportsController> logger)
        {
            _reportsService = reportsService;
            _workSpaceService = workSpaceService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo, string? region, string? status)
        {
            try
            {
                var model = await _reportsService.GetReportsDataAsync(dateFrom, dateTo, region, status);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
                TempData["ErrorMessage"] = "Failed to load reports: " + ex.Message;
                return View(new ReportsViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> WorkSpaceReport(DateTime? dateFrom, DateTime? dateTo, string? region)
        {
            try
            {
                var data = await _reportsService.GetWorkSpaceReportAsync(dateFrom, dateTo, region);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating WorkSpace report");
                return Json(new List<WorkSpaceReportDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> RequestReport(DateTime? dateFrom, DateTime? dateTo, string? region)
        {
            try
            {
                var data = await _reportsService.GetRequestReportAsync(dateFrom, dateTo, region);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating request report");
                return Json(new List<RequestReportDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportWorkSpaces(string? region, string? state, string? userId, string? bundleId, string format = "csv")
        {
            try
            {
                var data = await _reportsService.ExportWorkSpacesToExcelAsync(region, state, userId, bundleId);
                
                var fileName = $"workspaces_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
                var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                
                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting WorkSpaces");
                TempData["ErrorMessage"] = "Failed to export WorkSpaces: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportRequests(DateTime? dateFrom, DateTime? dateTo, string? region, string format = "csv")
        {
            try
            {
                var data = await _reportsService.ExportRequestsToExcelAsync(dateFrom, dateTo, region);
                
                var fileName = $"requests_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
                var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                
                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting requests");
                TempData["ErrorMessage"] = "Failed to export requests: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var model = await _reportsService.GetDashboardDataAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard reports");
                TempData["ErrorMessage"] = "Failed to load dashboard: " + ex.Message;
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType, DateTime? dateFrom, DateTime? dateTo, string? region)
        {
            try
            {
                switch (chartType.ToLower())
                {
                    case "workspaces-by-region":
                        var regionData = await _reportsService.GetWorkSpacesByRegionAsync();
                        return Json(new
                        {
                            labels = regionData.Select(r => r.Region).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "WorkSpaces by Region",
                                    data = regionData.Select(r => r.Count).ToArray(),
                                    backgroundColor = GenerateColors(regionData.Count)
                                }
                            }
                        });

                    case "workspaces-by-state":
                        var stateData = await _reportsService.GetWorkSpacesByStateAsync();
                        return Json(new
                        {
                            labels = stateData.Select(s => s.State).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "WorkSpaces by State",
                                    data = stateData.Select(s => s.Count).ToArray(),
                                    backgroundColor = GenerateColors(stateData.Count)
                                }
                            }
                        });

                    case "workspaces-by-bundle":
                        var bundleData = await _reportsService.GetWorkSpacesByBundleAsync();
                        return Json(new
                        {
                            labels = bundleData.Select(b => b.BundleName).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "WorkSpaces by Bundle",
                                    data = bundleData.Select(b => b.Count).ToArray(),
                                    backgroundColor = GenerateColors(bundleData.Count)
                                }
                            }
                        });

                    case "requests-timeline":
                        var requestData = await _reportsService.GetRequestReportAsync(dateFrom, dateTo, region);
                        return Json(new
                        {
                            labels = requestData.Select(r => r.Date.ToString("yyyy-MM-dd")).ToArray(),
                            datasets = new[]
                            {
                                new
                                {
                                    label = "Total Requests",
                                    data = requestData.Select(r => r.TotalRequests).ToArray(),
                                    borderColor = "#007bff",
                                    backgroundColor = "#007bff",
                                    fill = false
                                },
                                new
                                {
                                    label = "Completed Requests",
                                    data = requestData.Select(r => r.CompletedRequests).ToArray(),
                                    borderColor = "#28a745",
                                    backgroundColor = "#28a745",
                                    fill = false
                                },
                                new
                                {
                                    label = "Failed Requests",
                                    data = requestData.Select(r => r.FailedRequests).ToArray(),
                                    borderColor = "#dc3545",
                                    backgroundColor = "#dc3545",
                                    fill = false
                                }
                            }
                        });

                    default:
                        return BadRequest("Invalid chart type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating chart data for type {ChartType}", chartType);
                return Json(new { labels = new string[0], datasets = new object[0] });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                var dashboard = await _reportsService.GetDashboardDataAsync();
                
                return Json(new
                {
                    totalWorkSpaces = dashboard.TotalWorkSpaces,
                    runningWorkSpaces = dashboard.RunningWorkSpaces,
                    stoppedWorkSpaces = dashboard.StoppedWorkSpaces,
                    pendingRequests = dashboard.PendingRequests,
                    completedRequestsToday = dashboard.CompletedRequestsToday,
                    failedCreationsToday = dashboard.FailedCreationsToday
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics");
                return Json(new { });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var activities = await _reportsService.GetRecentActivityAsync(20);
                return Json(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity");
                return Json(new List<RecentActivityDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCustomReport(DateTime? dateFrom, DateTime? dateTo, string? region, string[] metrics)
        {
            try
            {
                var reportData = new
                {
                    dateFrom,
                    dateTo,
                    region,
                    generatedAt = DateTime.UtcNow,
                    workSpaceReport = await _reportsService.GetWorkSpaceReportAsync(dateFrom, dateTo, region),
                    requestReport = await _reportsService.GetRequestReportAsync(dateFrom, dateTo, region),
                    workSpacesByRegion = await _reportsService.GetWorkSpacesByRegionAsync(),
                    workSpacesByState = await _reportsService.GetWorkSpacesByStateAsync(),
                    workSpacesByBundle = await _reportsService.GetWorkSpacesByBundleAsync()
                };

                return Json(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report");
                return Json(new { error = "Failed to generate report: " + ex.Message });
            }
        }

        private string[] GenerateColors(int count)
        {
            var colors = new[]
            {
                "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
                "#FF9F40", "#FF6384", "#C9CBCF", "#4BC0C0", "#36A2EB"
            };

            if (count <= colors.Length)
            {
                return colors.Take(count).ToArray();
            }

            // Generate additional colors if needed
            var result = new List<string>(colors);
            var random = new Random();
            
            for (int i = colors.Length; i < count; i++)
            {
                var color = $"#{random.Next(0x1000000):X6}";
                result.Add(color);
            }

            return result.ToArray();
        }
    }
}