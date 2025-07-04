using System.ComponentModel.DataAnnotations;

namespace WorkSpaceManager.Models
{
    public class CreateWorkSpaceRequestViewModel
    {
        [Required]
        [Display(Name = "Request Name")]
        [StringLength(100, ErrorMessage = "Request name cannot exceed 100 characters.")]
        public string RequestName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Number of WorkSpaces")]
        [Range(1, 100, ErrorMessage = "Number of WorkSpaces must be between 1 and 100.")]
        public int NumberOfWorkSpaces { get; set; } = 1;

        [Required]
        [Display(Name = "AWS Region")]
        public string Region { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bundle")]
        public string BundleId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Directory")]
        public string DirectoryId { get; set; } = string.Empty;

        [Display(Name = "Username Prefix")]
        [StringLength(50, ErrorMessage = "Username prefix cannot exceed 50 characters.")]
        public string UserNamePrefix { get; set; } = "user";

        [Display(Name = "Enable Root Volume Encryption")]
        public bool RootVolumeEncrypted { get; set; } = true;

        [Display(Name = "Enable User Volume Encryption")]
        public bool UserVolumeEncrypted { get; set; } = true;

        [Display(Name = "Encryption Key (Optional)")]
        [StringLength(100)]
        public string? EncryptionKey { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Available options for dropdowns
        public List<BundleDto> AvailableBundles { get; set; } = new();
        public List<DirectoryDto> AvailableDirectories { get; set; } = new();
        public List<string> AvailableRegions { get; set; } = new();
    }

    public class WorkSpaceListViewModel
    {
        public List<WorkSpaceDto> WorkSpaces { get; set; } = new();
        public string? FilterRegion { get; set; }
        public string? FilterState { get; set; }
        public string? FilterUser { get; set; }
        public string? FilterBundle { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Available filter options
        public List<string> AvailableRegions { get; set; } = new();
        public List<string> AvailableStates { get; set; } = new();
        public List<BundleDto> AvailableBundles { get; set; } = new();
    }

    public class WorkSpaceRequestListViewModel
    {
        public List<WorkSpaceRequestDto> Requests { get; set; } = new();
        public string? FilterStatus { get; set; }
        public string? FilterRegion { get; set; }
        public DateTime? FilterDateFrom { get; set; }
        public DateTime? FilterDateTo { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class DashboardViewModel
    {
        public int TotalWorkSpaces { get; set; }
        public int RunningWorkSpaces { get; set; }
        public int StoppedWorkSpaces { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequestsToday { get; set; }
        public int FailedCreationsToday { get; set; }

        public List<WorkSpacesByRegionDto> WorkSpacesByRegion { get; set; } = new();
        public List<WorkSpacesByStateDto> WorkSpacesByState { get; set; } = new();
        public List<WorkSpacesByBundleDto> WorkSpacesByBundle { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
    }

    public class ReportsViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Region { get; set; }
        public string? Status { get; set; }

        public List<WorkSpaceReportDto> WorkSpaceReport { get; set; } = new();
        public List<RequestReportDto> RequestReport { get; set; } = new();
        public List<string> AvailableRegions { get; set; } = new();
    }

    // DTOs for data transfer
    public class WorkSpaceDto
    {
        public int Id { get; set; }
        public string WorkSpaceId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string BundleId { get; set; } = string.Empty;
        public string BundleName { get; set; } = string.Empty;
        public string DirectoryId { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? ComputerName { get; set; }
        public string? ComputeType { get; set; }
        public string? RunningMode { get; set; }
        public int? RootVolumeSizeGb { get; set; }
        public int? UserVolumeSizeGb { get; set; }
        public bool RootVolumeEncrypted { get; set; }
        public bool UserVolumeEncrypted { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class WorkSpaceRequestDto
    {
        public int Id { get; set; }
        public string RequestName { get; set; } = string.Empty;
        public int NumberOfWorkSpaces { get; set; }
        public string Region { get; set; } = string.Empty;
        public string BundleId { get; set; } = string.Empty;
        public string BundleName { get; set; } = string.Empty;
        public string DirectoryId { get; set; } = string.Empty;
        public string DirectoryName { get; set; } = string.Empty;
        public string UserNamePrefix { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int SuccessfulCreations { get; set; }
        public int FailedCreations { get; set; }
        public List<WorkSpaceDto> WorkSpaces { get; set; } = new();
    }

    public class BundleDto
    {
        public string BundleId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ComputeTypeName { get; set; }
        public int? RootStorageCapacityGb { get; set; }
        public int? UserStorageCapacityGb { get; set; }
        public string? Owner { get; set; }
        public string? Region { get; set; }
    }

    public class DirectoryDto
    {
        public string DirectoryId { get; set; } = string.Empty;
        public string? DirectoryName { get; set; }
        public string DirectoryType { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? RegistrationCode { get; set; }
        public string? Region { get; set; }
    }

    public class WorkSpacesByRegionDto
    {
        public string Region { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class WorkSpacesByStateDto
    {
        public string State { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class WorkSpacesByBundleDto
    {
        public string BundleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecentActivityDto
    {
        public string Activity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string User { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class WorkSpaceReportDto
    {
        public string Region { get; set; } = string.Empty;
        public string BundleName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RunningCount { get; set; }
        public int StoppedCount { get; set; }
        public int ErrorCount { get; set; }
        public decimal TotalCostEstimate { get; set; }
    }

    public class RequestReportDto
    {
        public DateTime Date { get; set; }
        public int TotalRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int FailedRequests { get; set; }
        public int PendingRequests { get; set; }
        public int TotalWorkSpacesCreated { get; set; }
        public int SuccessfulCreations { get; set; }
        public int FailedCreations { get; set; }
    }

    public class WorkSpaceOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public object? Data { get; set; }
    }
}