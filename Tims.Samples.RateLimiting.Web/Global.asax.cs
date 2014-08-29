using System.Web;
using System.Web.Http;

namespace Tims.Samples.RateLimiting.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(configuration =>
            {
                configuration.MessageHandlers.Add(new RateLimitHandler());
                configuration.MapHttpAttributeRoutes();
            });
        }
    }
}
