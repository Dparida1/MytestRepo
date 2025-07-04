using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace WorkSpaceManager
{
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public void InitializeDatabase()
        {
            CreateTablesIfNotExists();
        }

        private void CreateTablesIfNotExists()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create WorkSpaces table
                var createWorkSpacesTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkSpaces' AND xtype='U')
                CREATE TABLE WorkSpaces (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    WorkSpaceId nvarchar(50) NOT NULL,
                    DirectoryId nvarchar(50),
                    UserName nvarchar(100),
                    BundleId nvarchar(50),
                    State nvarchar(50),
                    SubnetId nvarchar(50),
                    IpAddress nvarchar(15),
                    ComputerName nvarchar(100),
                    Region nvarchar(20),
                    CreatedDate datetime NOT NULL DEFAULT GETDATE(),
                    LastUpdated datetime,
                    RequestId nvarchar(50),
                    ErrorCode nvarchar(50),
                    ErrorMessage nvarchar(max)
                )";

                using (var command = new SqlCommand(createWorkSpacesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create WorkSpaceRequests table
                var createRequestsTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkSpaceRequests' AND xtype='U')
                CREATE TABLE WorkSpaceRequests (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    RequestId nvarchar(50) NOT NULL,
                    RequestName nvarchar(200),
                    RequestedCount int NOT NULL,
                    CreatedCount int DEFAULT 0,
                    FailedCount int DEFAULT 0,
                    Region nvarchar(20),
                    BundleId nvarchar(50),
                    DirectoryId nvarchar(50),
                    UsernamePrefix nvarchar(50),
                    Status nvarchar(20) DEFAULT 'PENDING',
                    CreatedDate datetime NOT NULL DEFAULT GETDATE(),
                    CompletedDate datetime,
                    ErrorMessage nvarchar(max)
                )";

                using (var command = new SqlCommand(createRequestsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Bundles table
                var createBundlesTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Bundles' AND xtype='U')
                CREATE TABLE Bundles (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    BundleId nvarchar(50) NOT NULL,
                    Name nvarchar(200),
                    Description nvarchar(max),
                    ImageId nvarchar(50),
                    ComputeType nvarchar(20),
                    RootStorage nvarchar(50),
                    UserStorage nvarchar(50),
                    MonthlyPrice decimal(10,2),
                    HourlyPrice decimal(10,4),
                    Region nvarchar(20),
                    LastUpdated datetime DEFAULT GETDATE()
                )";

                using (var command = new SqlCommand(createBundlesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Directories table
                var createDirectoriesTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Directories' AND xtype='U')
                CREATE TABLE Directories (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    DirectoryId nvarchar(50) NOT NULL,
                    Name nvarchar(200),
                    DirectoryType nvarchar(50),
                    State nvarchar(20),
                    Region nvarchar(20),
                    SubnetIds nvarchar(max),
                    LastUpdated datetime DEFAULT GETDATE()
                )";

                using (var command = new SqlCommand(createDirectoriesTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // WorkSpace operations
        public void SaveWorkSpace(WorkSpace workSpace)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                INSERT INTO WorkSpaces (WorkSpaceId, DirectoryId, UserName, BundleId, State, SubnetId, IpAddress, ComputerName, Region, CreatedDate, RequestId, ErrorCode, ErrorMessage)
                VALUES (@WorkSpaceId, @DirectoryId, @UserName, @BundleId, @State, @SubnetId, @IpAddress, @ComputerName, @Region, @CreatedDate, @RequestId, @ErrorCode, @ErrorMessage)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@WorkSpaceId", workSpace.WorkSpaceId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DirectoryId", workSpace.DirectoryId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserName", workSpace.UserName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BundleId", workSpace.BundleId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@State", workSpace.State ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@SubnetId", workSpace.SubnetId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IpAddress", workSpace.IpAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ComputerName", workSpace.ComputerName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Region", workSpace.Region ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedDate", workSpace.CreatedDate);
                    command.Parameters.AddWithValue("@RequestId", workSpace.RequestId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ErrorCode", workSpace.ErrorCode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ErrorMessage", workSpace.ErrorMessage ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<WorkSpace> GetAllWorkSpaces()
        {
            var workSpaces = new List<WorkSpace>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM WorkSpaces ORDER BY CreatedDate DESC";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        workSpaces.Add(MapWorkSpaceFromReader(reader));
                    }
                }
            }
            return workSpaces;
        }

        public List<WorkSpace> GetRecentWorkSpaces(int count = 10)
        {
            var workSpaces = new List<WorkSpace>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = $"SELECT TOP {count} * FROM WorkSpaces ORDER BY CreatedDate DESC";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        workSpaces.Add(MapWorkSpaceFromReader(reader));
                    }
                }
            }
            return workSpaces;
        }

        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Get workspace counts
                var sql = @"
                SELECT 
                    COUNT(*) as Total,
                    SUM(CASE WHEN State = 'AVAILABLE' THEN 1 ELSE 0 END) as Running,
                    SUM(CASE WHEN State = 'STOPPED' THEN 1 ELSE 0 END) as Stopped,
                    SUM(CASE WHEN State IN ('PENDING', 'STARTING', 'REBOOTING') THEN 1 ELSE 0 END) as Pending,
                    SUM(CASE WHEN State IN ('ERROR', 'IMPAIRED', 'UNHEALTHY') THEN 1 ELSE 0 END) as Failed
                FROM WorkSpaces";

                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.TotalWorkSpaces = reader.GetInt32("Total");
                        stats.RunningWorkSpaces = reader.GetInt32("Running");
                        stats.StoppedWorkSpaces = reader.GetInt32("Stopped");
                        stats.PendingWorkSpaces = reader.GetInt32("Pending");
                        stats.FailedWorkSpaces = reader.GetInt32("Failed");
                    }
                }
            }

            stats.RecentWorkSpaces = GetRecentWorkSpaces(5);
            stats.RecentRequests = GetRecentRequests(5);
            return stats;
        }

        // WorkSpace Request operations
        public void SaveWorkSpaceRequest(WorkSpaceRequest request)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                INSERT INTO WorkSpaceRequests (RequestId, RequestName, RequestedCount, CreatedCount, FailedCount, Region, BundleId, DirectoryId, UsernamePrefix, Status, CreatedDate, ErrorMessage)
                VALUES (@RequestId, @RequestName, @RequestedCount, @CreatedCount, @FailedCount, @Region, @BundleId, @DirectoryId, @UsernamePrefix, @Status, @CreatedDate, @ErrorMessage)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RequestId", request.RequestId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RequestName", request.RequestName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RequestedCount", request.RequestedCount);
                    command.Parameters.AddWithValue("@CreatedCount", request.CreatedCount);
                    command.Parameters.AddWithValue("@FailedCount", request.FailedCount);
                    command.Parameters.AddWithValue("@Region", request.Region ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BundleId", request.BundleId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DirectoryId", request.DirectoryId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UsernamePrefix", request.UsernamePrefix ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", request.Status ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);
                    command.Parameters.AddWithValue("@ErrorMessage", request.ErrorMessage ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<WorkSpaceRequest> GetRecentRequests(int count = 10)
        {
            var requests = new List<WorkSpaceRequest>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = $"SELECT TOP {count} * FROM WorkSpaceRequests ORDER BY CreatedDate DESC";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        requests.Add(MapRequestFromReader(reader));
                    }
                }
            }
            return requests;
        }

        // Bundle operations
        public void SaveBundle(Bundle bundle)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Check if bundle already exists
                var checkSql = "SELECT COUNT(*) FROM Bundles WHERE BundleId = @BundleId AND Region = @Region";
                using (var checkCommand = new SqlCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@BundleId", bundle.BundleId);
                    checkCommand.Parameters.AddWithValue("@Region", bundle.Region);
                    var exists = (int)checkCommand.ExecuteScalar() > 0;

                    if (exists)
                    {
                        // Update existing
                        var updateSql = @"
                        UPDATE Bundles SET 
                            Name = @Name, Description = @Description, ImageId = @ImageId, ComputeType = @ComputeType,
                            RootStorage = @RootStorage, UserStorage = @UserStorage, MonthlyPrice = @MonthlyPrice,
                            HourlyPrice = @HourlyPrice, LastUpdated = GETDATE()
                        WHERE BundleId = @BundleId AND Region = @Region";
                        
                        using (var updateCommand = new SqlCommand(updateSql, connection))
                        {
                            AddBundleParameters(updateCommand, bundle);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Insert new
                        var insertSql = @"
                        INSERT INTO Bundles (BundleId, Name, Description, ImageId, ComputeType, RootStorage, UserStorage, MonthlyPrice, HourlyPrice, Region)
                        VALUES (@BundleId, @Name, @Description, @ImageId, @ComputeType, @RootStorage, @UserStorage, @MonthlyPrice, @HourlyPrice, @Region)";
                        
                        using (var insertCommand = new SqlCommand(insertSql, connection))
                        {
                            AddBundleParameters(insertCommand, bundle);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public List<Bundle> GetBundlesByRegion(string region)
        {
            var bundles = new List<Bundle>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM Bundles WHERE Region = @Region ORDER BY Name";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Region", region);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bundles.Add(MapBundleFromReader(reader));
                        }
                    }
                }
            }
            return bundles;
        }

        public List<Directory> GetDirectoriesByRegion(string region)
        {
            var directories = new List<Directory>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM Directories WHERE Region = @Region ORDER BY Name";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Region", region);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            directories.Add(MapDirectoryFromReader(reader));
                        }
                    }
                }
            }
            return directories;
        }

        // Helper methods
        private WorkSpace MapWorkSpaceFromReader(SqlDataReader reader)
        {
            return new WorkSpace
            {
                Id = reader.GetInt32("Id"),
                WorkSpaceId = reader.IsDBNull("WorkSpaceId") ? null : reader.GetString("WorkSpaceId"),
                DirectoryId = reader.IsDBNull("DirectoryId") ? null : reader.GetString("DirectoryId"),
                UserName = reader.IsDBNull("UserName") ? null : reader.GetString("UserName"),
                BundleId = reader.IsDBNull("BundleId") ? null : reader.GetString("BundleId"),
                State = reader.IsDBNull("State") ? null : reader.GetString("State"),
                SubnetId = reader.IsDBNull("SubnetId") ? null : reader.GetString("SubnetId"),
                IpAddress = reader.IsDBNull("IpAddress") ? null : reader.GetString("IpAddress"),
                ComputerName = reader.IsDBNull("ComputerName") ? null : reader.GetString("ComputerName"),
                Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastUpdated = reader.IsDBNull("LastUpdated") ? null : reader.GetDateTime("LastUpdated"),
                RequestId = reader.IsDBNull("RequestId") ? null : reader.GetString("RequestId"),
                ErrorCode = reader.IsDBNull("ErrorCode") ? null : reader.GetString("ErrorCode"),
                ErrorMessage = reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage")
            };
        }

        private WorkSpaceRequest MapRequestFromReader(SqlDataReader reader)
        {
            return new WorkSpaceRequest
            {
                Id = reader.GetInt32("Id"),
                RequestId = reader.IsDBNull("RequestId") ? null : reader.GetString("RequestId"),
                RequestName = reader.IsDBNull("RequestName") ? null : reader.GetString("RequestName"),
                RequestedCount = reader.GetInt32("RequestedCount"),
                CreatedCount = reader.GetInt32("CreatedCount"),
                FailedCount = reader.GetInt32("FailedCount"),
                Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                BundleId = reader.IsDBNull("BundleId") ? null : reader.GetString("BundleId"),
                DirectoryId = reader.IsDBNull("DirectoryId") ? null : reader.GetString("DirectoryId"),
                UsernamePrefix = reader.IsDBNull("UsernamePrefix") ? null : reader.GetString("UsernamePrefix"),
                Status = reader.IsDBNull("Status") ? null : reader.GetString("Status"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CompletedDate = reader.IsDBNull("CompletedDate") ? null : reader.GetDateTime("CompletedDate"),
                ErrorMessage = reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage")
            };
        }

        private Bundle MapBundleFromReader(SqlDataReader reader)
        {
            return new Bundle
            {
                Id = reader.GetInt32("Id"),
                BundleId = reader.GetString("BundleId"),
                Name = reader.IsDBNull("Name") ? null : reader.GetString("Name"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                ImageId = reader.IsDBNull("ImageId") ? null : reader.GetString("ImageId"),
                ComputeType = reader.IsDBNull("ComputeType") ? null : reader.GetString("ComputeType"),
                RootStorage = reader.IsDBNull("RootStorage") ? null : reader.GetString("RootStorage"),
                UserStorage = reader.IsDBNull("UserStorage") ? null : reader.GetString("UserStorage"),
                MonthlyPrice = reader.IsDBNull("MonthlyPrice") ? null : reader.GetDecimal("MonthlyPrice"),
                HourlyPrice = reader.IsDBNull("HourlyPrice") ? null : reader.GetDecimal("HourlyPrice"),
                Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                LastUpdated = reader.GetDateTime("LastUpdated")
            };
        }

        private Directory MapDirectoryFromReader(SqlDataReader reader)
        {
            var directory = new Directory
            {
                Id = reader.GetInt32("Id"),
                DirectoryId = reader.GetString("DirectoryId"),
                Name = reader.IsDBNull("Name") ? null : reader.GetString("Name"),
                DirectoryType = reader.IsDBNull("DirectoryType") ? null : reader.GetString("DirectoryType"),
                State = reader.IsDBNull("State") ? null : reader.GetString("State"),
                Region = reader.IsDBNull("Region") ? null : reader.GetString("Region"),
                LastUpdated = reader.GetDateTime("LastUpdated")
            };

            // Parse SubnetIds from comma-separated string
            var subnetIdsStr = reader.IsDBNull("SubnetIds") ? null : reader.GetString("SubnetIds");
            if (!string.IsNullOrEmpty(subnetIdsStr))
            {
                directory.SubnetIds = new List<string>(subnetIdsStr.Split(','));
            }

            return directory;
        }

        private void AddBundleParameters(SqlCommand command, Bundle bundle)
        {
            command.Parameters.AddWithValue("@BundleId", bundle.BundleId);
            command.Parameters.AddWithValue("@Name", bundle.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", bundle.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ImageId", bundle.ImageId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ComputeType", bundle.ComputeType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RootStorage", bundle.RootStorage ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UserStorage", bundle.UserStorage ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MonthlyPrice", bundle.MonthlyPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HourlyPrice", bundle.HourlyPrice ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Region", bundle.Region ?? (object)DBNull.Value);
        }
    }
}