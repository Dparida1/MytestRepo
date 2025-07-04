using System;
using System.Linq;
using System.Web.UI;

namespace WorkSpaceManager
{
    public partial class _Default : Page
    {
        private DatabaseHelper database;
        private AWSWorkSpaceService awsService;

        protected void Page_Load(object sender, EventArgs e)
        {
            database = new DatabaseHelper();
            awsService = new AWSWorkSpaceService();

            if (!IsPostBack)
            {
                // Initialize database on first load
                database.InitializeDatabase();
                LoadDashboardData();
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                var stats = database.GetDashboardStats();

                // Update stats labels
                TotalWorkSpacesLabel.Text = stats.TotalWorkSpaces.ToString();
                RunningWorkSpacesLabel.Text = stats.RunningWorkSpaces.ToString();
                StoppedWorkSpacesLabel.Text = stats.StoppedWorkSpaces.ToString();
                PendingWorkSpacesLabel.Text = stats.PendingWorkSpaces.ToString();

                // Bind recent workspaces
                if (stats.RecentWorkSpaces.Any())
                {
                    RecentWorkSpacesRepeater.DataSource = stats.RecentWorkSpaces;
                    RecentWorkSpacesRepeater.DataBind();
                    NoWorkSpacesPanel.Visible = false;
                }
                else
                {
                    NoWorkSpacesPanel.Visible = true;
                }

                // Bind recent requests
                if (stats.RecentRequests.Any())
                {
                    RecentRequestsRepeater.DataSource = stats.RecentRequests;
                    RecentRequestsRepeater.DataBind();
                    NoRequestsPanel.Visible = false;
                }
                else
                {
                    NoRequestsPanel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading dashboard data: {ex.Message}");
            }
        }

        protected void SyncDataButton_Click(object sender, EventArgs e)
        {
            SyncFromAWS();
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadDashboardData();
            ShowInfoMessage("Dashboard data refreshed.");
        }

        protected void ModalSyncButton_Click(object sender, EventArgs e)
        {
            var selectedRegion = SyncRegionDropDown.SelectedValue;
            SyncFromAWS(selectedRegion);
        }

        private void SyncFromAWS(string region = null)
        {
            try
            {
                var successCount = 0;
                var errorMessages = new System.Collections.Generic.List<string>();

                // Sync bundles
                var bundleResult = awsService.SyncBundlesFromAWS(region);
                if (bundleResult.Success)
                {
                    successCount++;
                }
                else
                {
                    errorMessages.Add($"Bundles: {bundleResult.Message}");
                }

                // Sync directories
                var directoryResult = awsService.SyncDirectoriesFromAWS(region);
                if (directoryResult.Success)
                {
                    successCount++;
                }
                else
                {
                    errorMessages.Add($"Directories: {directoryResult.Message}");
                }

                // Sync workspaces
                var workspaceResult = awsService.SyncWorkSpacesFromAWS(region);
                if (workspaceResult.Success)
                {
                    successCount++;
                }
                else
                {
                    errorMessages.Add($"WorkSpaces: {workspaceResult.Message}");
                }

                // Show results
                if (successCount > 0)
                {
                    ShowSuccessMessage($"Sync completed successfully for {successCount} data types from region {region ?? "default"}.");
                    LoadDashboardData(); // Refresh the dashboard
                }

                if (errorMessages.Any())
                {
                    ShowWarningMessage($"Some data could not be synced: {string.Join(", ", errorMessages)}");
                }

                if (successCount == 0)
                {
                    ShowErrorMessage("Sync failed. Please check your AWS credentials and permissions.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error during sync: {ex.Message}");
            }
        }

        // Helper methods for data binding
        protected string GetStateBootstrapClass(string state)
        {
            return WorkSpaceStates.GetBootstrapClass(state);
        }

        protected string GetRequestStatusBootstrapClass(string status)
        {
            return status?.ToUpper() switch
            {
                "COMPLETED" => "success",
                "FAILED" => "danger",
                "PENDING" => "warning",
                "IN_PROGRESS" => "info",
                _ => "secondary"
            };
        }

        private void ShowSuccessMessage(string message)
        {
            var master = Master as SiteMaster;
            master?.ShowSuccessMessage(message);
        }

        private void ShowErrorMessage(string message)
        {
            var master = Master as SiteMaster;
            master?.ShowErrorMessage(message);
        }

        private void ShowWarningMessage(string message)
        {
            var master = Master as SiteMaster;
            master?.ShowWarningMessage(message);
        }

        private void ShowInfoMessage(string message)
        {
            var master = Master as SiteMaster;
            master?.ShowInfoMessage(message);
        }
    }
}