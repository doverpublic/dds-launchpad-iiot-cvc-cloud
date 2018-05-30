using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Iot.Common
{
    public class HTTPHelper
    {
        public static string USER_PROFILE = "UserProfile";

        public static bool DeleteCookieValueFor(HttpContext context, string cookieName)
        {
            bool bRet = false;

            if (context != null)
            {
                IResponseCookies cookies = context.Response.Cookies;
                cookies.Delete(cookieName);
                bRet = true;
            }
            else
            {
                Console.WriteLine("On DeleteCookieValueFor - context object is null ");
            }

            return bRet;
        }

        public static bool EndSession(HttpContext context, Controller controller, bool registerUser = true )
        {
            bool bRet = false; 

            string sessionId = HTTPHelper.GetCookieValueFor(context, SessionManager.GetSessionCookieName());

            if(sessionId.Length > 0 )
            {
                HTTPHelper.DeleteCookieValueFor(context, SessionManager.GetSessionCookieName());

                SessionManager.DeleteSession(sessionId);

                controller.ViewBag.UserName = "";
                controller.ViewBag.Name = "";
                controller.ViewBag.Persona = "";
                controller.ViewBag.RegisterUser = registerUser;

                bRet = true;
            }

            return bRet;
        }

        public static string GetCookieValueFor( HttpContext context, string cookieName, bool setCookieToResponse = true )
        {
            string strRet = "";

            if( context != null )
            {
                IRequestCookieCollection cookies = context.Request.Cookies;
                string cookieValue = cookies[cookieName];

                if (cookieValue != null)
                {
                    strRet = System.Web.HttpUtility.UrlDecode(cookieValue);

                    if (setCookieToResponse)
                        if( !SetCookieValueFor(context, cookieName, strRet) )
                            Console.WriteLine("On GetCookieValueFor - could not set the cookie value to response");
                }
            }
            else
            {
                Console.WriteLine("On GetCookieValueFor - context object is null ");
            }

            return strRet;
        }

        public static string GetQueryParameterValueFor(HttpContext context, string parameterName )
        {
            string strRet = "";

            if (context != null)
            {
                IQueryCollection parameters = context.Request.Query;
                string parameterValue = parameters[parameterName];

                if (parameterValue != null && parameterValue.Length > 0)
                {
                    strRet = System.Web.HttpUtility.UrlDecode(parameterValue);
                }
            }
            else
            {
                Console.WriteLine("On GetQueryParameterValueFor - context object is null ");
            }

            return strRet;
        }

        public static bool IsSessionExpired(HttpContext context, Controller controller, bool registerUser = true)
        {
            bool bRet = true;

            string sessionId = HTTPHelper.GetCookieValueFor(context, SessionManager.GetSessionCookieName());

            if (sessionId.Length > 0)
            {
                bRet = SessionManager.IsSessionExpired(sessionId);

                if( bRet)
                {
                    HTTPHelper.DeleteCookieValueFor(context, SessionManager.GetSessionCookieName());

                    controller.ViewBag.UserName = "";
                    controller.ViewBag.Name = "";
                    controller.ViewBag.Persona = "";
                    controller.ViewBag.RegisterUser = registerUser;
                }
                else
                {
                    HTTPHelper.SetCookieValueFor(context, SessionManager.GetSessionCookieName(), sessionId);
                    UserProfile userProfile = (UserProfile)SessionManager.GetValue(sessionId, USER_PROFILE);

                    // need to be extra cautions - the session could have just expired from the few lines above
                    if( userProfile != null )
                    {
                        controller.ViewBag.UserName = userProfile.UserName;
                        if (userProfile.FirstName != null && userProfile.FirstName.Length > 0)
                            controller.ViewBag.Name = userProfile.FirstName;
                        else
                            controller.ViewBag.Name = userProfile.UserName;
                        controller.ViewBag.Persona = "User";
                        controller.ViewBag.RegisterUser = false;
                    }
                    else
                    {
                        controller.ViewBag.UserName = "";
                        controller.ViewBag.Name = "";
                        controller.ViewBag.Persona = "";
                        controller.ViewBag.RegisterUser = registerUser;
                    }
                }
            }
            else
            {
                controller.ViewBag.UserName = "";
                controller.ViewBag.Name = "";
                controller.ViewBag.Persona = "";
                controller.ViewBag.RegisterUser = registerUser;
            }

            return bRet;
        }

        public static string ReplaceUrlLastItem( string url, string newLocation )
        {
            string strRet = url;

            if( newLocation.Length > 0 )
            {
                string[] parts = url.Split('/');

                for( int index =0; index < parts.Length; index++ )
                {
                    if ((index + 1) == parts.Length)
                        strRet += "/" + newLocation;
                    else
                    {
                        if( index == 0 )
                            strRet += parts[index];
                        else
                            strRet += "/" + parts[index];
                    }
                }
            }
            Console.WriteLine("On ReplaceUrlLastItem url=[" + url + "] - new url=[" + strRet + "]");

            return strRet;
        }

        public static bool SetCookieValueFor(HttpContext context, string cookieName, string cookieValue)
        {
            bool bRet = false;

            if (context != null)
            {
                IResponseCookies cookies = context.Response.Cookies;
                cookies.Append(cookieName,cookieValue);
                bRet = true;
            }
            else
            {
                Console.WriteLine("On SetCookieValueFor - context object is null ");
            }

            return bRet;
        }

        public static string StartSession(HttpContext context, Controller controller, UserProfile userProfile, string defaultUserPersona, string defaultUserHomePage, string applicationHomePage )
        {
            string strRet = defaultUserHomePage;
            string sessionId = SessionManager.CreateNewSession();
            HTTPHelper.SetCookieValueFor(context, SessionManager.GetSessionCookieName(), sessionId);

            userProfile.ApplicationHomePage = applicationHomePage;
            userProfile.DefaultUserHomePage = defaultUserHomePage;
            userProfile.CurrentPersonaHomePage = defaultUserHomePage;
            userProfile.CurrentPersona = defaultUserPersona;

            SessionManager.SetValue(sessionId, USER_PROFILE, userProfile);
            controller.ViewBag.UserName = userProfile.UserName;
            if (userProfile.FirstName != null && userProfile.FirstName.Length > 0)
                controller.ViewBag.Name = userProfile.FirstName;
            else
                controller.ViewBag.Name = userProfile.UserName;
            controller.ViewBag.Persona = userProfile.CurrentPersona;
            controller.ViewBag.RegisterUser = false;

            strRet = userProfile.ApplicationHomePage + userProfile.CurrentPersonaHomePage;

            return strRet;
        }
    }
}
