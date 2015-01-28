using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using ServiceWatcherWebRole.Models;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData;
    

namespace ServiceWatcherWebRole
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();
            // OData routes / models
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Heartbeat>("Heartbeats");
            config.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());

            //allow odata query options
            config.AddODataQueryFilter(new EnableQueryAttribute());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
