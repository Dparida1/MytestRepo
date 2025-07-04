using System;
using System.Configuration;
using System.Web.UI;

namespace WorkSpaceManager
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set current region from configuration
                var currentRegion = ConfigurationManager.AppSettings["AWS_Region"] ?? "us-east-1";
                CurrentRegionLabel.Text = currentRegion;
            }
        }

        public void ShowAlert(string message, string alertType = "info")
        {
            var alertClass = $"alert-{alertType}";
            AlertPanel.CssClass = $"alert {alertClass} alert-dismissible fade show";
            AlertMessage.Text = message;
            AlertPanel.Visible = true;
        }

        public void ShowSuccessMessage(string message)
        {
            ShowAlert(message, "success");
        }

        public void ShowErrorMessage(string message)
        {
            ShowAlert(message, "danger");
        }

        public void ShowWarningMessage(string message)
        {
            ShowAlert(message, "warning");
        }

        public void ShowInfoMessage(string message)
        {
            ShowAlert(message, "info");
        }
    }
}