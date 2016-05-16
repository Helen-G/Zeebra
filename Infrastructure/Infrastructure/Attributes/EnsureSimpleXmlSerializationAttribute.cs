using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;

namespace AFT.RegoV2.Infrastructure.Attributes
{
    public class EnsureSimpleXmlSerializationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor descriptor)
        {
            // Clear the formatters list.
            var formatter = settings.Formatters.FirstOrDefault(f => f is XmlMediaTypeFormatter) as XmlMediaTypeFormatter;

            if (formatter == null)
            {
                formatter = new XmlMediaTypeFormatter { UseXmlSerializer = true };
                settings.Formatters.Add(formatter);
            }
            else
            {
                formatter.UseXmlSerializer = true;
            }
        }
    }
}
