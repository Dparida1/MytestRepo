using System;
using System.Linq;
using System.Web.UI;

namespace WorkSpaceManager
{
    public partial class CreateWorkSpaces : Page
    {
        private DatabaseHelper database;
        private AWSWorkSpaceService awsService;

        protected void Page_Load(object sender, EventArgs e)
        {
            database = new DatabaseHelper();
            awsService = new AWSWorkSpaceService();

            if (!IsPostBack)
            {
                LoadBundlesAndDirectories();
            }
        }

        private void LoadBundlesAndDirectories()
        {
            try
            {
                var selectedRegion = RegionDropDown.SelectedValue;
                
                // Load bundles
                var bundles = database.GetBundlesByRegion(selectedRegion);
                BundleDropDown.Items.Clear();
                BundleDropDown.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Bundle --", ""));
                
                foreach (var bundle in bundles)
                {
                    var text = $"{bundle.Name} ({bundle.ComputeType})";
                    if (bundle.MonthlyPrice.HasValue)
                    {
                        text += $" - ${bundle.MonthlyPrice:F2}/month";
                    }
                    BundleDropDown.Items.Add(new System.Web.UI.WebControls.ListItem(text, bundle.BundleId));
                }

                // Load directories
                var directories = database.GetDirectoriesByRegion(selectedRegion);
                DirectoryDropDown.Items.Clear();
                DirectoryDropDown.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Directory --", ""));
                
                foreach (var directory in directories)
                {
                    var text = $"{directory.Name} ({directory.DirectoryType})";
                    DirectoryDropDown.Items.Add(new System.Web.UI.WebControls.ListItem(text, directory.DirectoryId));
                }

                // Show message if no data found
                if (!bundles.Any() || !directories.Any())
                {
                    ShowWarningMessage($"No bundles or directories found for region {selectedRegion}. Please sync data from AWS first.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading bundles and directories: {ex.Message}");
            }
        }

        protected void RegionDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadBundlesAndDirectories();
        }

        protected void RunningModeDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            // This will trigger client-side JavaScript to show/hide timeout field
            // The actual logic is handled in JavaScript in the ASPX file
        }

        protected void SyncBundlesButton_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedRegion = RegionDropDown.SelectedValue;
                var successCount = 0;
                var errorMessages = new System.Collections.Generic.List<string>();

                // Sync bundles
                var bundleResult = awsService.SyncBundlesFromAWS(selectedRegion);
                if (bundleResult.Success)
                {
                    successCount++;
                }
                else
                {
                    errorMessages.Add($"Bundles: {bundleResult.Message}");
                }

                // Sync directories
                var directoryResult = awsService.SyncDirectoriesFromAWS(selectedRegion);
                if (directoryResult.Success)
                {
                    successCount++;
                }
                else
                {
                    errorMessages.Add($"Directories: {directoryResult.Message}");
                }

                // Show results
                if (successCount > 0)
                {
                    ShowSuccessMessage($"Successfully synced data for region {selectedRegion}.");
                    LoadBundlesAndDirectories(); // Refresh the dropdowns
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

        protected void CreateWorkSpacesButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                // Validate selections
                if (string.IsNullOrEmpty(BundleDropDown.SelectedValue))
                {
                    ShowErrorMessage("Please select a bundle.");
                    return;
                }

                if (string.IsNullOrEmpty(DirectoryDropDown.SelectedValue))
                {
                    ShowErrorMessage("Please select a directory.");
                    return;
                }

                // Create view model from form data
                var model = new CreateWorkSpaceViewModel
                {
                    RequestName = RequestNameTextBox.Text.Trim(),
                    WorkSpaceCount = int.Parse(WorkSpaceCountTextBox.Text),
                    Region = RegionDropDown.SelectedValue,
                    BundleId = BundleDropDown.SelectedValue,
                    DirectoryId = DirectoryDropDown.SelectedValue,
                    UsernamePrefix = UsernamePrefixTextBox.Text.Trim(),
                    VolumeEncryptionEnabled = VolumeEncryptionCheckBox.Checked,
                    UserVolumeEncryptionEnabled = UserVolumeEncryptionCheckBox.Checked,
                    RunningMode = RunningModeDropDown.SelectedValue,
                    RunningModeAutoStopTimeoutInMinutes = RunningModeDropDown.SelectedValue == "AUTO_STOP" 
                        ? int.Parse(AutoStopTimeoutTextBox.Text) 
                        : 60
                };

                // Validate workspace count
                if (model.WorkSpaceCount < 1 || model.WorkSpaceCount > 100)
                {
                    ShowErrorMessage("Number of WorkSpaces must be between 1 and 100.");
                    return;
                }

                // Call AWS service to create workspaces
                var result = awsService.CreateWorkSpaces(model);

                if (result.Success)
                {
                    // Show success message
                    ShowSuccessMessage(result.Message);
                    
                    // Show progress panel
                    ProgressPanel.Visible = true;
                    
                    // Clear form for next use
                    ClearForm();
                    
                    // Optionally redirect to view page after a delay
                    // Response.Redirect("~/ViewWorkSpaces.aspx", false);
                }
                else
                {
                    ShowErrorMessage($"Failed to create WorkSpaces: {result.Message}");
                    if (!string.IsNullOrEmpty(result.ErrorDetails))
                    {
                        ShowErrorMessage($"Details: {result.ErrorDetails}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error creating WorkSpaces: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            RequestNameTextBox.Text = "";
            WorkSpaceCountTextBox.Text = "1";
            UsernamePrefixTextBox.Text = "user";
            VolumeEncryptionCheckBox.Checked = true;
            UserVolumeEncryptionCheckBox.Checked = true;
            RunningModeDropDown.SelectedValue = "AUTO_STOP";
            AutoStopTimeoutTextBox.Text = "60";
            
            // Reset dropdowns to default selections
            if (BundleDropDown.Items.Count > 0)
            {
                BundleDropDown.SelectedIndex = 0;
            }
            if (DirectoryDropDown.Items.Count > 0)
            {
                DirectoryDropDown.SelectedIndex = 0;
            }
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