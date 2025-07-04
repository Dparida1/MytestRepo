using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Amazon;
using Amazon.WorkSpaces;
using Amazon.WorkSpaces.Model;
using Amazon.DirectoryService;
using Amazon.DirectoryService.Model;

namespace WorkSpaceManager
{
    public class AWSWorkSpaceService
    {
        private readonly string awsRegion;
        private readonly string awsProfile;
        private readonly DatabaseHelper database;

        public AWSWorkSpaceService()
        {
            awsRegion = ConfigurationManager.AppSettings["AWS_Region"] ?? "us-east-1";
            awsProfile = ConfigurationManager.AppSettings["AWS_Profile"] ?? "default";
            database = new DatabaseHelper();
        }

        public OperationResult SyncBundlesFromAWS(string region = null)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(region ?? awsRegion);
                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var request = new DescribeWorkspaceBundlesRequest();
                    var response = client.DescribeWorkspaceBundles(request);

                    foreach (var awsBundle in response.Bundles)
                    {
                        var bundle = new Bundle
                        {
                            BundleId = awsBundle.BundleId,
                            Name = awsBundle.Name,
                            Description = awsBundle.Description,
                            ImageId = awsBundle.ImageId,
                            ComputeType = awsBundle.ComputeType?.Name,
                            RootStorage = awsBundle.RootStorage?.Capacity,
                            UserStorage = awsBundle.UserStorage?.Capacity,
                            Region = region ?? awsRegion,
                            LastUpdated = DateTime.Now
                        };

                        database.SaveBundle(bundle);
                    }

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Successfully synced {response.Bundles.Count} bundles from region {region ?? awsRegion}",
                        Data = response.Bundles.Count
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to sync bundles from AWS",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult SyncDirectoriesFromAWS(string region = null)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(region ?? awsRegion);
                using (var dsClient = new AmazonDirectoryServiceClient(regionEndpoint))
                {
                    var request = new DescribeDirectoriesRequest();
                    var response = dsClient.DescribeDirectories(request);

                    foreach (var awsDirectory in response.DirectoryDescriptions)
                    {
                        var directory = new Directory
                        {
                            DirectoryId = awsDirectory.DirectoryId,
                            Name = awsDirectory.Name,
                            DirectoryType = awsDirectory.Type,
                            State = awsDirectory.Stage,
                            Region = region ?? awsRegion,
                            SubnetIds = awsDirectory.VpcSettings?.SubnetIds ?? new List<string>(),
                            LastUpdated = DateTime.Now
                        };

                        database.SaveDirectory(directory);
                    }

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Successfully synced {response.DirectoryDescriptions.Count} directories from region {region ?? awsRegion}",
                        Data = response.DirectoryDescriptions.Count
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to sync directories from AWS",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult CreateWorkSpaces(CreateWorkSpaceViewModel model)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var regionEndpoint = RegionEndpoint.GetBySystemName(model.Region);

                // Save the request to database first
                var workSpaceRequest = new WorkSpaceRequest
                {
                    RequestId = requestId,
                    RequestName = model.RequestName,
                    RequestedCount = model.WorkSpaceCount,
                    CreatedCount = 0,
                    FailedCount = 0,
                    Region = model.Region,
                    BundleId = model.BundleId,
                    DirectoryId = model.DirectoryId,
                    UsernamePrefix = model.UsernamePrefix,
                    Status = "PENDING",
                    CreatedDate = DateTime.Now
                };

                database.SaveWorkSpaceRequest(workSpaceRequest);

                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var workSpaceRequests = new List<WorkspaceRequest>();

                    // Create individual workspace requests
                    for (int i = 1; i <= model.WorkSpaceCount; i++)
                    {
                        var username = $"{model.UsernamePrefix}{i:D3}"; // e.g., user001, user002
                        
                        var workspaceRequest = new WorkspaceRequest
                        {
                            DirectoryId = model.DirectoryId,
                            UserName = username,
                            BundleId = model.BundleId,
                            VolumeEncryptionKey = null, // Use default encryption
                            UserVolumeEncryptionEnabled = model.UserVolumeEncryptionEnabled,
                            RootVolumeEncryptionEnabled = model.VolumeEncryptionEnabled,
                            WorkspaceProperties = new WorkspaceProperties
                            {
                                RunningMode = model.RunningMode == "AUTO_STOP" ? RunningMode.AUTO_STOP : RunningMode.ALWAYS_ON,
                                RunningModeAutoStopTimeoutInMinutes = model.RunningMode == "AUTO_STOP" ? model.RunningModeAutoStopTimeoutInMinutes : (int?)null
                            }
                        };

                        workSpaceRequests.Add(workspaceRequest);
                    }

                    // Submit to AWS in batches (AWS limit is 25 per request)
                    var batchSize = 25;
                    var createdCount = 0;
                    var failedCount = 0;
                    var errorMessages = new List<string>();

                    for (int i = 0; i < workSpaceRequests.Count; i += batchSize)
                    {
                        var batch = workSpaceRequests.Skip(i).Take(batchSize).ToList();
                        
                        var createRequest = new CreateWorkspacesRequest
                        {
                            Workspaces = batch
                        };

                        var response = client.CreateWorkspaces(createRequest);

                        // Process successful WorkSpaces
                        foreach (var pendingWorkSpace in response.PendingRequests)
                        {
                            var workSpace = new WorkSpace
                            {
                                WorkSpaceId = pendingWorkSpace.WorkspaceId,
                                DirectoryId = pendingWorkSpace.DirectoryId,
                                UserName = pendingWorkSpace.UserName,
                                BundleId = pendingWorkSpace.BundleId,
                                State = pendingWorkSpace.State,
                                SubnetId = pendingWorkSpace.SubnetId,
                                IpAddress = pendingWorkSpace.IpAddress,
                                ComputerName = pendingWorkSpace.ComputerName,
                                Region = model.Region,
                                CreatedDate = DateTime.Now,
                                RequestId = requestId
                            };

                            database.SaveWorkSpace(workSpace);
                            createdCount++;
                        }

                        // Process failed WorkSpaces
                        foreach (var failedWorkSpace in response.FailedRequests)
                        {
                            var workSpace = new WorkSpace
                            {
                                WorkSpaceId = null,
                                DirectoryId = model.DirectoryId,
                                UserName = failedWorkSpace.WorkspaceRequest?.UserName,
                                BundleId = model.BundleId,
                                State = "ERROR",
                                Region = model.Region,
                                CreatedDate = DateTime.Now,
                                RequestId = requestId,
                                ErrorCode = failedWorkSpace.ErrorCode,
                                ErrorMessage = failedWorkSpace.ErrorMessage
                            };

                            database.SaveWorkSpace(workSpace);
                            failedCount++;
                            errorMessages.Add($"User {failedWorkSpace.WorkspaceRequest?.UserName}: {failedWorkSpace.ErrorMessage}");
                        }
                    }

                    // Update the request with final counts
                    UpdateWorkSpaceRequest(requestId, createdCount, failedCount, errorMessages);

                    var message = $"WorkSpace creation completed. Created: {createdCount}, Failed: {failedCount}";
                    if (errorMessages.Any())
                    {
                        message += $". Errors: {string.Join("; ", errorMessages.Take(3))}";
                        if (errorMessages.Count > 3)
                        {
                            message += $" and {errorMessages.Count - 3} more...";
                        }
                    }

                    return new OperationResult
                    {
                        Success = createdCount > 0,
                        Message = message,
                        Data = new { CreatedCount = createdCount, FailedCount = failedCount, RequestId = requestId }
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to create WorkSpaces",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult SyncWorkSpacesFromAWS(string region = null)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(region ?? awsRegion);
                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var request = new DescribeWorkspacesRequest();
                    var response = client.DescribeWorkspaces(request);

                    var syncedCount = 0;

                    foreach (var awsWorkSpace in response.Workspaces)
                    {
                        // Check if WorkSpace already exists in database
                        var existingWorkSpaces = database.GetAllWorkSpaces();
                        var existingWorkSpace = existingWorkSpaces.FirstOrDefault(w => w.WorkSpaceId == awsWorkSpace.WorkspaceId);

                        if (existingWorkSpace == null)
                        {
                            // Create new WorkSpace record
                            var workSpace = new WorkSpace
                            {
                                WorkSpaceId = awsWorkSpace.WorkspaceId,
                                DirectoryId = awsWorkSpace.DirectoryId,
                                UserName = awsWorkSpace.UserName,
                                BundleId = awsWorkSpace.BundleId,
                                State = awsWorkSpace.State,
                                SubnetId = awsWorkSpace.SubnetId,
                                IpAddress = awsWorkSpace.IpAddress,
                                ComputerName = awsWorkSpace.ComputerName,
                                Region = region ?? awsRegion,
                                CreatedDate = DateTime.Now,
                                LastUpdated = DateTime.Now
                            };

                            database.SaveWorkSpace(workSpace);
                            syncedCount++;
                        }
                        else
                        {
                            // Update existing WorkSpace (you might want to implement update logic)
                            // For now, we'll just count it as synced
                            syncedCount++;
                        }
                    }

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Successfully synced {syncedCount} WorkSpaces from region {region ?? awsRegion}",
                        Data = syncedCount
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to sync WorkSpaces from AWS",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult StartWorkSpaces(List<string> workSpaceIds)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var request = new StartWorkspacesRequest
                    {
                        StartWorkspaceRequests = workSpaceIds.Select(id => new StartRequest { WorkspaceId = id }).ToList()
                    };

                    var response = client.StartWorkspaces(request);

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Start request submitted for {workSpaceIds.Count} WorkSpaces",
                        Data = response
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to start WorkSpaces",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult StopWorkSpaces(List<string> workSpaceIds)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var request = new StopWorkspacesRequest
                    {
                        StopWorkspaceRequests = workSpaceIds.Select(id => new StopRequest { WorkspaceId = id }).ToList()
                    };

                    var response = client.StopWorkspaces(request);

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Stop request submitted for {workSpaceIds.Count} WorkSpaces",
                        Data = response
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to stop WorkSpaces",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult RebootWorkSpaces(List<string> workSpaceIds)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var request = new RebootWorkspacesRequest
                    {
                        RebootWorkspaceRequests = workSpaceIds.Select(id => new RebootRequest { WorkspaceId = id }).ToList()
                    };

                    var response = client.RebootWorkspaces(request);

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Reboot request submitted for {workSpaceIds.Count} WorkSpaces",
                        Data = response
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to reboot WorkSpaces",
                    ErrorDetails = ex.Message
                };
            }
        }

