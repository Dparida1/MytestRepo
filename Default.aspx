<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WorkSpaceManager._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h2">
            <i class="fas fa-tachometer-alt text-primary"></i>
            Dashboard
        </h1>
        
        <div class="btn-group" role="group">
            <asp:Button ID="SyncDataButton" runat="server" 
                CssClass="btn btn-outline-primary" 
                Text="Sync from AWS" 
                OnClick="SyncDataButton_Click" />
            <asp:Button ID="RefreshButton" runat="server" 
                CssClass="btn btn-outline-secondary" 
                Text="Refresh" 
                OnClick="RefreshButton_Click" />
        </div>
    </div>

    <!-- Stats Cards -->
    <div class="row g-4 mb-4">
        <div class="col-lg-3 col-md-6">
            <div class="card stats-card border-0 shadow-sm h-100">
                <div class="card-body text-center">
                    <div class="display-6 text-primary mb-2">
                        <i class="fas fa-desktop"></i>
                    </div>
                    <h3 class="card-title mb-1">
                        <asp:Label ID="TotalWorkSpacesLabel" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="card-text text-muted">Total WorkSpaces</p>
                </div>
            </div>
        </div>

        <div class="col-lg-3 col-md-6">
            <div class="card stats-card border-0 shadow-sm h-100">
                <div class="card-body text-center">
                    <div class="display-6 text-success mb-2">
                        <i class="fas fa-play-circle"></i>
                    </div>
                    <h3 class="card-title mb-1">
                        <asp:Label ID="RunningWorkSpacesLabel" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="card-text text-muted">Running</p>
                </div>
            </div>
        </div>

        <div class="col-lg-3 col-md-6">
            <div class="card stats-card border-0 shadow-sm h-100">
                <div class="card-body text-center">
                    <div class="display-6 text-secondary mb-2">
                        <i class="fas fa-stop-circle"></i>
                    </div>
                    <h3 class="card-title mb-1">
                        <asp:Label ID="StoppedWorkSpacesLabel" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="card-text text-muted">Stopped</p>
                </div>
            </div>
        </div>

        <div class="col-lg-3 col-md-6">
            <div class="card stats-card border-0 shadow-sm h-100">
                <div class="card-body text-center">
                    <div class="display-6 text-warning mb-2">
                        <i class="fas fa-clock"></i>
                    </div>
                    <h3 class="card-title mb-1">
                        <asp:Label ID="PendingWorkSpacesLabel" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="card-text text-muted">Pending</p>
                </div>
            </div>
        </div>
    </div>

    <!-- Quick Actions -->
    <div class="row g-4 mb-4">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-header bg-light">
                    <h5 class="card-title mb-0">
                        <i class="fas fa-bolt text-warning"></i>
                        Quick Actions
                    </h5>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6 col-lg-3">
                            <asp:HyperLink ID="CreateWorkSpacesQuickLink" runat="server" 
                                NavigateUrl="~/CreateWorkSpaces.aspx" 
                                CssClass="btn btn-primary w-100">
                                <i class="fas fa-plus"></i> Create WorkSpaces
                            </asp:HyperLink>
                        </div>
                        <div class="col-md-6 col-lg-3">
                            <asp:HyperLink ID="ViewWorkSpacesQuickLink" runat="server" 
                                NavigateUrl="~/ViewWorkSpaces.aspx" 
                                CssClass="btn btn-outline-primary w-100">
                                <i class="fas fa-list"></i> View All WorkSpaces
                            </asp:HyperLink>
                        </div>
                        <div class="col-md-6 col-lg-3">
                            <asp:HyperLink ID="ReportsQuickLink" runat="server" 
                                NavigateUrl="~/Reports.aspx" 
                                CssClass="btn btn-outline-secondary w-100">
                                <i class="fas fa-chart-bar"></i> View Reports
                            </asp:HyperLink>
                        </div>
                        <div class="col-md-6 col-lg-3">
                            <button type="button" class="btn btn-outline-info w-100" data-bs-toggle="modal" data-bs-target="#syncModal">
                                <i class="fas fa-sync"></i> Sync AWS Data
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Recent Activity -->
    <div class="row g-4">
        <!-- Recent WorkSpaces -->
        <div class="col-lg-8">
            <div class="card border-0 shadow-sm h-100">
                <div class="card-header bg-light d-flex justify-content-between align-items-center">
                    <h5 class="card-title mb-0">
                        <i class="fas fa-history text-info"></i>
                        Recent WorkSpaces
                    </h5>
                    <asp:HyperLink ID="ViewAllWorkSpacesLink" runat="server" 
                        NavigateUrl="~/ViewWorkSpaces.aspx" 
                        CssClass="btn btn-sm btn-outline-primary">
                        View All
                    </asp:HyperLink>
                </div>
                <div class="card-body">
                    <asp:Repeater ID="RecentWorkSpacesRepeater" runat="server">
                        <HeaderTemplate>
                            <div class="table-responsive">
                                <table class="table table-hover">
                                    <thead>
                                        <tr>
                                            <th>WorkSpace ID</th>
                                            <th>User Name</th>
                                            <th>State</th>
                                            <th>Created</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <code><%# Eval("WorkSpaceId") %></code>
                                </td>
                                <td><%# Eval("UserName") %></td>
                                <td>
                                    <span class="badge bg-<%# GetStateBootstrapClass(Eval("State").ToString()) %>">
                                        <%# WorkSpaceStates.GetDisplayText(Eval("State").ToString()) %>
                                    </span>
                                </td>
                                <td><%# ((DateTime)Eval("CreatedDate")).ToString("MMM dd, HH:mm") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                    </tbody>
                                </table>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                    
                    <asp:Panel ID="NoWorkSpacesPanel" runat="server" Visible="false" CssClass="text-center py-4">
                        <div class="text-muted">
                            <i class="fas fa-inbox fa-3x mb-3"></i>
                            <p>No WorkSpaces found. <asp:HyperLink ID="CreateFirstWorkSpaceLink" runat="server" NavigateUrl="~/CreateWorkSpaces.aspx">Create your first WorkSpace</asp:HyperLink>.</p>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <!-- Recent Requests -->
        <div class="col-lg-4">
            <div class="card border-0 shadow-sm h-100">
                <div class="card-header bg-light">
                    <h5 class="card-title mb-0">
                        <i class="fas fa-tasks text-success"></i>
                        Recent Requests
                    </h5>
                </div>
                <div class="card-body">
                    <asp:Repeater ID="RecentRequestsRepeater" runat="server">
                        <ItemTemplate>
                            <div class="d-flex justify-content-between align-items-start mb-3 pb-3 border-bottom">
                                <div class="flex-grow-1">
                                    <h6 class="mb-1"><%# Eval("RequestName") %></h6>
                                    <small class="text-muted">
                                        <%# Eval("RequestedCount") %> requested, 
                                        <%# Eval("CreatedCount") %> created
                                    </small>
                                    <br>
                                    <small class="text-muted">
                                        <%# ((DateTime)Eval("CreatedDate")).ToString("MMM dd, HH:mm") %>
                                    </small>
                                </div>
                                <span class="badge bg-<%# GetRequestStatusBootstrapClass(Eval("Status").ToString()) %>">
                                    <%# Eval("Status") %>
                                </span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                    <asp:Panel ID="NoRequestsPanel" runat="server" Visible="false" CssClass="text-center py-4">
                        <div class="text-muted">
                            <i class="fas fa-clipboard-list fa-2x mb-2"></i>
                            <p>No recent requests.</p>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <!-- Sync Modal -->
    <div class="modal fade" id="syncModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <i class="fas fa-sync text-primary"></i>
                        Sync Data from AWS
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p>This will sync the following data from your AWS account:</p>
                    <ul>
                        <li>WorkSpace Bundles</li>
                        <li>Directory Services</li>
                        <li>Existing WorkSpaces</li>
                    </ul>
                    
                    <div class="mb-3">
                        <label for="syncRegionSelect" class="form-label">AWS Region</label>
                        <asp:DropDownList ID="SyncRegionDropDown" runat="server" CssClass="form-select">
                            <asp:ListItem Value="us-east-1" Text="US East (N. Virginia)" Selected="True"></asp:ListItem>
                            <asp:ListItem Value="us-west-2" Text="US West (Oregon)"></asp:ListItem>
                            <asp:ListItem Value="eu-west-1" Text="Europe (Ireland)"></asp:ListItem>
                            <asp:ListItem Value="ap-southeast-1" Text="Asia Pacific (Singapore)"></asp:ListItem>
                            <asp:ListItem Value="ap-southeast-2" Text="Asia Pacific (Sydney)"></asp:ListItem>
                            <asp:ListItem Value="ap-northeast-1" Text="Asia Pacific (Tokyo)"></asp:ListItem>
                            <asp:ListItem Value="ca-central-1" Text="Canada (Central)"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle"></i>
                        This process may take a few moments depending on the amount of data in your AWS account.
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <asp:Button ID="ModalSyncButton" runat="server" 
                        CssClass="btn btn-primary" 
                        Text="Start Sync" 
                        OnClick="ModalSyncButton_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>