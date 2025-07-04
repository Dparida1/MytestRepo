using WorkSpaceManager.Models;

namespace WorkSpaceManager.Services
{
    public interface IWorkSpaceService
    {
        Task<List<BundleDto>> GetAvailableBundlesAsync(string region);
        Task<List<DirectoryDto>> GetAvailableDirectoriesAsync(string region);
        Task<List<string>> GetAvailableRegionsAsync();
        Task<WorkSpaceOperationResult> CreateWorkSpacesAsync(CreateWorkSpaceRequestViewModel request, string userId);
        Task<List<WorkSpaceDto>> GetWorkSpacesAsync(string? region = null, string? state = null, string? userId = null, string? bundleId = null);
        Task<WorkSpaceDto?> GetWorkSpaceAsync(string workSpaceId);
        Task<WorkSpaceOperationResult> StartWorkSpaceAsync(string workSpaceId);
        Task<WorkSpaceOperationResult> StopWorkSpaceAsync(string workSpaceId);
        Task<WorkSpaceOperationResult> RebootWorkSpaceAsync(string workSpaceId);
        Task<WorkSpaceOperationResult> TerminateWorkSpaceAsync(string workSpaceId);
        Task<WorkSpaceOperationResult> ModifyWorkSpaceAsync(string workSpaceId, string? runningMode = null, int? autoStopTimeout = null);
        Task SyncWorkSpacesFromAWSAsync(string region);
        Task SyncBundlesFromAWSAsync(string region);
        Task SyncDirectoriesFromAWSAsync(string region);
    }
}