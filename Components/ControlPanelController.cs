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

namespace nBrane.Modules.AdministrationSuite.Components
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("nBrane/AdministrationSuite", "default", "{controller}/{action}", new[] { "nBrane.Modules.AdministrationSuite.Components" });
        }
    }

    [DnnAuthorize]
    public class ControlPanelController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage Load(string Name)
        {
            var apiResponse = new DTO.ApiTemplateResponse();

            //1=control, 2=javascript. 
            var controlName = string.Empty;

            switch (Name.ToLower())
            {
                case "pages":
                    controlName = "Main.Pages";
                    break;
                case "users":
                    controlName = "Main.Users";
                    break;
                case "modules":
                    controlName = "Main.Modules";
                    break;
                case "Site":
                    controlName = "Main.Site";
                    break;
                case "Host":
                    controlName = "Main.Host";
                    break;
                default:

                    break;
            }

            if (string.IsNullOrEmpty(controlName))
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, false);
            }

            var fileName = DotNetNuke.Common.Globals.ApplicationMapPath + "\\desktopmodules\\nbrane\\administrationsuite\\controls\\"+ controlName + ".html";

            if (!System.IO.File.Exists(fileName))
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, false);
            }

            var fileContents = Regex.Replace(System.IO.File.ReadAllText(fileName), @"[\r\n\t ]+", " ");
            fileContents = System.Web.HttpUtility.HtmlEncode(fileContents);

            apiResponse.HTML = fileContents;
            
            fileName = DotNetNuke.Common.Globals.ApplicationMapPath + "\\desktopmodules\\nbrane\\administrationsuite\\controls\\js\\" + controlName + ".js";
            fileContents = Regex.Replace(System.IO.File.ReadAllText(fileName), @"[\r\n\t ]+", " ");

            apiResponse.JS = fileContents;
            apiResponse.Success = true;

            var response = Request.CreateResponse(HttpStatusCode.OK, apiResponse);

            return response;
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage Logoff()
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            try
            {
                var ps = new DotNetNuke.Security.PortalSecurity();
                ps.SignOut();

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
        public HttpResponseMessage SetUserMode(string mode)
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            try
            {
                switch (mode.ToLower())
                {
                    case "view":
                    case "edit":
                    case "layout":
                        var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
                        var personalization = personalizationController.LoadProfile(UserInfo.UserID, PortalSettings.PortalId);
                        personalization.Profile["Usability:UserMode" + PortalSettings.PortalId] = mode.ToUpper();
                        personalization.IsModified = true;
                        personalizationController.SaveProfile(personalization);

                        apiResponse.Success = true;
                        break;
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
            var apiResponse = new DTO.ApiResponse<bool>();

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

                    if (!string.IsNullOrWhiteSpace(page.Theme))
                        dnnTab.SkinSrc = page.Theme;

                    if (!string.IsNullOrWhiteSpace(page.Container))
                        dnnTab.ContainerSrc = page.Container;

                    if (page.Id == -1) {
                        dnnTab.PortalID = PortalSettings.PortalId;
                        switch (page.PositionMode)
                        {
                            case "1":
                                tc.AddTabAfter(dnnTab, int.Parse(page.Position));
                                break;
                            case "2":
                                tc.AddTabBefore(dnnTab, int.Parse(page.Position));
                                break;
                            default:
                                tc.AddTab(dnnTab);
                                break;
                        }
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
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListImageItem>>();

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
                        //todo, parse for int and get by parent id
                        break;  
                }
                if (pageId > -2)
                {
                    var listOfPages = DotNetNuke.Entities.Tabs.TabController.GetTabsByParent(pageId, portalId);
                    apiResponse.CustomObject = new List<DTO.GenericListImageItem>();

                    foreach (var page in listOfPages.OrderBy(i => i.TabOrder))
                    {
                        var newItem = new DTO.GenericListImageItem() { Value = page.TabID.ToString(), Name = page.TabName };

                        if (string.IsNullOrWhiteSpace(page.IconFileLarge) == false)
                        {
                            newItem.Image = System.Web.VirtualPathUtility.ToAbsolute(page.IconFileLarge);
                        } else
                        {
                            newItem.Image = string.Empty;
                        }

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
        public HttpResponseMessage ListModules(string category)
        {
            var apiResponse = new DTO.ApiModuleResponse();

            try
            {
                var listOfModules = DotNetNuke.Entities.Modules.DesktopModuleController.GetPortalDesktopModules(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId).Values;

                apiResponse.Modules = new List<DTO.GenericListItem>();

                foreach (var module in listOfModules)
                {
                    apiResponse.Modules.Add(new DTO.GenericListItem() { Value = module.DesktopModuleID.ToString(), Name = module.FriendlyName });
                }

                apiResponse.Panes = new List<DTO.GenericListItem>();

                apiResponse.DefaultModuleLocation = DotNetNuke.Common.Globals.glbDefaultPane;

                apiResponse.Containers = ListContainers("host", "containers");

                apiResponse.Success = true;

                return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
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
        public HttpResponseMessage ListUsers(string filter)
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>>();

            try
            {
                var listOfUsers = Data.SearchForUsers(PortalSettings.PortalId, filter, 1, 15);

                apiResponse.CustomObject = new List<DTO.GenericListItem>();

                apiResponse.CustomObject.Add(new DTO.GenericListItem() { Value = "0", Name = "Anonymous User" });

                foreach (var user in listOfUsers)
                {
                    apiResponse.CustomObject.Add(new DTO.GenericListItem() { Value = user.UserId.ToString(), Name = user.DisplayName });
                }
                apiResponse.Success = true;

                return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
        }


        private DotNetNuke.Entities.Tabs.TabInfo GetParentTab(DotNetNuke.Entities.Tabs.TabInfo relativeToTab, DTO.PagePositionMode location)
        {
            if (relativeToTab == null)
            {
                return null;
            }

            var tabCtrl = new DotNetNuke.Entities.Tabs.TabController();
            DotNetNuke.Entities.Tabs.TabInfo parentTab = null;

            if (location == DTO.PagePositionMode.ChildOf)
            {
                parentTab = relativeToTab;
            }
            else if ((relativeToTab != null) && relativeToTab.ParentId != Null.NullInteger)
            {
                parentTab = tabCtrl.GetTab(relativeToTab.ParentId, relativeToTab.PortalID, false);
            }

            return parentTab;
        }
        
        internal static List<DTO.GenericSelectableListItem> ListContainers(string HostOrSite, string SkinOrContainer)
        {
            var apiResponse = new List<DTO.GenericSelectableListItem>();

            try
            {
                string strRoot = string.Empty;
                string strFolder = null;
                string[] arrFolders = null;
                string strFile = null;
                string[] arrFiles = null;
                string strLastFolder = string.Empty;
                string strSeparator = "----------------------------------------";

                string dbPrefix = string.Empty;
                string currentSetting = string.Empty;

                switch (HostOrSite.ToLower())
                {
                    case "host":
                        if (SkinOrContainer.ToLower() == "skin")
                        {
                            strRoot = DotNetNuke.Common.Globals.HostMapPath + DotNetNuke.UI.Skins.SkinController.RootSkin;
                            dbPrefix = "[G]" + DotNetNuke.UI.Skins.SkinController.RootSkin;
                        }
                        else {
                            strRoot = DotNetNuke.Common.Globals.HostMapPath + DotNetNuke.UI.Skins.SkinController.RootContainer;
                            dbPrefix = "[G]" + DotNetNuke.UI.Skins.SkinController.RootContainer;
                        }
                        break;
                    case "site":
                        if (SkinOrContainer.ToLower() == "skin")
                        {
                            strRoot = DotNetNuke.Entities.Portals.PortalSettings.Current.HomeDirectoryMapPath + DotNetNuke.UI.Skins.SkinController.RootSkin;
                            dbPrefix = "[L]" + DotNetNuke.UI.Skins.SkinController.RootSkin;
                        }
                        else {
                            strRoot = DotNetNuke.Entities.Portals.PortalSettings.Current.HomeDirectoryMapPath + DotNetNuke.UI.Skins.SkinController.RootContainer;
                            dbPrefix = "[L]" + DotNetNuke.UI.Skins.SkinController.RootContainer;
                        }
                        break;
                }

                if (SkinOrContainer.ToLower() == "skin")
                {
                    var currentDefault = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.SkinSrc;
                    if (string.IsNullOrWhiteSpace(currentDefault))
                    {
                        currentDefault = DotNetNuke.Entities.Portals.PortalSettings.Current.DefaultPortalSkin;
                    }

                    currentSetting = GetFriendySkinName(currentDefault);
                }
                else {
                    var currentDefault = DotNetNuke.Entities.Portals.PortalSettings.Current.ActiveTab.ContainerSrc;
                    if (string.IsNullOrWhiteSpace(currentDefault))
                    {
                        currentDefault = DotNetNuke.Entities.Portals.PortalSettings.Current.DefaultPortalContainer;
                    }

                    currentSetting = GetFriendySkinName(currentDefault);
                }

                if (string.IsNullOrEmpty(strRoot) == false && System.IO.Directory.Exists(strRoot))
                {
                    apiResponse = new List<DTO.GenericSelectableListItem>();
                    arrFolders = System.IO.Directory.GetDirectories(strRoot);
                    foreach (string strFolder_loopVariable in arrFolders)
                    {
                        strFolder = strFolder_loopVariable;
                        arrFiles = System.IO.Directory.GetFiles(strFolder, "*.ascx");
                        foreach (string strFile_loopVariable in arrFiles)
                        {
                            strFile = strFile_loopVariable;
                            strFolder = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);

                            //if (strLastFolder != strFolder)
                            //{
                            //    if (string.IsNullOrEmpty(strLastFolder) == false)
                            //    {
                            //        apiResponse.Add(new DTO.GenericSelectableListItem(strSeparator, "", false));
                            //    }
                            //    strLastFolder = strFolder;
                            //}
                            string skinName = FormatSkinName(strFolder, System.IO.Path.GetFileNameWithoutExtension(strFile)).Replace("_", " ");
                            bool isSelected = (bool)(skinName == currentSetting ? true : false);

                            apiResponse.Add(new DTO.GenericSelectableListItem(skinName, dbPrefix + "/" + strFolder + "/" + System.IO.Path.GetFileName(strFile), isSelected));
                        }
                    }
                }


                if (apiResponse.Count > 0)
                {
                    apiResponse.Insert(0, new DTO.GenericSelectableListItem(strSeparator, "", false));
                    apiResponse.Insert(0, new DTO.GenericSelectableListItem("Default - " + currentSetting, "-1", false));
                }
                else {
                    apiResponse.Insert(0, new DTO.GenericSelectableListItem("ContainerNoneAvailable", "-1", false));
                }
            }
            catch (Exception err)
            {
                Exceptions.LogException(err);
            }

            return apiResponse;
        }

        internal static string GetFriendySkinName(string param)
        {
            if (!string.IsNullOrWhiteSpace(param))
            {
                param = DotNetNuke.UI.Skins.SkinController.FormatSkinSrc(param, DotNetNuke.Entities.Portals.PortalSettings.Current);

                return System.IO.Path.GetDirectoryName(param).Split(System.IO.Path.DirectorySeparatorChar).Last() + " - " + System.IO.Path.GetFileNameWithoutExtension(param).Replace("_", " ");
            }

            return null;
        }

        private static string FormatSkinName(string strSkinFolder, string strSkinFile)
        {
            if (strSkinFolder.ToLower() == "_default")
            {
                // host folder
                return strSkinFile;
                // portal folder
            }
            else {
                switch (strSkinFile.ToLower())
                {
                    case "skin":
                    case "container":
                    case "default":
                        return strSkinFolder;
                    default:
                        return strSkinFolder + " - " + strSkinFile;
                }
            }
        }

    }
}
