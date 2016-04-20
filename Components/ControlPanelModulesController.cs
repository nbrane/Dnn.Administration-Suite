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
    public class ControlPanelModulesController : DnnApiController
    {
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
        public HttpResponseMessage ListModuleCategories()
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>>();
            
            try
            {
                apiResponse.CustomObject = new List<DTO.GenericListItem>();

                var termController = DotNetNuke.Entities.Content.Common.Util.GetTermController();

                foreach (var term in termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Where(t => t.Name != "< None >"))
                {
                    apiResponse.CustomObject.Add(new DTO.GenericListItem() { Value = term.TermId.ToString(), Name = term.Name });
                }
                apiResponse.CustomObject.Add(new DTO.GenericListItem() { Value = "All", Name = "All" });

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
        public HttpResponseMessage ListModules(string category)
        {
            var apiResponse = new DTO.ApiModuleResponse();

            try
            {
                var listOfModules = DotNetNuke.Entities.Modules.DesktopModuleController.GetPortalDesktopModules(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId).Values;

                apiResponse.Modules = new List<DTO.GenericListItem>();

                foreach (var module in listOfModules)
                {
                    if (category.Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        apiResponse.Modules.Add(new DTO.GenericListItem() { Value = module.DesktopModuleID.ToString(), Name = module.FriendlyName });
                    }
                    else
                    {
                        if (module.DesktopModule.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                        {
                            apiResponse.Modules.Add(new DTO.GenericListItem() { Value = module.DesktopModuleID.ToString(), Name = module.FriendlyName });
                        }
                    }
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
        
    }
}
