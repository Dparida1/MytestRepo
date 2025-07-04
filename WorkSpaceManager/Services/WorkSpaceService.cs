using Amazon.WorkSpaces;
using Amazon.WorkSpaces.Model;
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Data;
using WorkSpaceManager.Models;

namespace WorkSpaceManager.Services
{
    public class WorkSpaceService : IWorkSpaceService
    {
        private readonly WorkSpaceContext _context;
        private readonly IAmazonWorkSpaces _workSpacesClient;
        private readonly ILogger<WorkSpaceService> _logger;
        private readonly IConfiguration _configuration;

        public WorkSpaceService(
            WorkSpaceContext context,
            IAmazonWorkSpaces workSpacesClient,
            ILogger<WorkSpaceService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _workSpacesClient = workSpacesClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<string>> GetAvailableRegionsAsync()
        {
            // Common AWS regions where WorkSpaces is available
            return new List<string>
            {
                "us-east-1",
                "us-west-2",
                "ap-south-1",
                "ap-northeast-1",
                "ap-northeast-2",
                "ap-southeast-1",
                "ap-southeast-2",
                "ca-central-1",
                "eu-central-1",
                "eu-west-1",
                "eu-west-2",
                "sa-east-1"
            };
        }

        public async Task<List<BundleDto>> GetAvailableBundlesAsync(string region)
        {
            try
            {
                // First, check local cache
                var cachedBundles = await _context.Bundles
                    .Where(b => b.Region == region && b.IsActive)
                    .ToListAsync();

                if (cachedBundles.Any() && cachedBundles.First().LastUpdated > DateTime.UtcNow.AddHours(-1))
                {
                    return cachedBundles.Select(b => new BundleDto
                    {
                        BundleId = b.BundleId,
                        Name = b.Name,
                        Description = b.Description,
                        ComputeTypeName = b.ComputeTypeName,
                        RootStorageCapacityGb = b.RootStorageCapacityGb,
                        UserStorageCapacityGb = b.UserStorageCapacityGb,
                        Owner = b.Owner,
                        Region = b.Region
                    }).ToList();
                }

                // Fetch from AWS and update cache
                await SyncBundlesFromAWSAsync(region);

                return await _context.Bundles
                    .Where(b => b.Region == region && b.IsActive)
                    .Select(b => new BundleDto
                    {
                        BundleId = b.BundleId,
                        Name = b.Name,
                        Description = b.Description,
                        ComputeTypeName = b.ComputeTypeName,
                        RootStorageCapacityGb = b.RootStorageCapacityGb,
                        UserStorageCapacityGb = b.UserStorageCapacityGb,
                        Owner = b.Owner,
                        Region = b.Region
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bundles for region {Region}", region);
                return new List<BundleDto>();
            }
        }

        public async Task<List<DirectoryDto>> GetAvailableDirectoriesAsync(string region)
        {
            try
            {
                // First, check local cache
                var cachedDirectories = await _context.Directories
                    .Where(d => d.Region == region && d.IsActive)
                    .ToListAsync();

                if (cachedDirectories.Any() && cachedDirectories.First().LastUpdated > DateTime.UtcNow.AddHours(-1))
                {
                    return cachedDirectories.Select(d => new DirectoryDto
                    {
                        DirectoryId = d.DirectoryId,
                        DirectoryName = d.DirectoryName,
                        DirectoryType = d.DirectoryType,
                        State = d.State,
                        RegistrationCode = d.RegistrationCode,
                        Region = d.Region
                    }).ToList();
                }

                // Fetch from AWS and update cache
                await SyncDirectoriesFromAWSAsync(region);

                return await _context.Directories
                    .Where(d => d.Region == region && d.IsActive)
                    .Select(d => new DirectoryDto
                    {
                        DirectoryId = d.DirectoryId,
                        DirectoryName = d.DirectoryName,
                        DirectoryType = d.DirectoryType,
                        State = d.State,
                        RegistrationCode = d.RegistrationCode,
                        Region = d.Region
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching directories for region {Region}", region);
                return new List<DirectoryDto>();
            }
        }

        public async Task<WorkSpaceOperationResult> CreateWorkSpacesAsync(CreateWorkSpaceRequestViewModel request, string userId)
        {
            var result = new WorkSpaceOperationResult();

            try
            {
                // Create request record
                var requestEntity = new WorkSpaceRequestEntity
                {
                    RequestName = request.RequestName,
                    NumberOfWorkSpaces = request.NumberOfWorkSpaces,
                    Region = request.Region,
                    BundleId = request.BundleId,
                    DirectoryId = request.DirectoryId,
                    UserNamePrefix = request.UserNamePrefix,
                    RequestStatus = "Processing",
                    RequestedBy = userId,
                    Notes = request.Notes,
                    RootVolumeEncrypted = request.RootVolumeEncrypted,
                    UserVolumeEncrypted = request.UserVolumeEncrypted,
                    EncryptionKey = request.EncryptionKey
                };

                _context.WorkSpaceRequests.Add(requestEntity);
                await _context.SaveChangesAsync();

                // Get bundle and directory information
                var bundle = await _context.Bundles.FirstOrDefaultAsync(b => b.BundleId == request.BundleId);
                var directory = await _context.Directories.FirstOrDefaultAsync(d => d.DirectoryId == request.DirectoryId);

                if (bundle != null)
                {
                    requestEntity.BundleName = bundle.Name;
                }
                if (directory != null)
                {
                    requestEntity.DirectoryName = directory.DirectoryName;
                }

                // Create WorkSpaces
                var workSpaceRequests = new List<WorkspaceRequest>();
                for (int i = 1; i <= request.NumberOfWorkSpaces; i++)
                {
                    var username = $"{request.UserNamePrefix}{i:D3}";
                    
                    var workSpaceRequest = new WorkspaceRequest
                    {
                        DirectoryId = request.DirectoryId,
                        UserName = username,
                        BundleId = request.BundleId,
                        RootVolumeEncryptionEnabled = request.RootVolumeEncrypted,
                        UserVolumeEncryptionEnabled = request.UserVolumeEncrypted
                    };

                    if (!string.IsNullOrEmpty(request.EncryptionKey))
                    {
                        workSpaceRequest.VolumeEncryptionKey = request.EncryptionKey;
                    }

                    workSpaceRequests.Add(workSpaceRequest);
                }

                // Submit to AWS
                var createRequest = new CreateWorkspacesRequest
                {
                    Workspaces = workSpaceRequests
                };

                var response = await _workSpacesClient.CreateWorkspacesAsync(createRequest);

                // Process results
                foreach (var pendingRequest in response.PendingRequests)
                {
                    var workSpaceEntity = new WorkSpaceEntity
                    {
                        WorkSpaceId = pendingRequest.WorkspaceId,
                        UserName = pendingRequest.UserName,
                        BundleId = request.BundleId,
                        BundleName = bundle?.Name ?? "",
                        DirectoryId = request.DirectoryId,
                        Region = request.Region,
                        State = pendingRequest.State.Value,
                        RootVolumeEncrypted = request.RootVolumeEncrypted,
                        UserVolumeEncrypted = request.UserVolumeEncrypted,
                        CreatedBy = userId,
                        WorkSpaceRequestId = requestEntity.Id
                    };

                    _context.WorkSpaces.Add(workSpaceEntity);
                    requestEntity.SuccessfulCreations++;
                }

                foreach (var failedRequest in response.FailedRequests)
                {
                    var workSpaceEntity = new WorkSpaceEntity
                    {
                        WorkSpaceId = "",
                        UserName = failedRequest.WorkspaceRequest.UserName,
                        BundleId = request.BundleId,
                        BundleName = bundle?.Name ?? "",
                        DirectoryId = request.DirectoryId,
                        Region = request.Region,
                        State = "FAILED",
                        ErrorCode = failedRequest.ErrorCode,
                        ErrorMessage = failedRequest.ErrorMessage,
                        RootVolumeEncrypted = request.RootVolumeEncrypted,
                        UserVolumeEncrypted = request.UserVolumeEncrypted,
                        CreatedBy = userId,
                        WorkSpaceRequestId = requestEntity.Id
                    };

                    _context.WorkSpaces.Add(workSpaceEntity);
                    requestEntity.FailedCreations++;
                }

                // Update request status
                if (response.FailedRequests.Any() && !response.PendingRequests.Any())
                {
                    requestEntity.RequestStatus = "Failed";
                }
                else if (response.FailedRequests.Any())
                {
                    requestEntity.RequestStatus = "Partial";
                }
                else
                {
                    requestEntity.RequestStatus = "Completed";
                }

                requestEntity.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.Message = $"WorkSpace creation request submitted. {response.PendingRequests.Count} successful, {response.FailedRequests.Count} failed.";
                result.Data = new { RequestId = requestEntity.Id, PendingCount = response.PendingRequests.Count, FailedCount = response.FailedRequests.Count };

                if (response.FailedRequests.Any())
                {
                    result.Errors = response.FailedRequests.Select(f => $"{f.WorkspaceRequest.UserName}: {f.ErrorMessage}").ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WorkSpaces");
                result.Success = false;
                result.Message = "Failed to create WorkSpaces: " + ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<List<WorkSpaceDto>> GetWorkSpacesAsync(string? region = null, string? state = null, string? userId = null, string? bundleId = null)
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

            var entities = await query.OrderByDescending(w => w.CreatedAt).ToListAsync();

            return entities.Select(w => new WorkSpaceDto
            {
                Id = w.Id,
                WorkSpaceId = w.WorkSpaceId,
                UserName = w.UserName,
                BundleId = w.BundleId,
                BundleName = w.BundleName,
                DirectoryId = w.DirectoryId,
                Region = w.Region,
                State = w.State,
                IpAddress = w.IpAddress,
                ComputerName = w.ComputerName,
                ComputeType = w.ComputeType,
                RunningMode = w.RunningMode,
                RootVolumeSizeGb = w.RootVolumeSizeGb,
                UserVolumeSizeGb = w.UserVolumeSizeGb,
                RootVolumeEncrypted = w.RootVolumeEncrypted,
                UserVolumeEncrypted = w.UserVolumeEncrypted,
                ErrorMessage = w.ErrorMessage,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt,
                CreatedBy = w.CreatedBy
            }).ToList();
        }

        public async Task<WorkSpaceDto?> GetWorkSpaceAsync(string workSpaceId)
        {
            var entity = await _context.WorkSpaces.FirstOrDefaultAsync(w => w.WorkSpaceId == workSpaceId);
            
            if (entity == null)
                return null;

            return new WorkSpaceDto
            {
                Id = entity.Id,
                WorkSpaceId = entity.WorkSpaceId,
                UserName = entity.UserName,
                BundleId = entity.BundleId,
                BundleName = entity.BundleName,
                DirectoryId = entity.DirectoryId,
                Region = entity.Region,
                State = entity.State,
                IpAddress = entity.IpAddress,
                ComputerName = entity.ComputerName,
                ComputeType = entity.ComputeType,
                RunningMode = entity.RunningMode,
                RootVolumeSizeGb = entity.RootVolumeSizeGb,
                UserVolumeSizeGb = entity.UserVolumeSizeGb,
                RootVolumeEncrypted = entity.RootVolumeEncrypted,
                UserVolumeEncrypted = entity.UserVolumeEncrypted,
                ErrorMessage = entity.ErrorMessage,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy
            };
        }

        public async Task<WorkSpaceOperationResult> StartWorkSpaceAsync(string workSpaceId)
        {
            var result = new WorkSpaceOperationResult();

            try
            {
                var request = new StartWorkspacesRequest
                {
                    StartWorkspaceRequests = new List<StartRequest>
                    {
                        new StartRequest { WorkspaceId = workSpaceId }
                    }
                };

                var response = await _workSpacesClient.StartWorkspacesAsync(request);

                if (response.FailedRequests.Any())
                {
                    var error = response.FailedRequests.First();
                    result.Success = false;
                    result.Message = error.ErrorMessage;
                    result.Errors.Add(error.ErrorMessage);
                }
                else
                {
                    result.Success = true;
                    result.Message = "WorkSpace start initiated successfully";
                    
                    // Update local database
                    var workSpace = await _context.WorkSpaces.FirstOrDefaultAsync(w => w.WorkSpaceId == workSpaceId);
                    if (workSpace != null)
                    {
                        workSpace.State = "STARTING";
                        workSpace.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting WorkSpace {WorkSpaceId}", workSpaceId);
                result.Success = false;
                result.Message = "Failed to start WorkSpace: " + ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<WorkSpaceOperationResult> StopWorkSpaceAsync(string workSpaceId)
        {
            var result = new WorkSpaceOperationResult();

            try
            {
                var request = new StopWorkspacesRequest
                {
                    StopWorkspaceRequests = new List<StopRequest>
                    {
                        new StopRequest { WorkspaceId = workSpaceId }
                    }
                };

                var response = await _workSpacesClient.StopWorkspacesAsync(request);

                if (response.FailedRequests.Any())
                {
                    var error = response.FailedRequests.First();
                    result.Success = false;
                    result.Message = error.ErrorMessage;
                    result.Errors.Add(error.ErrorMessage);
                }
                else
                {
                    result.Success = true;
                    result.Message = "WorkSpace stop initiated successfully";
                    
                    // Update local database
                    var workSpace = await _context.WorkSpaces.FirstOrDefaultAsync(w => w.WorkSpaceId == workSpaceId);
                    if (workSpace != null)
                    {
                        workSpace.State = "STOPPING";
                        workSpace.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping WorkSpace {WorkSpaceId}", workSpaceId);
                result.Success = false;
                result.Message = "Failed to stop WorkSpace: " + ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<WorkSpaceOperationResult> RebootWorkSpaceAsync(string workSpaceId)
        {
            var result = new WorkSpaceOperationResult();

            try
            {
                var request = new RebootWorkspacesRequest
                {
                    RebootWorkspaceRequests = new List<RebootRequest>
                    {
                        new RebootRequest { WorkspaceId = workSpaceId }
                    }
                };

                var response = await _workSpacesClient.RebootWorkspacesAsync(request);

                if (response.FailedRequests.Any())
                {
                    var error = response.FailedRequests.First();
                    result.Success = false;
                    result.Message = error.ErrorMessage;
                    result.Errors.Add(error.ErrorMessage);
                }
                else
                {
                    result.Success = true;
                    result.Message = "WorkSpace reboot initiated successfully";
                    
                    // Update local database
                    var workSpace = await _context.WorkSpaces.FirstOrDefaultAsync(w => w.WorkSpaceId == workSpaceId);
                    if (workSpace != null)
                    {
                        workSpace.State = "REBOOTING";
                        workSpace.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebooting WorkSpace {WorkSpaceId}", workSpaceId);
                result.Success = false;
                result.Message = "Failed to reboot WorkSpace: " + ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<WorkSpaceOperationResult> TerminateWorkSpaceAsync(string workSpaceId)
        {
            var result = new WorkSpaceOperationResult();

            try
            {
                var request = new TerminateWorkspacesRequest
                {
                    TerminateWorkspaceRequests = new List<TerminateRequest>
                    {
                        new TerminateRequest { WorkspaceId = workSpaceId }
                    }
                };

                var response = await _workSpacesClient.TerminateWorkspacesAsync(request);

                if (response.FailedRequests.Any())
                {
                    var error = response.FailedRequests.First();
                    result.Success = false;
                    result.Message = error.ErrorMessage;
                    result.Errors.Add(error.ErrorMessage);
                }
                else
                {
                    result.Success = true;
                    result.Message = "WorkSpace termination initiated successfully";
                    
                    // Update local database
                    var workSpace = await _context.WorkSpaces.FirstOrDefaultAsync(w => w.WorkSpaceId == workSpaceId);
                    if (workSpace != null)
                    {
                        workSpace.State = "TERMINATING";
                        workSpace.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating WorkSpace {WorkSpaceId}", workSpaceId);
                result.Success = false;
                result.Message = "Failed to terminate WorkSpace: " + ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<WorkSpaceOperationResult> ModifyWorkSpaceAsync(string workSpaceId, string? runningMode = null, int? autoStopTimeout = null)
        {
            var result = new WorkSpaceOperationResult();

            try
            {
                var workspaceProperties = new WorkspaceProperties();
                
                if (!string.IsNullOrEmpty(runningMode))
                {
                    workspaceProperties.RunningMode = runningMode;
                }

                if (autoStopTimeout.HasValue)
                {
                    workspaceProperties.RunningModeAutoStopTimeoutInMinutes = autoStopTimeout.Value;
                }

                var request = new ModifyWorkspacePropertiesRequest
                {
                    WorkspaceId = workSpaceId,
                    WorkspaceProperties = workspaceProperties
                };

                await _workSpacesClient.ModifyWorkspacePropertiesAsync(request);

                result.Success = true;
                result.Message = "WorkSpace properties modified successfully";
                
                // Update local database
                var workSpace = await _context.WorkSpaces.FirstOrDefaultAsync(w => w.WorkSpaceId == workSpaceId);
                if (workSpace != null)
                {
                    if (!string.IsNullOrEmpty(runningMode))
                        workSpace.RunningMode = runningMode;
                    
                    workSpace.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error modifying WorkSpace {WorkSpaceId}", workSpaceId);
                result.Success = false;
                result.Message = "Failed to modify WorkSpace: " + ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task SyncWorkSpacesFromAWSAsync(string region)
        {
            try
            {
                var request = new DescribeWorkspacesRequest();
                var response = await _workSpacesClient.DescribeWorkspacesAsync(request);

                foreach (var workspace in response.Workspaces)
                {
                    var existingWorkSpace = await _context.WorkSpaces
                        .FirstOrDefaultAsync(w => w.WorkSpaceId == workspace.WorkspaceId);

                    if (existingWorkSpace != null)
                    {
                        // Update existing
                        existingWorkSpace.State = workspace.State.Value;
                        existingWorkSpace.IpAddress = workspace.IpAddress;
                        existingWorkSpace.ComputerName = workspace.ComputerName;
                        existingWorkSpace.SubnetId = workspace.SubnetId;
                        existingWorkSpace.ComputeType = workspace.WorkspaceProperties?.ComputeTypeName?.Value;
                        existingWorkSpace.RunningMode = workspace.WorkspaceProperties?.RunningMode?.Value;
                        existingWorkSpace.RootVolumeSizeGb = workspace.WorkspaceProperties?.RootVolumeSizeGib;
                        existingWorkSpace.UserVolumeSizeGb = workspace.WorkspaceProperties?.UserVolumeSizeGib;
                        existingWorkSpace.ErrorMessage = workspace.ErrorMessage;
                        existingWorkSpace.ErrorCode = workspace.ErrorCode;
                        existingWorkSpace.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new
                        var newWorkSpace = new WorkSpaceEntity
                        {
                            WorkSpaceId = workspace.WorkspaceId,
                            UserName = workspace.UserName,
                            BundleId = workspace.BundleId,
                            DirectoryId = workspace.DirectoryId,
                            Region = region,
                            State = workspace.State.Value,
                            IpAddress = workspace.IpAddress,
                            ComputerName = workspace.ComputerName,
                            SubnetId = workspace.SubnetId,
                            ComputeType = workspace.WorkspaceProperties?.ComputeTypeName?.Value,
                            RunningMode = workspace.WorkspaceProperties?.RunningMode?.Value,
                            RootVolumeSizeGb = workspace.WorkspaceProperties?.RootVolumeSizeGib,
                            UserVolumeSizeGb = workspace.WorkspaceProperties?.UserVolumeSizeGib,
                            RootVolumeEncrypted = workspace.RootVolumeEncryptionEnabled ?? false,
                            UserVolumeEncrypted = workspace.UserVolumeEncryptionEnabled ?? false,
                            ErrorMessage = workspace.ErrorMessage,
                            ErrorCode = workspace.ErrorCode,
                            CreatedBy = "System"
                        };

                        _context.WorkSpaces.Add(newWorkSpace);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing WorkSpaces from AWS for region {Region}", region);
            }
        }

        public async Task SyncBundlesFromAWSAsync(string region)
        {
            try
            {
                var request = new DescribeWorkspaceBundlesRequest();
                var response = await _workSpacesClient.DescribeWorkspaceBundlesAsync(request);

                foreach (var bundle in response.Bundles)
                {
                    var existingBundle = await _context.Bundles
                        .FirstOrDefaultAsync(b => b.BundleId == bundle.BundleId);

                    if (existingBundle != null)
                    {
                        // Update existing
                        existingBundle.Name = bundle.Name;
                        existingBundle.Description = bundle.Description;
                        existingBundle.ComputeTypeName = bundle.ComputeType?.Name;
                        existingBundle.RootStorageCapacityGb = bundle.RootStorage?.Capacity;
                        existingBundle.UserStorageCapacityGb = bundle.UserStorage?.Capacity;
                        existingBundle.Owner = bundle.Owner;
                        existingBundle.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new
                        var newBundle = new BundleEntity
                        {
                            BundleId = bundle.BundleId,
                            Name = bundle.Name,
                            Description = bundle.Description,
                            ComputeTypeName = bundle.ComputeType?.Name,
                            RootStorageCapacityGb = bundle.RootStorage?.Capacity,
                            UserStorageCapacityGb = bundle.UserStorage?.Capacity,
                            Owner = bundle.Owner,
                            Region = region
                        };

                        _context.Bundles.Add(newBundle);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing bundles from AWS for region {Region}", region);
            }
        }

        public async Task SyncDirectoriesFromAWSAsync(string region)
        {
            try
            {
                var request = new DescribeWorkspaceDirectoriesRequest();
                var response = await _workSpacesClient.DescribeWorkspaceDirectoriesAsync(request);

                foreach (var directory in response.Directories)
                {
                    var existingDirectory = await _context.Directories
                        .FirstOrDefaultAsync(d => d.DirectoryId == directory.DirectoryId);

                    if (existingDirectory != null)
                    {
                        // Update existing
                        existingDirectory.DirectoryName = directory.DirectoryName;
                        existingDirectory.DirectoryType = directory.DirectoryType?.Value;
                        existingDirectory.State = directory.State?.Value;
                        existingDirectory.RegistrationCode = directory.RegistrationCode;
                        existingDirectory.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new
                        var newDirectory = new DirectoryEntity
                        {
                            DirectoryId = directory.DirectoryId,
                            DirectoryName = directory.DirectoryName,
                            DirectoryType = directory.DirectoryType?.Value ?? "",
                            State = directory.State?.Value ?? "",
                            RegistrationCode = directory.RegistrationCode,
                            Region = region
                        };

                        _context.Directories.Add(newDirectory);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing directories from AWS for region {Region}", region);
            }
        }
    }
}