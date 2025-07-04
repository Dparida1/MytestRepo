<%@ Page Title="Create WorkSpaces" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreateWorkSpaces.aspx.cs" Inherits="WorkSpaceManager.CreateWorkSpaces" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h2">
            <i class="fas fa-plus text-primary"></i>
            Create WorkSpaces
        </h1>
        
        <asp:HyperLink ID="BackToDashboardLink" runat="server" 
            NavigateUrl="~/Default.aspx" 
            CssClass="btn btn-outline-secondary">
            <i class="fas fa-arrow-left"></i> Back to Dashboard
        </asp:HyperLink>
    </div>

    <div class="row">
        <div class="col-lg-8">
            <div class="card border-0 shadow-sm">
                <div class="card-header bg-light">
                    <h5 class="card-title mb-0">
                        <i class="fas fa-cog text-primary"></i>
                        WorkSpace Configuration
                    </h5>
                </div>
                <div class="card-body">
                    <!-- Basic Information -->
                    <div class="mb-4">
                        <h6 class="text-muted mb-3">Basic Information</h6>
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label for="RequestNameTextBox" class="form-label">Request Name</label>
                                <asp:TextBox ID="RequestNameTextBox" runat="server" 
                                    CssClass="form-control" 
                                    placeholder="e.g., Development Team WorkSpaces"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequestNameRequired" runat="server" 
                                    ControlToValidate="RequestNameTextBox" 
                                    ErrorMessage="Request name is required" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RequiredFieldValidator>
                            </div>
                            <div class="col-md-6">
                                <label for="WorkSpaceCountTextBox" class="form-label">Number of WorkSpaces</label>
                                <asp:TextBox ID="WorkSpaceCountTextBox" runat="server" 
                                    CssClass="form-control" 
                                    TextMode="Number" 
                                    Text="1" 
                                    min="1" max="100"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="WorkSpaceCountRequired" runat="server" 
                                    ControlToValidate="WorkSpaceCountTextBox" 
                                    ErrorMessage="Number of WorkSpaces is required" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RequiredFieldValidator>
                                <asp:RangeValidator ID="WorkSpaceCountRange" runat="server" 
                                    ControlToValidate="WorkSpaceCountTextBox" 
                                    Type="Integer" 
                                    MinimumValue="1" 
                                    MaximumValue="100" 
                                    ErrorMessage="Must be between 1 and 100" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RangeValidator>
                            </div>
                        </div>
                    </div>

                    <!-- AWS Configuration -->
                    <div class="mb-4">
                        <h6 class="text-muted mb-3">AWS Configuration</h6>
                        <div class="row g-3">
                            <div class="col-md-4">
                                <label for="RegionDropDown" class="form-label">AWS Region</label>
                                <asp:DropDownList ID="RegionDropDown" runat="server" 
                                    CssClass="form-select" 
                                    AutoPostBack="true" 
                                    OnSelectedIndexChanged="RegionDropDown_SelectedIndexChanged">
                                    <asp:ListItem Value="us-east-1" Text="US East (N. Virginia)" Selected="True"></asp:ListItem>
                                    <asp:ListItem Value="us-west-2" Text="US West (Oregon)"></asp:ListItem>
                                    <asp:ListItem Value="eu-west-1" Text="Europe (Ireland)"></asp:ListItem>
                                    <asp:ListItem Value="ap-southeast-1" Text="Asia Pacific (Singapore)"></asp:ListItem>
                                    <asp:ListItem Value="ap-southeast-2" Text="Asia Pacific (Sydney)"></asp:ListItem>
                                    <asp:ListItem Value="ap-northeast-1" Text="Asia Pacific (Tokyo)"></asp:ListItem>
                                    <asp:ListItem Value="ca-central-1" Text="Canada (Central)"></asp:ListItem>
                                </asp:DropDownList>
                                <small class="form-text text-muted">Select your AWS region</small>
                            </div>
                            <div class="col-md-4">
                                <label for="BundleDropDown" class="form-label">Bundle</label>
                                <asp:DropDownList ID="BundleDropDown" runat="server" 
                                    CssClass="form-select" 
                                    DataTextField="Name" 
                                    DataValueField="BundleId">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="BundleRequired" runat="server" 
                                    ControlToValidate="BundleDropDown" 
                                    InitialValue="" 
                                    ErrorMessage="Please select a bundle" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RequiredFieldValidator>
                                <small class="form-text text-muted">WorkSpace configuration template</small>
                            </div>
                            <div class="col-md-4">
                                <label for="DirectoryDropDown" class="form-label">Directory</label>
                                <asp:DropDownList ID="DirectoryDropDown" runat="server" 
                                    CssClass="form-select" 
                                    DataTextField="Name" 
                                    DataValueField="DirectoryId">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="DirectoryRequired" runat="server" 
                                    ControlToValidate="DirectoryDropDown" 
                                    InitialValue="" 
                                    ErrorMessage="Please select a directory" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RequiredFieldValidator>
                                <small class="form-text text-muted">Active Directory for user authentication</small>
                            </div>
                        </div>
                        
                        <!-- Sync Button -->
                        <div class="row mt-3">
                            <div class="col-12">
                                <asp:Button ID="SyncBundlesButton" runat="server" 
                                    CssClass="btn btn-outline-info btn-sm" 
                                    Text="Sync Bundles & Directories" 
                                    OnClick="SyncBundlesButton_Click" 
                                    CausesValidation="false" />
                                <small class="form-text text-muted ms-2">
                                    Refresh available bundles and directories from AWS
                                </small>
                            </div>
                        </div>
                    </div>

                    <!-- User Configuration -->
                    <div class="mb-4">
                        <h6 class="text-muted mb-3">User Configuration</h6>
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label for="UsernamePrefixTextBox" class="form-label">Username Prefix</label>
                                <asp:TextBox ID="UsernamePrefixTextBox" runat="server" 
                                    CssClass="form-control" 
                                    Text="user" 
                                    placeholder="e.g., user, dev, test"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="UsernamePrefixRequired" runat="server" 
                                    ControlToValidate="UsernamePrefixTextBox" 
                                    ErrorMessage="Username prefix is required" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RequiredFieldValidator>
                                <small class="form-text text-muted">Users will be named: prefix001, prefix002, etc.</small>
                            </div>
                            <div class="col-md-6">
                                <label class="form-label">Username Preview</label>
                                <div class="form-control bg-light" style="min-height: 38px;">
                                    <asp:Label ID="UsernamePreviewLabel" runat="server" 
                                        Text="user001, user002, user003..." 
                                        CssClass="text-muted"></asp:Label>
                                </div>
                                <small class="form-text text-muted">Preview of generated usernames</small>
                            </div>
                        </div>
                    </div>

                    <!-- Security Settings -->
                    <div class="mb-4">
                        <h6 class="text-muted mb-3">Security Settings</h6>
                        <div class="row g-3">
                            <div class="col-md-6">
                                <div class="form-check">
                                    <asp:CheckBox ID="VolumeEncryptionCheckBox" runat="server" 
                                        CssClass="form-check-input" 
                                        Checked="true" />
                                    <label class="form-check-label" for="VolumeEncryptionCheckBox">
                                        Enable Root Volume Encryption
                                    </label>
                                </div>
                                <small class="form-text text-muted">Encrypt the root volume of WorkSpaces</small>
                            </div>
                            <div class="col-md-6">
                                <div class="form-check">
                                    <asp:CheckBox ID="UserVolumeEncryptionCheckBox" runat="server" 
                                        CssClass="form-check-input" 
                                        Checked="true" />
                                    <label class="form-check-label" for="UserVolumeEncryptionCheckBox">
                                        Enable User Volume Encryption
                                    </label>
                                </div>
                                <small class="form-text text-muted">Encrypt the user data volume</small>
                            </div>
                        </div>
                    </div>

                    <!-- WorkSpace Properties -->
                    <div class="mb-4">
                        <h6 class="text-muted mb-3">WorkSpace Properties</h6>
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label for="RunningModeDropDown" class="form-label">Running Mode</label>
                                <asp:DropDownList ID="RunningModeDropDown" runat="server" 
                                    CssClass="form-select" 
                                    AutoPostBack="true" 
                                    OnSelectedIndexChanged="RunningModeDropDown_SelectedIndexChanged">
                                    <asp:ListItem Value="AUTO_STOP" Text="Auto Stop" Selected="True"></asp:ListItem>
                                    <asp:ListItem Value="ALWAYS_ON" Text="Always On"></asp:ListItem>
                                </asp:DropDownList>
                                <small class="form-text text-muted">How the WorkSpace should run</small>
                            </div>
                            <div class="col-md-6">
                                <label for="AutoStopTimeoutTextBox" class="form-label">Auto Stop Timeout (minutes)</label>
                                <asp:TextBox ID="AutoStopTimeoutTextBox" runat="server" 
                                    CssClass="form-control" 
                                    TextMode="Number" 
                                    Text="60" 
                                    min="60" max="36000"></asp:TextBox>
                                <asp:RangeValidator ID="AutoStopTimeoutRange" runat="server" 
                                    ControlToValidate="AutoStopTimeoutTextBox" 
                                    Type="Integer" 
                                    MinimumValue="60" 
                                    MaximumValue="36000" 
                                    ErrorMessage="Must be between 60 and 36000 minutes" 
                                    CssClass="text-danger small" 
                                    Display="Dynamic"></asp:RangeValidator>
                                <small class="form-text text-muted">Idle time before auto-stopping (Auto Stop mode only)</small>
                            </div>
                        </div>
                    </div>

                    <!-- Action Buttons -->
                    <div class="d-flex justify-content-between">
                        <asp:HyperLink ID="CancelLink" runat="server" 
                            NavigateUrl="~/Default.aspx" 
                            CssClass="btn btn-outline-secondary">
                            <i class="fas fa-times"></i> Cancel
                        </asp:HyperLink>
                        
                        <asp:Button ID="CreateWorkSpacesButton" runat="server" 
                            CssClass="btn btn-primary btn-lg" 
                            Text="Create WorkSpaces" 
                            OnClick="CreateWorkSpacesButton_Click" 
                            OnClientClick="return confirm('Are you sure you want to create these WorkSpaces? This action cannot be undone.');" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Side Panel with Info -->
        <div class="col-lg-4">
            <div class="card border-0 shadow-sm">
                <div class="card-header bg-light">
                    <h5 class="card-title mb-0">
                        <i class="fas fa-info-circle text-info"></i>
                        Information
                    </h5>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <h6>What happens next?</h6>
                        <ol class="small">
                            <li>WorkSpaces will be created in AWS</li>
                            <li>Users will be automatically generated</li>
                            <li>WorkSpaces will appear in "Pending" state</li>
                            <li>Once ready, users can connect using WorkSpaces client</li>
                        </ol>
                    </div>

                    <div class="mb-3">
                        <h6>Cost Estimation</h6>
                        <div class="alert alert-info small">
                            <i class="fas fa-calculator"></i>
                            Actual costs depend on the selected bundle and usage. 
                            Check AWS pricing for WorkSpaces for detailed information.
                        </div>
                    </div>

                    <div class="mb-3">
                        <h6>Prerequisites</h6>
                        <ul class="small">
                            <li>Valid AWS WorkSpaces bundle</li>
                            <li>Active Directory Service</li>
                            <li>Sufficient WorkSpaces quota</li>
                            <li>Proper IAM permissions</li>
                        </ul>
                    </div>
                </div>
            </div>

            <!-- Progress Panel (hidden by default) -->
            <asp:Panel ID="ProgressPanel" runat="server" Visible="false" CssClass="card border-0 shadow-sm mt-3">
                <div class="card-header bg-success text-white">
                    <h5 class="card-title mb-0">
                        <i class="fas fa-check-circle"></i>
                        Creation in Progress
                    </h5>
                </div>
                <div class="card-body">
                    <p>Your WorkSpaces are being created. This may take several minutes.</p>
                    <div class="progress mb-3">
                        <div class="progress-bar progress-bar-striped progress-bar-animated" 
                             role="progressbar" style="width: 100%"></div>
                    </div>
                    <asp:HyperLink ID="ViewProgressLink" runat="server" 
                        NavigateUrl="~/ViewWorkSpaces.aspx" 
                        CssClass="btn btn-outline-primary btn-sm">
                        <i class="fas fa-eye"></i> View Progress
                    </asp:HyperLink>
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- JavaScript for form interactions -->
    <script>
        // Update username preview when prefix or count changes
        function updateUsernamePreview() {
            var prefix = document.getElementById('<%= UsernamePrefixTextBox.ClientID %>').value || 'user';
            var count = parseInt(document.getElementById('<%= WorkSpaceCountTextBox.ClientID %>').value) || 1;
            
            var preview = '';
            var maxPreview = Math.min(count, 3);
            
            for (var i = 1; i <= maxPreview; i++) {
                if (i > 1) preview += ', ';
                preview += prefix + String(i).padStart(3, '0');
            }
            
            if (count > 3) {
                preview += '...';
            }
            
            document.getElementById('<%= UsernamePreviewLabel.ClientID %>').textContent = preview;
        }

        // Update timeout field visibility based on running mode
        function updateTimeoutVisibility() {
            var runningMode = document.getElementById('<%= RunningModeDropDown.ClientID %>').value;
            var timeoutField = document.getElementById('<%= AutoStopTimeoutTextBox.ClientID %>').closest('.col-md-6');
            
            if (runningMode === 'AUTO_STOP') {
                timeoutField.style.display = 'block';
            } else {
                timeoutField.style.display = 'none';
            }
        }

        // Initialize on page load
        document.addEventListener('DOMContentLoaded', function() {
            updateUsernamePreview();
            updateTimeoutVisibility();
            
            // Add event listeners
            document.getElementById('<%= UsernamePrefixTextBox.ClientID %>').addEventListener('input', updateUsernamePreview);
            document.getElementById('<%= WorkSpaceCountTextBox.ClientID %>').addEventListener('input', updateUsernamePreview);
            document.getElementById('<%= RunningModeDropDown.ClientID %>').addEventListener('change', updateTimeoutVisibility);
        });
    </script>
</asp:Content>