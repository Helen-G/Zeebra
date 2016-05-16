using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using AFT.RegoV2.Webservices.Adapters.SlotServer.AppHosts;
using Funq;
using ServiceStack;


namespace AFT.RegoV2.Webservices.Adapters.SlotServer
{
    public class Global : HttpApplication
    {

        
        protected void Application_Start(object sender, EventArgs e)
        {
            //Initialize your application
            (new SlotServerAppHost()).Init();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}