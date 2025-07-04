using Microsoft.AspNetCore.Mvc;
using WorkSpaceManager.Models;
using WorkSpaceManager.Services;

namespace WorkSpaceManager.Controllers
{
    public class WorkSpaceController : Controller
    {
        private readonly IWorkSpaceService _workSpaceService;
        private readonly ILogger<WorkSpaceController> _logger;

        public WorkSpaceController(IWorkSpaceService workSpaceService, ILogger<WorkSpaceController> logger)
        {
            _workSpaceService = workSpaceService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? region, string? state, string? user, string? bundle, int page = 1, int pageSize = 20)
        {
            try
            {
                var workSpaces = await _workSpaceService.GetWorkSpacesAsync(region, state, user, bundle);
                
                var model = new WorkSpaceListViewModel
                {
                    FilterRegion = region,
                    FilterState = state,
                    FilterUser = user,
                    FilterBundle = bundle,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = workSpaces.Count,
                    WorkSpaces = workSpaces.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                    AvailableRegions = await _workSpaceService.GetAvailableRegionsAsync(),
                    AvailableStates = new List<string> { "AVAILABLE", "STOPPED", "PENDING", "STARTING", "STOPPING", "REBOOTING", "TERMINATED", "TERMINATING", "UNHEALTHY" },
                    AvailableBundles = await _workSpaceService.GetAvailableBundlesAsync(region ?? "us-east-1")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading WorkSpaces list");
                TempData["ErrorMessage"] = "Failed to load WorkSpaces: " + ex.Message;
                return View(new WorkSpaceListViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new CreateWorkSpaceRequestViewModel
                {
                    AvailableRegions = await _workSpaceService.GetAvailableRegionsAsync(),
                    AvailableBundles = new List<BundleDto>(),
                    AvailableDirectories = new List<DirectoryDto>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create WorkSpace form");
                TempData["ErrorMessage"] = "Failed to load form: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorkSpaceRequestViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload dropdown data
                    model.AvailableRegions = await _workSpaceService.GetAvailableRegionsAsync();
                    model.AvailableBundles = await _workSpaceService.GetAvailableBundlesAsync(model.Region ?? "us-east-1");
                    model.AvailableDirectories = await _workSpaceService.GetAvailableDirectoriesAsync(model.Region ?? "us-east-1");
                    return View(model);
                }

                var userId = User.Identity?.Name ?? "Anonymous";
                var result = await _workSpaceService.CreateWorkSpacesAsync(model, userId);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    if (result.Errors.Any())
                    {
                        TempData["WarningMessage"] = "Some WorkSpaces failed to create: " + string.Join(", ", result.Errors);
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    // Reload dropdown data
                    model.AvailableRegions = await _workSpaceService.GetAvailableRegionsAsync();
                    model.AvailableBundles = await _workSpaceService.GetAvailableBundlesAsync(model.Region ?? "us-east-1");
                    model.AvailableDirectories = await _workSpaceService.GetAvailableDirectoriesAsync(model.Region ?? "us-east-1");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WorkSpaces");
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                
                // Reload dropdown data
                model.AvailableRegions = await _workSpaceService.GetAvailableRegionsAsync();
                model.AvailableBundles = await _workSpaceService.GetAvailableBundlesAsync(model.Region ?? "us-east-1");
                model.AvailableDirectories = await _workSpaceService.GetAvailableDirectoriesAsync(model.Region ?? "us-east-1");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var workSpace = await _workSpaceService.GetWorkSpaceAsync(id);
                if (workSpace == null)
                {
                    return NotFound();
                }

                return View(workSpace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading WorkSpace details for {Id}", id);
                TempData["ErrorMessage"] = "Failed to load WorkSpace details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Start(string id)
        {
            try
            {
                var result = await _workSpaceService.StartWorkSpaceAsync(id);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting WorkSpace {Id}", id);
                return Json(new { success = false, message = "Failed to start WorkSpace: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Stop(string id)
        {
            try
            {
                var result = await _workSpaceService.StopWorkSpaceAsync(id);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping WorkSpace {Id}", id);
                return Json(new { success = false, message = "Failed to stop WorkSpace: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reboot(string id)
        {
            try
            {
                var result = await _workSpaceService.RebootWorkSpaceAsync(id);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebooting WorkSpace {Id}", id);
                return Json(new { success = false, message = "Failed to reboot WorkSpace: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Terminate(string id)
        {
            try
            {
                var result = await _workSpaceService.TerminateWorkSpaceAsync(id);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating WorkSpace {Id}", id);
                return Json(new { success = false, message = "Failed to terminate WorkSpace: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Modify(string id, string? runningMode, int? autoStopTimeout)
        {
            try
            {
                var result = await _workSpaceService.ModifyWorkSpaceAsync(id, runningMode, autoStopTimeout);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error modifying WorkSpace {Id}", id);
                return Json(new { success = false, message = "Failed to modify WorkSpace: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBundlesForRegion(string region)
        {
            try
            {
                var bundles = await _workSpaceService.GetAvailableBundlesAsync(region);
                return Json(bundles.Select(b => new { value = b.BundleId, text = $"{b.Name} ({b.ComputeTypeName})" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bundles for region {Region}", region);
                return Json(new object[0]);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDirectoriesForRegion(string region)
        {
            try
            {
                var directories = await _workSpaceService.GetAvailableDirectoriesAsync(region);
                return Json(directories.Select(d => new { value = d.DirectoryId, text = $"{d.DirectoryName} ({d.DirectoryType})" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting directories for region {Region}", region);
                return Json(new object[0]);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkOperation(string operation, string[] workSpaceIds)
        {
            try
            {
                if (workSpaceIds == null || !workSpaceIds.Any())
                {
                    return Json(new { success = false, message = "No WorkSpaces selected" });
                }

                var results = new List<string>();
                var errors = new List<string>();

                foreach (var id in workSpaceIds)
                {
                    WorkSpaceOperationResult result;
                    
                    switch (operation.ToLower())
                    {
                        case "start":
                            result = await _workSpaceService.StartWorkSpaceAsync(id);
                            break;
                        case "stop":
                            result = await _workSpaceService.StopWorkSpaceAsync(id);
                            break;
                        case "reboot":
                            result = await _workSpaceService.RebootWorkSpaceAsync(id);
                            break;
                        case "terminate":
                            result = await _workSpaceService.TerminateWorkSpaceAsync(id);
                            break;
                        default:
                            return Json(new { success = false, message = "Invalid operation" });
                    }

                    if (result.Success)
                    {
                        results.Add($"{id}: {result.Message}");
                    }
                    else
                    {
                        errors.Add($"{id}: {result.Message}");
                    }
                }

                var message = $"Operation completed. {results.Count} successful, {errors.Count} failed.";
                if (errors.Any())
                {
                    message += " Errors: " + string.Join("; ", errors);
                }

                return Json(new { success = errors.Count == 0, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation {Operation}", operation);
                return Json(new { success = false, message = "Bulk operation failed: " + ex.Message });
            }
        }
    }
}