using System;
using System.Web.Configuration;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Common.Base
{
    [Category("WebService")]
    public abstract class WebServiceTestsBase : MultiprocessTestsBase
    {
        protected WebServiceTestsBase()
        {
            ServicesHostBaseUrl = new Uri(WebConfigurationManager.AppSettings["ServiceHostUrl"]);
        }

        protected Uri ServicesHostBaseUrl { get; set; }
    }
}