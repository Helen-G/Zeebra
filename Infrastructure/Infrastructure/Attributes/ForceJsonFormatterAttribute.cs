using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace AFT.RegoV2.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ForceJsonFormatterAttribute : Attribute, IControllerConfiguration
    {
        private readonly string[] _additionalMediaTypes;

        public ForceJsonFormatterAttribute(params string[] additionalMediaTypes)
        {
            _additionalMediaTypes = additionalMediaTypes ?? new string[]{};
        }

        public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor descriptor)
        {
            // Clear the formatters list.
            settings.Formatters.Clear();
            
            var formatter = new JsonMediaTypeFormatter();
            foreach (var mediaType in _additionalMediaTypes)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }
            settings.Formatters.Add(formatter);
        }
    }
}