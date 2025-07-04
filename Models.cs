using System;
using System.Collections.Generic;

namespace WorkSpaceManager
{
    // WorkSpace entity for database storage
    public class WorkSpace
    {
        public int Id { get; set; }
        public string WorkSpaceId { get; set; }
        public string DirectoryId { get; set; }
        public string UserName { get; set; }
        public string BundleId { get; set; }
        public string State { get; set; }  // PENDING, AVAILABLE, IMPAIRED, UNHEALTHY, REBOOTING, STARTING, REBUILDING, RESTORING, MAINTENANCE, ADMIN_MAINTENANCE, TERMINATING, TERMINATED, SUSPENDED, UPDATING, STOPPING, STOPPED, ERROR
        public string SubnetId { get; set; }
        public string IpAddress { get; set; }
        public string ComputerName { get; set; }
        public string Region { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string RequestId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    // WorkSpace Request for bulk creation tracking
    public class WorkSpaceRequest
    {
        public int Id { get; set; }
        public string RequestId { get; set; }
        public string RequestName { get; set; }
        public int RequestedCount { get; set; }
        public int CreatedCount { get; set; }
        public int FailedCount { get; set; }
        public string Region { get; set; }
        public string BundleId { get; set; }
        public string DirectoryId { get; set; }
        public string UsernamePrefix { get; set; }
        public string Status { get; set; }  // PENDING, IN_PROGRESS, COMPLETED, FAILED
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ErrorMessage { get; set; }
    }

    // Bundle information from AWS
    public class Bundle
    {
        public int Id { get; set; }
        public string BundleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageId { get; set; }
        public string ComputeType { get; set; }  // VALUE, STANDARD, PERFORMANCE, POWER, GRAPHICS, POWERPRO, GRAPHICSPRO
        public string RootStorage { get; set; }
        public string UserStorage { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? HourlyPrice { get; set; }
        public string Region { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // Directory information from AWS
    public class Directory
    {
        public int Id { get; set; }
        public string DirectoryId { get; set; }
        public string Name { get; set; }
        public string DirectoryType { get; set; }  // SimpleAD, MicrosoftAD, ADConnector
        public string State { get; set; }
        public string Region { get; set; }
        public List<string> SubnetIds { get; set; } = new List<string>();
        public DateTime LastUpdated { get; set; }
    }

    // View Models for Web Forms
    public class DashboardStats
    {
        public int TotalWorkSpaces { get; set; }
        public int RunningWorkSpaces { get; set; }
        public int StoppedWorkSpaces { get; set; }
        public int PendingWorkSpaces { get; set; }
        public int FailedWorkSpaces { get; set; }
        public List<WorkSpace> RecentWorkSpaces { get; set; } = new List<WorkSpace>();
        public List<WorkSpaceRequest> RecentRequests { get; set; } = new List<WorkSpaceRequest>();
    }

    public class CreateWorkSpaceViewModel
    {
        public string RequestName { get; set; }
        public int WorkSpaceCount { get; set; } = 1;
        public string Region { get; set; }
        public string BundleId { get; set; }
        public string DirectoryId { get; set; }
        public string UsernamePrefix { get; set; } = "user";
        public bool VolumeEncryptionEnabled { get; set; } = true;
        public bool UserVolumeEncryptionEnabled { get; set; } = true;
        public string RunningMode { get; set; } = "AUTO_STOP";
        public int RunningModeAutoStopTimeoutInMinutes { get; set; } = 60;
    }

    // Simple result class for operations
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
        public object Data { get; set; }
    }

    // AWS WorkSpace state mapping
    public static class WorkSpaceStates
    {
        public const string PENDING = "PENDING";
        public const string AVAILABLE = "AVAILABLE";
        public const string IMPAIRED = "IMPAIRED";
        public const string UNHEALTHY = "UNHEALTHY";
        public const string REBOOTING = "REBOOTING";
        public const string STARTING = "STARTING";
        public const string REBUILDING = "REBUILDING";
        public const string RESTORING = "RESTORING";
        public const string MAINTENANCE = "MAINTENANCE";
        public const string ADMIN_MAINTENANCE = "ADMIN_MAINTENANCE";
        public const string TERMINATING = "TERMINATING";
        public const string TERMINATED = "TERMINATED";
        public const string SUSPENDED = "SUSPENDED";
        public const string UPDATING = "UPDATING";
        public const string STOPPING = "STOPPING";
        public const string STOPPED = "STOPPED";
        public const string ERROR = "ERROR";

        public static string GetDisplayText(string state)
        {
            return state switch
            {
                PENDING => "Pending",
                AVAILABLE => "Available",
                IMPAIRED => "Impaired", 
                UNHEALTHY => "Unhealthy",
                REBOOTING => "Rebooting",
                STARTING => "Starting",
                REBUILDING => "Rebuilding",
                RESTORING => "Restoring",
                MAINTENANCE => "Maintenance",
                ADMIN_MAINTENANCE => "Admin Maintenance",
                TERMINATING => "Terminating",
                TERMINATED => "Terminated",
                SUSPENDED => "Suspended",
                UPDATING => "Updating",
                STOPPING => "Stopping",
                STOPPED => "Stopped",
                ERROR => "Error",
                _ => state
            };
        }

        public static string GetBootstrapClass(string state)
        {
            return state switch
            {
                AVAILABLE => "success",
                STOPPED => "secondary", 
                PENDING or STARTING or REBOOTING => "warning",
                ERROR or IMPAIRED or UNHEALTHY => "danger",
                TERMINATED => "dark",
                _ => "info"
            };
        }
    }
}