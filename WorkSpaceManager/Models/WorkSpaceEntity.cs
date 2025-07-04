using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkSpaceManager.Models
{
    public class WorkSpaceEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string WorkSpaceId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BundleId { get; set; } = string.Empty;

        [StringLength(100)]
        public string BundleName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DirectoryId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Region { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? ComputerName { get; set; }

        [StringLength(50)]
        public string? SubnetId { get; set; }

        [StringLength(50)]
        public string? ComputeType { get; set; }

        [StringLength(50)]
        public string? RunningMode { get; set; }

        public int? RootVolumeSizeGb { get; set; }

        public int? UserVolumeSizeGb { get; set; }

        public bool RootVolumeEncrypted { get; set; }

        public bool UserVolumeEncrypted { get; set; }

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        [StringLength(50)]
        public string? ErrorCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Tags { get; set; }

        // Navigation property for creation request
        public int? WorkSpaceRequestId { get; set; }
        public virtual WorkSpaceRequestEntity? WorkSpaceRequest { get; set; }
    }

    public class WorkSpaceRequestEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestName { get; set; } = string.Empty;

        [Required]
        public int NumberOfWorkSpaces { get; set; }

        [Required]
        [StringLength(50)]
        public string Region { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BundleId { get; set; } = string.Empty;

        [StringLength(100)]
        public string BundleName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DirectoryId { get; set; } = string.Empty;

        [StringLength(100)]
        public string DirectoryName { get; set; } = string.Empty;

        [StringLength(100)]
        public string UserNamePrefix { get; set; } = string.Empty;

        [StringLength(50)]
        public string RequestStatus { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [StringLength(100)]
        public string RequestedBy { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool RootVolumeEncrypted { get; set; } = true;

        public bool UserVolumeEncrypted { get; set; } = true;

        [StringLength(100)]
        public string? EncryptionKey { get; set; }

        public int SuccessfulCreations { get; set; } = 0;

        public int FailedCreations { get; set; } = 0;

        // Navigation property
        public virtual ICollection<WorkSpaceEntity> WorkSpaces { get; set; } = new List<WorkSpaceEntity>();
    }

    public class BundleEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BundleId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? ComputeTypeName { get; set; }

        public int? RootStorageCapacityGb { get; set; }

        public int? UserStorageCapacityGb { get; set; }

        [StringLength(50)]
        public string? Owner { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? Region { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DirectoryEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DirectoryId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DirectoryName { get; set; }

        [Required]
        [StringLength(50)]
        public string DirectoryType { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RegistrationCode { get; set; }

        [StringLength(50)]
        public string? Region { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}