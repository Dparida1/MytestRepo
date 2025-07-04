using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WorkSpaceManager.Models;
using WorkSpaceManager.Services;

namespace WorkSpaceManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IReportsService _reportsService;
        private readonly IWorkSpaceService _workSpaceService;

        public HomeController(
            ILogger<HomeController> logger,
            IReportsService reportsService,
            IWorkSpaceService workSpaceService)
        {
            _logger = logger;
            _reportsService = reportsService;
            _workSpaceService = workSpaceService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _reportsService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View(new DashboardViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardChartData(string chartType)
        {
            try
            {
                switch (chartType.ToLower())
                {
                    case "region":
                        var regionData = await _reportsService.GetWorkSpacesByRegionAsync();
                        return Json(new
                        {
                            labels = regionData.Select(r => r.Region).ToArray(),
                            data = regionData.Select(r => r.Count).ToArray()
                        });

                    case "state":
                        var stateData = await _reportsService.GetWorkSpacesByStateAsync();
                        return Json(new
                        {
                            labels = stateData.Select(s => s.State).ToArray(),
                            data = stateData.Select(s => s.Count).ToArray()
                        });

                    case "bundle":
                        var bundleData = await _reportsService.GetWorkSpacesByBundleAsync();
                        return Json(new
                        {
                            labels = bundleData.Select(b => b.BundleName).ToArray(),
                            data = bundleData.Select(b => b.Count).ToArray()
                        });

                    default:
                        return BadRequest("Invalid chart type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data for type {ChartType}", chartType);
                return Json(new { labels = new string[0], data = new int[0] });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SyncData(string region)
        {
            try
            {
                if (string.IsNullOrEmpty(region))
                {
                    return Json(new { success = false, message = "Region is required" });
                }

                // Sync data from AWS
                await _workSpaceService.SyncWorkSpacesFromAWSAsync(region);
                await _workSpaceService.SyncBundlesFromAWSAsync(region);
                await _workSpaceService.SyncDirectoriesFromAWSAsync(region);

                return Json(new { success = true, message = $"Data synchronized for region {region}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing data for region {Region}", region);
                return Json(new { success = false, message = "Failed to sync data: " + ex.Message });
            }
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}