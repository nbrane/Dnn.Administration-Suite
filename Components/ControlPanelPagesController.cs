using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using DotNetNuke.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using System.Linq;
using System.Web;

namespace nBrane.Modules.AdministrationSuite.Components
{
    [DnnAuthorize]
    public class ControlPanelPagesController : DnnApiController
    {

        [HttpPost]
        [DnnPageEditor]
        public HttpResponseMessage DeletePage(DTO.PageDetails page)
        {
            var apiResponse = new DTO.ApiResponse<DTO.SavePageResponse>();
            apiResponse.CustomObject = new DTO.SavePageResponse();

            try
            {
                var tabController = new DotNetNuke.Entities.Tabs.TabController();
                var tab = tabController.GetTab(page.Id, PortalSettings.PortalId);
                if (DotNetNuke.Security.Permissions.TabPermissionController.CanDeletePage(tab) && !DotNetNuke.Entities.Tabs.TabController.IsSpecialTab(tab.TabID, PortalSettings))
                {
                    if (tab.TabID == PortalSettings.ActiveTab.TabID)
                    {
                        apiResponse.CustomObject.Redirect = true;
                        apiResponse.CustomObject.Url = DotNetNuke.Common.Globals.NavigateURL(PortalSettings.HomeTabId);
                    }

                    apiResponse.Success = tabController.SoftDeleteTab(tab.TabID, PortalSettings); ;
                }
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }

        [HttpPost]
        [DnnPageEditor]
        public HttpResponseMessage SavePage(DTO.PageDetails page)
        {
            var apiResponse = new DTO.ApiResponse<DTO.SavePageResponse>();
            apiResponse.CustomObject = new DTO.SavePageResponse();
            try
            {
                //Validation:
                //Tab name is required
                //Tab name is invalid
                string invalidType;
                if (!DotNetNuke.Entities.Tabs.TabController.IsValidTabName(page.Name, out invalidType))
                {
                    switch (invalidType)
                    {
                        case "EmptyTabName":
                            apiResponse.Message = "Page name is required.";
                            break;
                        case "InvalidTabName":
                            apiResponse.Message = "Page name is invalid.";
                            break;
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                }

                var tc = new DotNetNuke.Entities.Tabs.TabController();
                var dnnTab = page.Id == -1 ? new DotNetNuke.Entities.Tabs.TabInfo() : tc.GetTab(page.Id, PortalSettings.PortalId);

                if (dnnTab != null)
                {
                    dnnTab.TabName = page.Name.Trim();

                    if (!string.IsNullOrWhiteSpace(page.Title))
                        dnnTab.Title = page.Title.Trim();

                    if (!string.IsNullOrWhiteSpace(page.Description))
                        dnnTab.Description = page.Description.Trim();

                    dnnTab.IsVisible = page.Visible;
                    dnnTab.DisableLink = page.Disabled;
                    dnnTab.IsSecure = page.Secure;

                    if (!string.IsNullOrWhiteSpace(page.Theme))
                        dnnTab.SkinSrc = page.Theme != "-1" ? page.Theme : string.Empty;

                    if (!string.IsNullOrWhiteSpace(page.Container))
                        dnnTab.ContainerSrc = page.Container != "-1" ? page.Container : string.Empty;

                    if (page.Id == -1) {
                        dnnTab.PortalID = PortalSettings.PortalId;
                        switch (page.PositionMode)
                        {
                            case "1":
                                page.Id = tc.AddTabAfter(dnnTab, int.Parse(page.Position));
                                break;
                            case "2":
                                page.Id = tc.AddTabBefore(dnnTab, int.Parse(page.Position));
                                break;
                            default:
                                page.Id = tc.AddTab(dnnTab);
                                break;
                        }
                        
                        apiResponse.CustomObject.Redirect = true;
                        apiResponse.CustomObject.Url = DotNetNuke.Common.Globals.NavigateURL(page.Id);
                    }
                    else {
                        tc.UpdateTab(dnnTab);
                        if (!string.IsNullOrWhiteSpace(page.Position) && !string.IsNullOrWhiteSpace(page.PositionMode))
                        {
                            var positionTabID = int.Parse(page.Position);
                            var positionModeInt = int.Parse(page.PositionMode);

                            var relativeTab = tc.GetTab(positionTabID, PortalSettings.PortalId);

                            // var parentTab = GetParentTab(relativeTab, (PagePositionMode)positionModeInt);

                            if (relativeTab != null)
                            {
                                switch (page.PositionMode)
                                {
                                    case "1":
                                        tc.MoveTabAfter(dnnTab, relativeTab.TabID);
                                        break;
                                    case "2":
                                        tc.MoveTabBefore(dnnTab, relativeTab.TabID);
                                        break;
                                }
                            }
                        }
                    }

                    apiResponse.Success = true;
                }
                
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage ListAllPages(int portalId)
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>> ();

            try
            {                
                apiResponse.CustomObject = new List<DTO.GenericListItem>();

                foreach (var tab in DotNetNuke.Entities.Tabs.TabController.GetPortalTabs(portalId, -1, true, true, false, false))
                {
                    var newGenericItem = new DTO.GenericListItem();
                    newGenericItem.Name = tab.IndentedTabName;
                    newGenericItem.Value = tab.TabID.ToString();
                    apiResponse.CustomObject.Add(newGenericItem);
                }

                apiResponse.Success = true;
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage LoadPageDetails(int id)
        {
            var apiResponse = new DTO.ApiResponse<DTO.PageDetails>();

            try
            {
                var tc = new DotNetNuke.Entities.Tabs.TabController();
                apiResponse.CustomObject = new DTO.PageDetails(tc.GetTab(id, PortalSettings.PortalId));
                apiResponse.CustomObject.LoadAllPages();
                apiResponse.CustomObject.LoadThemesAndContainers();

                apiResponse.Success = true;
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage ListPages(string parent)
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericPageListItem>>();

            try
            {
                var pageId = -2;
                var portalId = PortalSettings.PortalId;

                switch (parent.ToLower())
                {
                    case "admin":
                        pageId = PortalSettings.AdminTabId;
                        break;
                    case "host":
                        pageId = PortalSettings.SuperTabId;
                        portalId = -1;
                        break;
                    case "all":
                        pageId = -1;
                        break;
                    default:
                        int.TryParse(parent, out pageId);
                        break;  
                }
                if (pageId > -2)
                {
                    var listOfPages = DotNetNuke.Entities.Tabs.TabController.GetTabsByParent(pageId, portalId);
                    apiResponse.CustomObject = new List<DTO.GenericPageListItem>();

                    foreach (var page in listOfPages.Where(i => i.IsDeleted == false).OrderBy(i => i.TabOrder))
                    {
                        var newItem = new DTO.GenericPageListItem() { Value = page.TabID.ToString(), Name = page.TabName };

                        if (string.IsNullOrWhiteSpace(page.IconFileLarge) == false)
                        {
                            newItem.Image = VirtualPathUtility.ToAbsolute(page.IconFileLarge);
                        } else
                        {
                            newItem.Image = string.Empty;
                        }

                        newItem.HasChildren = page.HasChildren;

                        apiResponse.CustomObject.Add(newItem);
                    }

                    apiResponse.Success = true;

                    return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                }
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage ListParentPages(int id)
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericPageListItem>>();

            try
            {
                var portalId = PortalSettings.PortalId;
                var tab = new DotNetNuke.Entities.Tabs.TabController().GetTab(id, portalId);
                if (tab != null)
                {
                    var listOfPages = DotNetNuke.Entities.Tabs.TabController.GetTabsByParent(tab.ParentId, portalId);
                    apiResponse.CustomObject = new List<DTO.GenericPageListItem>();

                    foreach (var page in listOfPages.Where(i => i.IsDeleted == false).OrderBy(i => i.TabOrder))
                    {
                        var newItem = new DTO.GenericPageListItem() { Value = page.TabID.ToString(), Name = page.TabName };

                        if (string.IsNullOrWhiteSpace(page.IconFileLarge) == false)
                        {
                            newItem.Image = VirtualPathUtility.ToAbsolute(page.IconFileLarge);
                        }
                        else
                        {
                            newItem.Image = string.Empty;
                        }

                        newItem.HasChildren = page.HasChildren;

                        apiResponse.CustomObject.Add(newItem);
                    }

                    apiResponse.Success = true;

                    return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                }
                
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }
    }
}
