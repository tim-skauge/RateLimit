using System.Web.Http;
using System.Web.Http.Results;

namespace Tims.Samples.RateLimiting.Web
{
    public class RootController : ApiController
    {
        [Route("")]
        public OkResult Get()
        {
            return Ok();
        }
    }
}
