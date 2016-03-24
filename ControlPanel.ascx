<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ControlPanel.ascx.cs" Inherits="nBrane.Modules.AdministrationSuite.ControlPanel" %>
<div id="nbr-admin-suite" class="nbr-admin-suite">
    <ul class="nbr-upper-control-panel">
        <li><i class="fa fa-sign-in"></i><span>Revert User Impersonation</span></li>
        <li><i class="fa fa-sign-out"></i><span>Logout</span></li>

        <li><i class="fa fa-edit"></i><span>Switch to Edit</span></li>
        <li><span>Switch to View</span></li>
        <li><span>Switch to Layout</span></li>
    </ul>

    <ul class="nbr-control-panel" data-bind="click:Load">
        
        <li data-action="Modules">
            <i class="fa fa-image"></i>
            <span>Modules</span>
        </li>
        <li data-action="Pages">
            <i class="fa fa-file-text"></i>
            <span>Pages</span>
        </li>
        <li data-action="Users"><i class="fa fa-users"></i><span>Users</span></li>
        <li data-action="Site"><i class="fa fa-cog"></i><span>Site</span></li>
        <li data-action="Host"><i class="fa fa-fort-awesome"></i><span>Host</span></li>
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
