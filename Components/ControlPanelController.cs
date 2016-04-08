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
                case "site":
                    controlName = "Main.Site";
                    break;
                case "host":
                    controlName = "Main.Host";
                    break;
                case "cache":
                    controlName = "Main.Cache";
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
        public HttpResponseMessage Impersonate(int Id)
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            Cookie cookie = null;
            try
            {
                cookie = Common.GenerateImpersonationCookie(UserInfo.UserID, Id);

                if (Id == 0)
                {
                    var objPortalSecurity = new DotNetNuke.Security.PortalSecurity();
                    objPortalSecurity.SignOut();
                }
                else {
                    var targetUserInfo = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, Id);

                    if (targetUserInfo != null)
                    {
                        DataCache.ClearUserCache(PortalSettings.PortalId, PortalSettings.UserInfo.Username);

                        var objPortalSecurity = new DotNetNuke.Security.PortalSecurity();
                        objPortalSecurity.SignOut();

                        DotNetNuke.Entities.Users.UserController.UserLogin(PortalSettings.PortalId, targetUserInfo, PortalSettings.PortalName, HttpContext.Current.Request.UserHostAddress, false);
                    }
                }

                apiResponse.Success = true;
            }
            catch (Exception err)
            {
                apiResponse.Success = false;
                apiResponse.Message = err.Message;

                Exceptions.LogException(err);
            }

            var actualResponse = Request.CreateResponse(HttpStatusCode.OK, apiResponse);

            actualResponse.Headers.SetCookie(cookie);

            return actualResponse;
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
        public HttpResponseMessage SaveModule(DTO.ModuleDetails module)
        {
            var apiResponse = new DTO.ApiResponse<int>();

            try
            {
                int iPosition = -1;
                switch (module.Position.ToUpper())
                {
                    case "TOP":
                        iPosition = 0;
                        break;
                    case "ABOVE":
                        if (string.IsNullOrEmpty(module.ModuleInstance) == false)
                        {
                            iPosition = int.Parse(module.ModuleInstance) - 1;
                        }
                        break;
                    case "BELOW":
                        if (string.IsNullOrEmpty(module.ModuleInstance) == false)
                        {
                            iPosition = int.Parse(module.ModuleInstance) + 1;
                        }
                        break;
                    case "BOTTOM":
                        iPosition = -1;
                        break;
                }

                if (module.CreateAs == "copy")
                {
                    if (module.Container == "-1")
                    {
                        module.Container = string.Empty;
                    }

                    Common.AddModuleCopy(module.ModuleId, module.PageId, iPosition, module.Location, module.Container);
                    apiResponse.Success = true;
                }
                else if (module.CreateAs == "link")
                {
                    if (module.Container == "-1")
                    {
                        module.Container = string.Empty;
                    }

                    Common.AddExistingModule(module.ModuleId, module.PageId, module.Location, iPosition, "", module.Container);
                    apiResponse.Success = true;
                }
                else
                {
                    List<int> lstNewModules = new List<int>();

                    var objTabPermissions = PortalSettings.ActiveTab.TabPermissions;
                    var objPermissionController = new DotNetNuke.Security.Permissions.PermissionController();
                    var objModules = new DotNetNuke.Entities.Modules.ModuleController();

                    var objEventLog = new DotNetNuke.Services.Log.EventLog.EventLogController();
                    int j = 0;

                    try
                    {
                        DotNetNuke.Entities.Modules.DesktopModuleInfo desktopModule = null;
                        if (!DotNetNuke.Entities.Modules.DesktopModuleController.GetDesktopModules(PortalSettings.PortalId).TryGetValue(module.Module, out desktopModule))
                        {
                            apiResponse.Message = "desktopModuleId";
                            return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        //LogException(ex);
                    }

                    int UserId = UserInfo.UserID;

                    foreach (var objModuleDefinition in DotNetNuke.Entities.Modules.Definitions.ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(module.Module).Values)
                    {
                        var objModule = new DotNetNuke.Entities.Modules.ModuleInfo();
                        objModule.Initialize(PortalSettings.PortalId);

                        objModule.PortalID = PortalSettings.PortalId;
                        objModule.TabID = PortalSettings.ActiveTab.TabID;

                        objModule.ModuleOrder = iPosition;
                        if (string.IsNullOrEmpty(module.Title) == true)
                        {
                            objModule.ModuleTitle = objModuleDefinition.FriendlyName;
                        }
                        else {
                            objModule.ModuleTitle = module.Title;
                        }

                        if (!string.IsNullOrEmpty(module.Container) && module.Container != "-1")
                        {
                            objModule.ContainerSrc = module.Container;
                        }

                        objModule.PaneName = module.Location;
                        objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                        if (objModuleDefinition.DefaultCacheTime > 0)
                        {
                            objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
                            if (PortalSettings.DefaultModuleId > Null.NullInteger && PortalSettings.DefaultTabId > Null.NullInteger)
                            {
                                var defaultModule = objModules.GetModule(PortalSettings.DefaultModuleId, PortalSettings.DefaultTabId, true);
                                if ((defaultModule != null))
                                {
                                    objModule.CacheTime = defaultModule.CacheTime;
                                }
                            }
                        }

                        switch (module.Visibility)
                        {
                            case 0:
                                objModule.InheritViewPermissions = true;
                                break;
                            case 1:
                                objModule.InheritViewPermissions = false;
                                break;
                            case 2:
                                objModule.InheritViewPermissions = false;
                                break;
                            case 3:
                                objModule.InheritViewPermissions = false;
                                break;
                            case 4:
                                objModule.InheritViewPermissions = false;
                                break;
                        }

                        // get the default module view permissions
                        var arrSystemModuleViewPermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");

                        // get the permissions from the page
                        foreach (DotNetNuke.Security.Permissions.TabPermissionInfo objTabPermission in objTabPermissions)
                        {
                            if (objTabPermission.PermissionKey == "VIEW" && module.Visibility == 0)
                            {
                                //Don't need to explicitly add View permisisons if "Same As Page"
                                continue;
                            }

                            // get the system module permissions for the permissionkey
                            var arrSystemModulePermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey);
                            // loop through the system module permissions
                            for (j = 0; j <= arrSystemModulePermissions.Count - 1; j++)
                            {
                                // create the module permission
                                DotNetNuke.Security.Permissions.PermissionInfo objSystemModulePermission = null;
                                objSystemModulePermission = (DotNetNuke.Security.Permissions.PermissionInfo)arrSystemModulePermissions[j];
                                if (objSystemModulePermission.PermissionKey == "VIEW" && module.Visibility == 1 && objTabPermission.PermissionKey != "EDIT")
                                {
                                    //Only Page Editors get View permissions if "Page Editors Only"
                                    continue;
                                }

                                var objModulePermission = Common.AddModulePermission(objModule, objSystemModulePermission, objTabPermission.RoleID, objTabPermission.UserID, objTabPermission.AllowAccess);

                                // ensure that every EDIT permission which allows access also provides VIEW permission
                                if (objModulePermission.PermissionKey == "EDIT" & objModulePermission.AllowAccess)
                                {
                                    var objModuleViewperm = Common.AddModulePermission(objModule, (DotNetNuke.Security.Permissions.PermissionInfo)arrSystemModuleViewPermissions[0], objModulePermission.RoleID, objModulePermission.UserID, true);
                                }
                            }

                            //Get the custom Module Permissions,  Assume that roles with Edit Tab Permissions
                            //are automatically assigned to the Custom Module Permissions
                            if (objTabPermission.PermissionKey == "EDIT")
                            {
                                var arrCustomModulePermissions = objPermissionController.GetPermissionsByModuleDefID(objModule.ModuleDefID);

                                // loop through the custom module permissions
                                for (j = 0; j <= arrCustomModulePermissions.Count - 1; j++)
                                {
                                    // create the module permission
                                    DotNetNuke.Security.Permissions.PermissionInfo objCustomModulePermission = null;
                                    objCustomModulePermission = (DotNetNuke.Security.Permissions.PermissionInfo)arrCustomModulePermissions[j];

                                    Common.AddModulePermission(objModule, objCustomModulePermission, objTabPermission.RoleID, objTabPermission.UserID, objTabPermission.AllowAccess);
                                }
                            }
                        }

                        objModule.AllTabs = false;
                        //objModule.Alignment = align;

                        apiResponse.CustomObject = objModules.AddModule(objModule);
                        apiResponse.Success = true;
                    }
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
        public HttpResponseMessage SaveUser(DTO.UserDetails user)
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            try
            {
                var userController = new DotNetNuke.Entities.Users.UserController();

                if (user.Id == -1)
                {
                    if (!DotNetNuke.Entities.Users.UserController.ValidatePassword(user.Password))
                    {
                        apiResponse.Success = false;
                        apiResponse.Message = "Invalid Password";

                        return Request.CreateResponse(HttpStatusCode.OK, apiResponse);
                    }

                    //new user
                    var dnnUser = new DotNetNuke.Entities.Users.UserInfo();
                    dnnUser.Username = user.UserName;
                    dnnUser.FirstName = user.FirstName;
                    dnnUser.LastName = user.LastName;
                    dnnUser.DisplayName = user.DisplayName;
                    dnnUser.Email = user.EmailAddress;
                    
                    dnnUser.PortalID = PortalSettings.PortalId;

                    dnnUser.Membership.Password = user.Password;
                    dnnUser.Membership.Approved = true;

                    DotNetNuke.Entities.Users.UserController.CreateUser(ref dnnUser);
                    apiResponse.Success = true;
                }
                else
                {
                    //existing user
                    var dnnUser = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, user.Id);
                    if (dnnUser != null)
                    {
                        //dnnUser.Username = user.UserName;
                        dnnUser.FirstName = user.FirstName;
                        dnnUser.LastName = user.LastName;
                        dnnUser.DisplayName = user.DisplayName;
                        dnnUser.Email = user.EmailAddress;
                        //dnnUser.Membership.Password = user.Password;

                        DotNetNuke.Entities.Users.UserController.UpdateUser(PortalSettings.PortalId, dnnUser);
                        apiResponse.Success = true;
                    }
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
        public HttpResponseMessage ListModulesOnPage(int portalId, int tabId)
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>>();

            try
            {
                var mc = new DotNetNuke.Entities.Modules.ModuleController();
                apiResponse.CustomObject = new List<DTO.GenericListItem>();

                foreach (var tabModule in mc.GetTabModules(tabId))
                {
                    var newGenericItem = new DTO.GenericListItem();
                    newGenericItem.Name = string.Format("{0} - {1} - {2}", string.IsNullOrWhiteSpace(tabModule.Value.ModuleTitle) ? "No Title" : tabModule.Value.ModuleTitle, tabModule.Value.PaneName, tabModule.Value.ModuleDefinition.FriendlyName);
                    newGenericItem.Value = tabModule.Value.ModuleID.ToString();
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
        public HttpResponseMessage LoadUserDetails(int Id)
        {
            var apiResponse = new DTO.ApiResponse<DTO.UserDetails>();

            try
            {
                var userInfo = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, Id);
                if (userInfo != null)
                {
                    apiResponse.CustomObject = new DTO.UserDetails();
                    apiResponse.CustomObject.Id = userInfo.UserID;
                    apiResponse.CustomObject.UserName = userInfo.Username;
                    apiResponse.CustomObject.DisplayName = userInfo.DisplayName;
                    apiResponse.CustomObject.FirstName = userInfo.FirstName;
                    apiResponse.CustomObject.LastName = userInfo.LastName;
                    apiResponse.CustomObject.EmailAddress = userInfo.Email;
                    apiResponse.CustomObject.LastLogin = userInfo.LastModifiedOnDate;
                    apiResponse.CustomObject.Authorized = userInfo.Membership.Approved;
                    apiResponse.CustomObject.Locked = userInfo.Membership.LockedOut;

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

                    foreach (var page in listOfPages.Where(i => i.IsDeleted == false).OrderBy(i => i.TabOrder))
                    {
                        var newItem = new DTO.GenericListImageItem() { Value = page.TabID.ToString(), Name = page.TabName };

                        if (string.IsNullOrWhiteSpace(page.IconFileLarge) == false)
                        {
                            newItem.Image = VirtualPathUtility.ToAbsolute(page.IconFileLarge);
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
        public HttpResponseMessage LoadCacheDetails(int PageId)
        {
            var apiResponse = new DTO.CacheResponse();

            try
            {
                apiResponse.PageOuputCacheVariations = 0;
                apiResponse.SiteOuputCacheVariations = 0;
                
                foreach (var item in DotNetNuke.Services.OutputCache.OutputCachingProvider.GetProviderList())
                {
                    apiResponse.PageOuputCacheVariations += DotNetNuke.Services.OutputCache.OutputCachingProvider.Instance(item.Key).GetItemCount(PageId);
                }

                apiResponse.TotalCacheItems = HttpContext.Current.Cache.Count;

                if (HttpContext.Current.Cache.EffectivePrivateBytesLimit > 1024 * 1024 * 1024)
                {
                    apiResponse.TotalCacheSizeLimit = (HttpContext.Current.Cache.EffectivePrivateBytesLimit / (1024.0 * 1024.0 * 1024.0)).ToString("N2") + " GB";
                }
                else if (HttpContext.Current.Cache.EffectivePrivateBytesLimit > 1024 * 1024)
                {
                    apiResponse.TotalCacheSizeLimit = (HttpContext.Current.Cache.EffectivePrivateBytesLimit / (1024.0 * 1024.0)).ToString("N2") + " MB";
                }
                else
                {
                    apiResponse.TotalCacheSizeLimit = (HttpContext.Current.Cache.EffectivePrivateBytesLimit / 1024.0).ToString("N2") + " KB";
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
        public HttpResponseMessage ClearOutputCache(int PageId)
        {
            var apiResponse = new DTO.ApiResponse<bool>();

            try
            { 
                foreach (var item in DotNetNuke.Services.OutputCache.OutputCachingProvider.GetProviderList())
                {
                    if (PageId == -1)
                    {
                        DotNetNuke.Services.OutputCache.OutputCachingProvider.Instance(item.Key).PurgeCache(PortalSettings.PortalId);
                        DataCache.ClearPortalCache(PortalSettings.PortalId, true);
                        DataCache.ClearCache();
                        DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.ClearCache();

                    }
                    else if (PageId == -2)
                    {
                        foreach (DotNetNuke.Entities.Portals.PortalInfo portal in new DotNetNuke.Entities.Portals.PortalController().GetPortals())
                        {
                            DotNetNuke.Services.OutputCache.OutputCachingProvider.Instance(item.Key).PurgeCache(portal.PortalID);
                            DataCache.ClearPortalCache(portal.PortalID, true);
                        }

                        DataCache.ClearCache();
                        DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.ClearCache();
                    }
                    else if (PageId > 0)
                    {
                        DataCache.ClearCache();
                        DotNetNuke.Services.OutputCache.OutputCachingProvider.Instance(item.Key).Remove(PageId);
                    }
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
        public HttpResponseMessage InstallExtensionUrl(int PageId)
        {
            var apiResponse = new DTO.ApiResponse<string>();

            try
            {
                if (DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                {
                    var objModules = new DotNetNuke.Entities.Modules.ModuleController();
                    var objModule = objModules.GetModuleByDefinition(-1, "Extensions");
                    if (objModule != null) { 
                        apiResponse.Message = DotNetNuke.Common.Globals.NavigateURL(objModule.TabID, true, PortalSettings, "Install", new string[] { "rtab=" + PageId.ToString() });
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
        public HttpResponseMessage RecycleApplication()
        {
            var apiResponse = new DTO.ApiResponse<bool>();

            try
            {
                if (DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { BypassBuffering = true, LogTypeKey = DotNetNuke.Services.Log.EventLog.EventLogController.EventLogType.HOST_ALERT.ToString() };
                    log.AddProperty("Message", "Application Recycle triggered from the Control Panel");
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                    Config.Touch();

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

                apiResponse.Containers = Common.ListContainers("host", "containers");

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
    }
}
