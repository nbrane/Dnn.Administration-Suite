using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace nBrane.Modules.AdministrationSuite.Components
{
    static class Common
    {
        internal static void SetCookie(this HttpResponseHeaders headers, Cookie cookie)
        {
            var cookieBuilder = new StringBuilder(HttpUtility.UrlEncode(cookie.Name) + "=" + HttpUtility.UrlEncode(cookie.Value));
            if (cookie.HttpOnly)
            {
                cookieBuilder.Append("; HttpOnly");
            }

            if (cookie.Secure)
            {
                cookieBuilder.Append("; Secure");
            }

            headers.Add("Set-Cookie", cookieBuilder.ToString());
        }



        internal static DotNetNuke.Entities.Tabs.TabInfo GetParentTab(DotNetNuke.Entities.Tabs.TabInfo relativeToTab, DTO.PagePositionMode location)
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
                            strRoot = PortalSettings.Current.HomeDirectoryMapPath + DotNetNuke.UI.Skins.SkinController.RootSkin;
                            dbPrefix = "[L]" + DotNetNuke.UI.Skins.SkinController.RootSkin;
                        }
                        else {
                            strRoot = PortalSettings.Current.HomeDirectoryMapPath + DotNetNuke.UI.Skins.SkinController.RootContainer;
                            dbPrefix = "[L]" + DotNetNuke.UI.Skins.SkinController.RootContainer;
                        }
                        break;
                }
                var siteDefault  = string.Empty;
                if (SkinOrContainer.ToLower() == "skin")
                {
                    var currentDefault = PortalSettings.Current.ActiveTab.SkinSrc;
                    if (string.IsNullOrWhiteSpace(currentDefault))
                    {
                        currentDefault = PortalSettings.Current.DefaultPortalSkin;
                    }

                    siteDefault = GetFriendySkinName(PortalSettings.Current.DefaultPortalSkin);
                    currentSetting = GetFriendySkinName(currentDefault);
                }
                else {
                    var currentDefault = PortalSettings.Current.ActiveTab.ContainerSrc;
                    if (string.IsNullOrWhiteSpace(currentDefault))
                    {
                        currentDefault = PortalSettings.Current.DefaultPortalContainer;
                    }

                    siteDefault = GetFriendySkinName(PortalSettings.Current.DefaultPortalContainer);
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
                    apiResponse.Insert(0, new DTO.GenericSelectableListItem("Default - " + siteDefault, "-1", false));
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
                param = DotNetNuke.UI.Skins.SkinController.FormatSkinSrc(param, PortalSettings.Current);

                return System.IO.Path.GetDirectoryName(param).Split(System.IO.Path.DirectorySeparatorChar).Last() + " - " + System.IO.Path.GetFileNameWithoutExtension(param).Replace("_", " ");
            }

            return null;
        }

        internal static string FormatSkinName(string strSkinFolder, string strSkinFile)
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

        internal static DotNetNuke.Security.Permissions.ModulePermissionInfo AddModulePermission(DotNetNuke.Entities.Modules.ModuleInfo objModule, DotNetNuke.Security.Permissions.PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var objModulePermission = new DotNetNuke.Security.Permissions.ModulePermissionInfo();
            objModulePermission.ModuleID = objModule.ModuleID;
            objModulePermission.PermissionID = permission.PermissionID;
            objModulePermission.RoleID = roleId;
            objModulePermission.UserID = userId;
            objModulePermission.PermissionKey = permission.PermissionKey;
            objModulePermission.AllowAccess = allowAccess;

            // add the permission to the collection
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }

            return objModulePermission;
        }


        internal static bool CanUserImpersonateOtherUsers()
        {
            var objCurrentUser = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();

            if (objCurrentUser.IsSuperUser || objCurrentUser.IsInRole(PortalSettings.Current.AdministratorRoleName) || IsUserImpersonated())
            {
                return true;
            }

            return false;
        }

        internal static bool IsUserImpersonated()
        {

            if (HttpContext.Current.Request.Cookies[impersonationCookieKey] != null)
            {
                string cookieValue = GetUserImpersonationCookie();
                dynamic cookieArray = cookieValue.Split(':');

                int originalUserId = int.Parse(cookieArray.First());
                int targetUserId = int.Parse(cookieArray.Last());

                if (originalUserId == 0)
                    originalUserId = -1;
                if (targetUserId == 0)
                    targetUserId = -1;

                if (!HttpContext.Current.Request.IsAuthenticated && (PortalSettings.Current.UserId != originalUserId && PortalSettings.Current.UserId != targetUserId))
                {
                    //if the request isn't authenticated, we need to clear our cookie
                    ClearUserImpersonationCookie();
                    return false;
                }
                return true;
            }
            return false;
        }

        const string impersonationCookieKey = "nbrane-user-impersonation";

        internal static Cookie ClearUserImpersonationCookie()
        {
            //reset the cookie value
            var cookie = new Cookie(impersonationCookieKey, string.Empty);
            cookie.Expires = DateTime.Now.AddDays(-1);

            return cookie;
        }

        internal static string GetUserImpersonationCookie()
        {
            string functionReturnValue = null;
            functionReturnValue = Null.NullString;

            var objPortalSecurity = new DotNetNuke.Security.PortalSecurity();

            if (HttpContext.Current.Request.Cookies[impersonationCookieKey] != null)
            {
                string cookieValue = objPortalSecurity.Decrypt(DotNetNuke.Entities.Host.Host.GUID.ToString(), HttpContext.Current.Request.Cookies[impersonationCookieKey].Value);

                functionReturnValue = cookieValue;
            }
            return functionReturnValue;

        }

        internal static Cookie GenerateImpersonationCookie(int originalUserId, int targetUserId)
        {
            var objPortalSecurity = new DotNetNuke.Security.PortalSecurity();
            var cookie = new Cookie(impersonationCookieKey, objPortalSecurity.Encrypt(DotNetNuke.Entities.Host.Host.GUID.ToString(), originalUserId + ":" + targetUserId));
            cookie.Expires = DateTime.Now.AddMinutes(60);

            return cookie;
        }

        internal static int AddExistingModule(int moduleId, int tabId, string paneName, int position, string align, string container)
        {

            var objModules = new DotNetNuke.Entities.Modules.ModuleController();
            var objEventLog = new DotNetNuke.Services.Log.EventLog.EventLogController();

            int UserId = PortalSettings.Current.UserId;

            var objModule = objModules.GetModule(moduleId, tabId, false);
            if (objModule != null)
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                var objClone = objModule.Clone();
                objClone.TabID = PortalSettings.Current.ActiveTab.TabID;
                objClone.ModuleOrder = position;
                objClone.PaneName = paneName;
                objClone.Alignment = align;
                objClone.ContainerSrc = container;

                int iNewModuleId = objModules.AddModule(objClone);
                //objEventLog.AddLog(objClone, PortalSettings.Current, UserId, "", DotNetNuke.Services.Log.EventLog.EventLogController.EventLogType.MODULE_CREATED);

                return iNewModuleId;
            }

            return -1;
        }

        internal static void AddModuleCopy(int iModuleId, int iTabId, int iOrderPosition, string sPaneName, string container)
        {
            var objModules = new DotNetNuke.Entities.Modules.ModuleController();
            var objModule = objModules.GetModule(iModuleId, iTabId, false);
            if (objModule != null)
            {
                //Clone module as it exists in the cache and changes we make will update the cached object
                var newModule = objModule.Clone();

                newModule.ModuleID = Null.NullInteger;
                newModule.TabID = PortalSettings.Current.ActiveTab.TabID;
                newModule.ModuleTitle = "Copy of " + objModule.ModuleTitle;
                newModule.ModuleOrder = iOrderPosition;
                newModule.PaneName = sPaneName;
                newModule.ContainerSrc = container;
                newModule.ModuleID = objModules.AddModule(newModule);

                if (string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass) == false)
                {
                    object objObject = DotNetNuke.Framework.Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass);
                    if (objObject is DotNetNuke.Entities.Modules.IPortable)
                    {
                        string Content = Convert.ToString(((DotNetNuke.Entities.Modules.IPortable)objObject).ExportModule(iModuleId));
                        if (string.IsNullOrEmpty(Content) == false)
                        {
                            ((DotNetNuke.Entities.Modules.IPortable)objObject).ImportModule(newModule.ModuleID, Content, newModule.DesktopModule.Version, PortalSettings.Current.UserId);
                        }
                    }
                }
            }
        }
    }
}