        public OperationResult TerminateWorkSpaces(List<string> workSpaceIds)
        {
            try
            {
                var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
                using (var client = new AmazonWorkSpacesClient(regionEndpoint))
                {
                    var request = new TerminateWorkspacesRequest
                    {
                        TerminateWorkspaceRequests = workSpaceIds.Select(id => new TerminateRequest { WorkspaceId = id }).ToList()
                    };

                    var response = client.TerminateWorkspaces(request);

                    return new OperationResult
                    {
                        Success = true,
                        Message = $"Terminate request submitted for {workSpaceIds.Count} WorkSpaces",
                        Data = response
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Failed to terminate WorkSpaces",
                    ErrorDetails = ex.Message
                };
            }
        }

        private void UpdateWorkSpaceRequest(string requestId, int createdCount, int failedCount, List<string> errorMessages)
        {
            try
            {
                // Note: This is a simplified update. In a real implementation, you'd want to update the existing record
                // For now, we'll just log the completion
                var status = createdCount > 0 ? "COMPLETED" : "FAILED";
                var errorMessage = errorMessages.Any() ? string.Join("; ", errorMessages) : null;

                // You would implement an update method in DatabaseHelper for this
                // database.UpdateWorkSpaceRequest(requestId, createdCount, failedCount, status, errorMessage);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the main operation
                System.Diagnostics.Debug.WriteLine($"Failed to update request {requestId}: {ex.Message}");
            }
        }

        // Helper method to save directory (missing from DatabaseHelper)
        private void SaveDirectory(Directory directory)
        {
            // This method should be added to DatabaseHelper, similar to SaveBundle
            // For now, we'll implement a basic version here
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                connection.Open();
                
                // Check if directory exists
                var checkSql = "SELECT COUNT(*) FROM Directories WHERE DirectoryId = @DirectoryId AND Region = @Region";
                using (var checkCommand = new System.Data.SqlClient.SqlCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@DirectoryId", directory.DirectoryId);
                    checkCommand.Parameters.AddWithValue("@Region", directory.Region);
                    var exists = (int)checkCommand.ExecuteScalar() > 0;

                    if (!exists)
                    {
                        var insertSql = @"
                        INSERT INTO Directories (DirectoryId, Name, DirectoryType, State, Region, SubnetIds, LastUpdated)
                        VALUES (@DirectoryId, @Name, @DirectoryType, @State, @Region, @SubnetIds, @LastUpdated)";
                        
                        using (var insertCommand = new System.Data.SqlClient.SqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@DirectoryId", directory.DirectoryId);
                            insertCommand.Parameters.AddWithValue("@Name", directory.Name ?? (object)System.DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@DirectoryType", directory.DirectoryType ?? (object)System.DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@State", directory.State ?? (object)System.DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@Region", directory.Region ?? (object)System.DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@SubnetIds", string.Join(",", directory.SubnetIds));
                            insertCommand.Parameters.AddWithValue("@LastUpdated", directory.LastUpdated);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}