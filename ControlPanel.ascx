<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ControlPanel.ascx.cs" Inherits="nBrane.Modules.AdministrationSuite.ControlPanel" %>
<div id="nbr-admin-suite" class="nbr-admin-suite">
    <ul class="nbr-upper-control-panel">
<% if (IsUserImpersonated()) { %>
        <li><i class="fa fa-sign-in"></i> <span>Revert User Impersonation</span></li>
<% } %>
        <li data-bind="click:Logoff"><i class="fa fa-sign-out"></i> <span data-bind="restext: 'Logoff'">Logout</span></li>
<% if (PortalSettings.UserMode.ToString().ToLower() != "view") { %>
        <li data-bind="click:SwitchInto" data-action="VIEW"><i class="fa fa-eye"></i> <span data-bind="restext: 'SwitchToView'"></span></li>
<% } if (PortalSettings.UserMode.ToString().ToLower() != "edit") { %>
        <li data-bind="click:SwitchInto" data-action="EDIT"><i class="fa fa-edit"></i> <span data-bind="restext: 'SwitchToEdit'"></span></li>
<% } if (PortalSettings.UserMode.ToString().ToLower() != "layout") { %>
        <li data-bind="click:SwitchInto" data-action="LAYOUT"><i class="fa fa-wrench"></i> <span data-bind="restext: 'SwitchToLayout'"></span></li>
<% } %>
<% if (ShowCachePanel() || !ShowCachePanel()) { %>
		<li data-action="Cache" data-bind="click:Load"><i class="fa fa-info-circle"></i> <span data-bind="restext: 'Tools'"></span></li>
<% } %>
    </ul>
	
	<script type="text/javascript">
	   var controlPanelPortalId = <%= PortalSettings.Current.PortalId %>;
	   var controlPanelTabId = <%= PortalSettings.Current.ActiveTab.TabID %>;
	   var controlPanelPanes = <%= PortalSettings.Current.ActiveTab.Panes.ToJson() %>;
	</script>

    <ul class="nbr-control-panel" data-bind="click:Load">
        <li data-action="Modules"><i class="fa fa-image"></i> <span data-bind="restext: 'Modules'"></span></li>
        <li data-action="Pages" data-subaction="all"><i class="fa fa-file-text"></i> <span data-bind="restext: 'Pages'"></span></li>
        <li data-action="Users"><i class="fa fa-users"></i><span data-bind="restext: 'Users'"></span></li>
        <li data-action="Pages" data-subaction="admin"><i class="fa fa-cog"></i><span data-bind="restext: 'Site'"></span></li>
        <li data-action="Pages" data-subaction="host"><i class="fa fa-fort-awesome"></i><span data-bind="restext: 'Host'"></span></li>
    </ul>

    <div class="nbr-admin-suite-loading">
        <img src="<%: ResolveUrl("images/robot_loop.gif") %>"/>
        <h2 id="LoadingTitle" class="nbr_paneltitle">LOADING</h2>
        <span id="LoadingMessage">PLEASE WAIT</span>
        <span id="LoadingStep"></span>
    </div>   
    
    <div class="nbr-admin-suite-confirm">
        <img src="<%: ResolveUrl("images/robot_yellow.png") %>"/>
        <h2 id="messagePlaceholder"></h2>
        <span id="subMessagePlaceholder"></span>
        <br/><br/>
        <p>
            <button class="btn btn-primary" id="yesButton">Yes</button>
            <button class="btn btn-default" id="noButton">No</button>
        </p>
    </div>
</div>
