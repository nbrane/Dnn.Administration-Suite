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
        private LocalizationProvider localizationProvider = new LocalizationProvider();

        [HttpGet]
        public HttpResponseMessage LoadJS(string Name)
        {
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
                case "main":
                    controlName = "Main";
                    break;
                default:

                    break;
            }

            if (!string.IsNullOrWhiteSpace(controlName))
            {
                var fileName = DotNetNuke.Common.Globals.ApplicationMapPath + "\\desktopmodules\\nbrane\\administrationsuite\\controls\\js\\" + controlName + ".js";
                if (System.IO.File.Exists(fileName))
                {
                    var fileContents = System.IO.File.ReadAllText(fileName);
                    if (fileContents.Contains("\"[data-bind: main-resource-file]\""))
                    {
                        fileContents = fileContents.Replace("\"[data-bind: main-resource-file]\"", Newtonsoft.Json.JsonConvert.SerializeObject(localizationProvider.GetCompiledResourceFile(PortalSettings, "/DesktopModules/nBrane/AdministrationSuite/Controls/App_LocalResources/" + controlName + ".resx", System.Threading.Thread.CurrentThread.CurrentCulture.Name)));
                    }
                    
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(fileContents, System.Text.Encoding.UTF8, "text/plain");

                    return response;
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, false);
        }

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


            if (fileContents.Contains("\"[data-bind: sslenabled]\""))
            {
                fileContents = fileContents.Replace("\"[data-bind: sslenabled]\"", PortalSettings.SSLEnabled.ToString().ToLower());
            }

            apiResponse.JS = fileContents;

            var result = Newtonsoft.Json.JsonConvert.SerializeObject(localizationProvider.GetCompiledResourceFile(PortalSettings, "/DesktopModules/nBrane/AdministrationSuite/Controls/App_LocalResources/" + controlName + ".resx", System.Threading.Thread.CurrentThread.CurrentCulture.Name));
            apiResponse.LANG = result;

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
    }
}
