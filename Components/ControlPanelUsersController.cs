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
    public class ControlPanelUsersController : DnnApiController
    {
        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage Impersonate(int Id)
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            var impersonationToken = string.Empty;
            try
            {
                var objPortalSecurity = new DotNetNuke.Security.PortalSecurity();

                impersonationToken = objPortalSecurity.Encrypt(DotNetNuke.Entities.Host.Host.GUID.ToString(), UserInfo.UserID + ":" + Id);

                if (Id == 0)
                { 
                    objPortalSecurity.SignOut();
                }
                else {
                    var targetUserInfo = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, Id);

                    if (targetUserInfo != null)
                    {
                        DataCache.ClearUserCache(PortalSettings.PortalId, PortalSettings.UserInfo.Username);

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

            var actualResponse = Request.CreateResponse<DTO.ApiResponse<bool>>(HttpStatusCode.OK, apiResponse);

            var cookie = new HttpCookie(Common.impersonationCookieKey, impersonationToken);
            cookie.Expires = DateTime.Now.AddMinutes(Config.GetAuthCookieTimeout());
            cookie.Domain = Request.RequestUri.Host;
            cookie.Path = "/";

            HttpContext.Current.Response.SetCookie(cookie);

            return actualResponse;
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage RevertImpersonation()
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            CookieHeaderValue cookie = null;
            var impersonationToken = string.Empty;
            try
            {
                var objPortalSecurity = new DotNetNuke.Security.PortalSecurity();

                var impersonationCookie = Common.GetUserImpersonationCookie().Split(':');

                var Id = 0;
                var currentId = 0;

                int.TryParse(impersonationCookie.First(), out Id);
                int.TryParse(impersonationCookie.Last(), out currentId);

                if (currentId == 0 && UserInfo.UserID == -1)
                {
                    currentId = -1;
                }

                if (Id == 0 || UserInfo.UserID != currentId)
                {
                    objPortalSecurity.SignOut();
                }
                else
                {
                    var targetUserInfo = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, Id);

                    if (targetUserInfo != null)
                    {
                        DataCache.ClearUserCache(PortalSettings.PortalId, PortalSettings.UserInfo.Username);

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

            var actualResponse = Request.CreateResponse<DTO.ApiResponse<bool>>(HttpStatusCode.OK, apiResponse);


            var newcookie = new HttpCookie(Common.impersonationCookieKey, impersonationToken);
            newcookie.Expires = DateTime.Now.AddMinutes(-60);
            newcookie.Domain = Request.RequestUri.Host;
            newcookie.Path = "/";

            HttpContext.Current.Response.SetCookie(newcookie);

            return actualResponse;
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
        public HttpResponseMessage ViewUser(DTO.UserDetails user)
        {
            var apiResponse = new DTO.ApiResponse<string>();
            try
            {
                var userController = new DotNetNuke.Entities.Users.UserController();

                var dnnUser = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, user.Id);
                if (dnnUser != null)
                {
                    apiResponse.CustomObject = DotNetNuke.Common.Globals.UserProfileURL(dnnUser.UserID);
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

        [HttpPost]
        [DnnPageEditor]
        public HttpResponseMessage ManageUser(DTO.UserDetails user)
        {
            var apiResponse = new DTO.ApiResponse<string>();
            try
            {
                var userController = new DotNetNuke.Entities.Users.UserController();

                var dnnUser = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, user.Id);
                if (dnnUser != null)
                {
                    var objModules = new DotNetNuke.Entities.Modules.ModuleController();
                    var objModule = objModules.GetModuleByDefinition(PortalSettings.PortalId, "User Accounts");
                    if (objModule != null)
                    {
                        apiResponse.CustomObject = DotNetNuke.Common.Globals.NavigateURL(objModule.TabID, false, PortalSettings, "Edit", new string[] { "UserId=" + dnnUser.UserID.ToString(), "mid=" + objModule.ModuleID.ToString() });
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
        public HttpResponseMessage UnlockUser(DTO.UserDetails user)
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            try
            {
                var userController = new DotNetNuke.Entities.Users.UserController();

                var dnnUser = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, user.Id);
                if (dnnUser != null)
                {
                    DotNetNuke.Entities.Users.UserController.UnLockUser(dnnUser);
                    DotNetNuke.Common.Utilities.DataCache.RemoveCache("MembershipUser_" + dnnUser.Username);
                    DotNetNuke.Common.Utilities.DataCache.ClearUserCache(PortalSettings.PortalId, dnnUser.Username);

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

        [HttpPost]
        [DnnPageEditor]
        public HttpResponseMessage SendPasswordReset(DTO.UserDetails user)
        {
            var apiResponse = new DTO.ApiResponse<bool>();
            try
            {
                var userController = new DotNetNuke.Entities.Users.UserController();

                var dnnUser = DotNetNuke.Entities.Users.UserController.GetUserById(PortalSettings.PortalId, user.Id);
                if (dnnUser != null)
                {
                    DotNetNuke.Entities.Users.UserController.ResetPasswordToken(dnnUser, DotNetNuke.Entities.Host.Host.AdminMembershipResetLinkValidity);

                    bool canSend = DotNetNuke.Services.Mail.Mail.SendMail(dnnUser, DotNetNuke.Services.Mail.MessageType.PasswordReminder, PortalSettings) == string.Empty;
                    var message = String.Empty;

                    if (canSend)
                    {
                        apiResponse.Success = true;

                       // message = Localization.GetString("PasswordSent", LocalResourceFile);
                       // LogSuccess();
                    }
                    else
                    {
                        apiResponse.Success = false;
                        //message = Localization.GetString("OptionUnavailable", LocalResourceFile);
                        //moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                        //LogFailure(message);
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
