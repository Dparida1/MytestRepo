<%@ Page Title="View WorkSpaces" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ViewWorkSpaces.aspx.cs" Inherits="WorkSpaceManager.ViewWorkSpaces" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h2">
            <i class="fas fa-desktop text-primary"></i>
            WorkSpaces
        </h1>
        
        <div class="btn-group" role="group">
            <asp:HyperLink ID="CreateNewLink" runat="server" 
                NavigateUrl="~/CreateWorkSpaces.aspx" 
                CssClass="btn btn-primary">
                <i class="fas fa-plus"></i> Create New
            </asp:HyperLink>
            <asp:Button ID="RefreshButton" runat="server" 
                CssClass="btn btn-outline-secondary" 
                Text="Refresh" 
                OnClick="RefreshButton_Click" />
        </div>
    </div>

    <!-- Filters -->
    <div class="card border-0 shadow-sm mb-4">
        <div class="card-body">
            <div class="row g-3 align-items-end">
                <div class="col-md-3">
                    <label for="StateFilterDropDown" class="form-label">Filter by State</label>
                    <asp:DropDownList ID="StateFilterDropDown" runat="server" 
                        CssClass="form-select" 
                        AutoPostBack="true" 
                        OnSelectedIndexChanged="StateFilterDropDown_SelectedIndexChanged">
                        <asp:ListItem Value="" Text="All States" Selected="True"></asp:ListItem>
                        <asp:ListItem Value="AVAILABLE" Text="Available"></asp:ListItem>
                        <asp:ListItem Value="STOPPED" Text="Stopped"></asp:ListItem>
                        <asp:ListItem Value="PENDING" Text="Pending"></asp:ListItem>
                        <asp:ListItem Value="ERROR" Text="Error"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label for="RegionFilterDropDown" class="form-label">Filter by Region</label>
                    <asp:DropDownList ID="RegionFilterDropDown" runat="server" 
                        CssClass="form-select" 
                        AutoPostBack="true" 
                        OnSelectedIndexChanged="RegionFilterDropDown_SelectedIndexChanged">
                        <asp:ListItem Value="" Text="All Regions" Selected="True"></asp:ListItem>
                        <asp:ListItem Value="us-east-1" Text="US East (N. Virginia)"></asp:ListItem>
                        <asp:ListItem Value="us-west-2" Text="US West (Oregon)"></asp:ListItem>
                        <asp:ListItem Value="eu-west-1" Text="Europe (Ireland)"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-4">
                    <label for="SearchTextBox" class="form-label">Search</label>
                    <div class="input-group">
                        <asp:TextBox ID="SearchTextBox" runat="server" 
                            CssClass="form-control" 
                            placeholder="Search by WorkSpace ID or Username"></asp:TextBox>
                        <asp:Button ID="SearchButton" runat="server" 
                            CssClass="btn btn-outline-secondary" 
                            Text="Search" 
                            OnClick="SearchButton_Click" />
                    </div>
                </div>
                <div class="col-md-2">
                    <asp:Button ID="ClearFiltersButton" runat="server" 
                        CssClass="btn btn-outline-info w-100" 
                        Text="Clear Filters" 
                        OnClick="ClearFiltersButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- WorkSpaces Table -->
    <div class="card border-0 shadow-sm">
        <div class="card-header bg-light d-flex justify-content-between align-items-center">
            <h5 class="card-title mb-0">
                <i class="fas fa-list text-info"></i>
                All WorkSpaces
                <asp:Label ID="WorkSpaceCountLabel" runat="server" CssClass="badge bg-secondary ms-2"></asp:Label>
            </h5>
            
            <div class="btn-group btn-group-sm">
                <button type="button" class="btn btn-outline-success" onclick="bulkAction('start')">
                    <i class="fas fa-play"></i> Start Selected
                </button>
                <button type="button" class="btn btn-outline-warning" onclick="bulkAction('stop')">
                    <i class="fas fa-stop"></i> Stop Selected
                </button>
                <button type="button" class="btn btn-outline-info" onclick="bulkAction('reboot')">
                    <i class="fas fa-redo"></i> Reboot Selected
                </button>
            </div>
        </div>
        <div class="card-body p-0">
            <asp:GridView ID="WorkSpacesGridView" runat="server" 
                CssClass="table table-hover mb-0" 
                AutoGenerateColumns="false" 
                DataKeyNames="WorkSpaceId"
                EmptyDataText="No WorkSpaces found."
                OnRowCommand="WorkSpacesGridView_RowCommand"
                OnRowDataBound="WorkSpacesGridView_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="">
                        <HeaderTemplate>
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" id="selectAll" onclick="toggleAllCheckboxes(this)">
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <div class="form-check">
                                <input class="form-check-input workspace-checkbox" type="checkbox" 
                                       value='<%# Eval("WorkSpaceId") %>' data-workspace-id='<%# Eval("WorkSpaceId") %>'>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="WorkSpace ID">
                        <ItemTemplate>
                            <code class="small"><%# Eval("WorkSpaceId") %></code>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:BoundField DataField="UserName" HeaderText="User Name" />
                    
                    <asp:TemplateField HeaderText="State">
                        <ItemTemplate>
                            <span class="badge bg-<%# GetStateBootstrapClass(Eval("State").ToString()) %>">
                                <%# WorkSpaceStates.GetDisplayText(Eval("State").ToString()) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:BoundField DataField="Region" HeaderText="Region" />
                    
                    <asp:TemplateField HeaderText="IP Address">
                        <ItemTemplate>
                            <%# string.IsNullOrEmpty(Eval("IpAddress")?.ToString()) ? "-" : Eval("IpAddress") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Created">
                        <ItemTemplate>
                            <%# ((DateTime)Eval("CreatedDate")).ToString("MMM dd, yyyy") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="btn-group btn-group-sm">
                                <asp:Button ID="StartButton" runat="server" 
                                    CssClass="btn btn-outline-success btn-sm" 
                                    Text="Start" 
                                    CommandName="StartWorkSpace" 
                                    CommandArgument='<%# Eval("WorkSpaceId") %>'
                                    OnClientClick="return confirmWorkSpaceOperation('start', 1);" />
                                <asp:Button ID="StopButton" runat="server" 
                                    CssClass="btn btn-outline-warning btn-sm" 
                                    Text="Stop" 
                                    CommandName="StopWorkSpace" 
                                    CommandArgument='<%# Eval("WorkSpaceId") %>'
                                    OnClientClick="return confirmWorkSpaceOperation('stop', 1);" />
                                <asp:Button ID="RebootButton" runat="server" 
                                    CssClass="btn btn-outline-info btn-sm" 
                                    Text="Reboot" 
                                    CommandName="RebootWorkSpace" 
                                    CommandArgument='<%# Eval("WorkSpaceId") %>'
                                    OnClientClick="return confirmWorkSpaceOperation('reboot', 1);" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center py-5">
                        <i class="fas fa-inbox fa-3x text-muted mb-3"></i>
                        <h5 class="text-muted">No WorkSpaces Found</h5>
                        <p class="text-muted">Start by creating your first WorkSpace.</p>
                        <asp:HyperLink ID="CreateFirstWorkSpaceLink" runat="server" 
                            NavigateUrl="~/CreateWorkSpaces.aspx" 
                            CssClass="btn btn-primary">
                            <i class="fas fa-plus"></i> Create WorkSpace
                        </asp:HyperLink>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <!-- JavaScript for bulk operations -->
    <script>
        function toggleAllCheckboxes(source) {
            var checkboxes = document.querySelectorAll('.workspace-checkbox');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = source.checked;
            }
        }

        function getSelectedWorkSpaces() {
            var selected = [];
            var checkboxes = document.querySelectorAll('.workspace-checkbox:checked');
            for (var i = 0; i < checkboxes.length; i++) {
                selected.push(checkboxes[i].getAttribute('data-workspace-id'));
            }
            return selected;
        }

        function bulkAction(action) {
            var selected = getSelectedWorkSpaces();
            
            if (selected.length === 0) {
                alert('Please select one or more WorkSpaces.');
                return;
            }

            if (!confirmWorkSpaceOperation(action, selected.length)) {
                return;
            }

            // Create a form to submit the bulk action
            var form = document.createElement('form');
            form.method = 'POST';
            form.action = window.location.href;

            // Add action parameter
            var actionInput = document.createElement('input');
            actionInput.type = 'hidden';
            actionInput.name = 'bulkAction';
            actionInput.value = action;
            form.appendChild(actionInput);

            // Add selected workspace IDs
            for (var i = 0; i < selected.length; i++) {
                var workspaceInput = document.createElement('input');
                workspaceInput.type = 'hidden';
                workspaceInput.name = 'selectedWorkSpaces';
                workspaceInput.value = selected[i];
                form.appendChild(workspaceInput);
            }

            document.body.appendChild(form);
            form.submit();
        }
    </script>
</asp:Content>