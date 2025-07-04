using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WorkSpaceManager
{
    public partial class ViewWorkSpaces : Page
    {
        private DatabaseHelper database;
        private AWSWorkSpaceService awsService;

        protected void Page_Load(object sender, EventArgs e)
        {
            database = new DatabaseHelper();
            awsService = new AWSWorkSpaceService();

            if (!IsPostBack)
            {
                LoadWorkSpaces();
            }

            // Handle bulk actions from JavaScript
            HandleBulkActions();
        }

        private void LoadWorkSpaces()
        {
            try
            {
                var workSpaces = database.GetAllWorkSpaces();

                // Apply filters
                workSpaces = ApplyFilters(workSpaces);

                WorkSpacesGridView.DataSource = workSpaces;
                WorkSpacesGridView.DataBind();

                WorkSpaceCountLabel.Text = workSpaces.Count.ToString();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading WorkSpaces: {ex.Message}");
            }
        }

        private List<WorkSpace> ApplyFilters(List<WorkSpace> workSpaces)
        {
            // Filter by state
            if (!string.IsNullOrEmpty(StateFilterDropDown.SelectedValue))
            {
                workSpaces = workSpaces.Where(w => w.State == StateFilterDropDown.SelectedValue).ToList();
            }

            // Filter by region
            if (!string.IsNullOrEmpty(RegionFilterDropDown.SelectedValue))
            {
                workSpaces = workSpaces.Where(w => w.Region == RegionFilterDropDown.SelectedValue).ToList();
            }

            // Search filter
            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                var searchTerm = SearchTextBox.Text.Trim().ToLower();
                workSpaces = workSpaces.Where(w => 
                    (!string.IsNullOrEmpty(w.WorkSpaceId) && w.WorkSpaceId.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(w.UserName) && w.UserName.ToLower().Contains(searchTerm))
                ).ToList();
            }

            return workSpaces;
        }

        private void HandleBulkActions()
        {
            var bulkAction = Request.Form["bulkAction"];
            var selectedWorkSpaces = Request.Form.GetValues("selectedWorkSpaces");

            if (!string.IsNullOrEmpty(bulkAction) && selectedWorkSpaces != null && selectedWorkSpaces.Length > 0)
            {
                var workSpaceIds = selectedWorkSpaces.ToList();
                OperationResult result = null;

                switch (bulkAction.ToLower())
                {
                    case "start":
                        result = awsService.StartWorkSpaces(workSpaceIds);
                        break;
                    case "stop":
                        result = awsService.StopWorkSpaces(workSpaceIds);
                        break;
                    case "reboot":
                        result = awsService.RebootWorkSpaces(workSpaceIds);
                        break;
                    default:
                        ShowErrorMessage("Invalid bulk action specified.");
                        return;
                }

                if (result != null)
                {
                    if (result.Success)
                    {
                        ShowSuccessMessage(result.Message);
                    }
                    else
                    {
                        ShowErrorMessage($"Bulk {bulkAction} failed: {result.Message}");
                    }
                }

                // Refresh the page to show updated data
                Response.Redirect(Request.Url.ToString(), false);
            }
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadWorkSpaces();
            ShowInfoMessage("WorkSpaces list refreshed.");
        }

        protected void StateFilterDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadWorkSpaces();
        }

        protected void RegionFilterDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadWorkSpaces();
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            LoadWorkSpaces();
        }

        protected void ClearFiltersButton_Click(object sender, EventArgs e)
        {
            StateFilterDropDown.SelectedIndex = 0;
            RegionFilterDropDown.SelectedIndex = 0;
            SearchTextBox.Text = "";
            LoadWorkSpaces();
            ShowInfoMessage("Filters cleared.");
        }

        protected void WorkSpacesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var workSpaceId = e.CommandArgument.ToString();

            try
            {
                OperationResult result = null;

                switch (e.CommandName)
                {
                    case "StartWorkSpace":
                        result = awsService.StartWorkSpaces(new List<string> { workSpaceId });
                        break;
                    case "StopWorkSpace":
                        result = awsService.StopWorkSpaces(new List<string> { workSpaceId });
                        break;
                    case "RebootWorkSpace":
                        result = awsService.RebootWorkSpaces(new List<string> { workSpaceId });
                        break;
                    default:
                        ShowErrorMessage("Invalid operation specified.");
                        return;
                }

                if (result != null)
                {
                    if (result.Success)
                    {
                        ShowSuccessMessage(result.Message);
                    }
                    else
                    {
                        ShowErrorMessage($"Operation failed: {result.Message}");
                    }
                }

                // Refresh the grid
                LoadWorkSpaces();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error performing operation: {ex.Message}");
            }
        }

        protected void WorkSpacesGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var workSpace = e.Row.DataItem as WorkSpace;
                if (workSpace != null)
                {
                    // Find action buttons
                    var startButton = e.Row.FindControl("StartButton") as Button;
                    var stopButton = e.Row.FindControl("StopButton") as Button;
                    var rebootButton = e.Row.FindControl("RebootButton") as Button;

                    // Enable/disable buttons based on WorkSpace state
                    if (startButton != null && stopButton != null && rebootButton != null)
                    {
                        switch (workSpace.State?.ToUpper())
                        {
                            case "AVAILABLE":
                                startButton.Enabled = false;
                                stopButton.Enabled = true;
                                rebootButton.Enabled = true;
                                break;
                            case "STOPPED":
                                startButton.Enabled = true;
                                stopButton.Enabled = false;
                                rebootButton.Enabled = false;
                                break;
                            case "PENDING":
                            case "STARTING":
                            case "STOPPING":
                            case "REBOOTING":
                                startButton.Enabled = false;
                                stopButton.Enabled = false;
                                rebootButton.Enabled = false;
                                break;
                            default:
                                startButton.Enabled = false;
                                stopButton.Enabled = false;
                                rebootButton.Enabled = false;
                                break;
                        }
                    }
                }
            }
        }

        // Helper method for data binding
        protected string GetStateBootstrapClass(string state)
        {
            return WorkSpaceStates.GetBootstrapClass(state);
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