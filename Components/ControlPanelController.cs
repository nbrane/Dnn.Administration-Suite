using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web.Http;

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
        public HttpResponseMessage LoadPageDetails(int id)
        {
            var apiResponse = new DTO.ApiResponse<DTO.PageDetails>();

            try
            {
                var tc = new DotNetNuke.Entities.Tabs.TabController();
                apiResponse.CustomObject = new DTO.PageDetails(tc.GetTab(id, PortalSettings.PortalId));
                apiResponse.CustomObject.LoadAllPages();

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
            System.Threading.Thread.Sleep(800);

            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>>();

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
                    apiResponse.CustomObject = new List<DTO.GenericListItem>();

                    foreach (var page in listOfPages)
                    {
                        apiResponse.CustomObject.Add(new DTO.GenericListItem() { Value = page.TabID.ToString(), Name = page.TabName });
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
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>>();

            try
            {
                var listOfModules = DotNetNuke.Entities.Modules.DesktopModuleController.GetPortalDesktopModules(PortalSettings.PortalId).Values;

                apiResponse.CustomObject = new List<DTO.GenericListItem>();

                foreach (var module in listOfModules)
                {
                    apiResponse.CustomObject.Add(new DTO.GenericListItem() { Value = module.DesktopModuleID.ToString(), Name = module.FriendlyName });
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

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage ListUsers(string filter)
        {
            var apiResponse = new DTO.ApiResponse<List<DTO.GenericListItem>>();

            try
            {
                var listOfUsers = Data.SearchForUsers(PortalSettings.PortalId, filter, 1, 15);

                apiResponse.CustomObject = new List<DTO.GenericListItem>();

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
