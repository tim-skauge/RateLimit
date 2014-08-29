using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tims.Samples.RateLimiting.Web
{
    public class RateLimitHandler : DelegatingHandler
    {
        private readonly TimeSpan throttleTimeWindow = TimeSpan.FromMinutes(1);
        private readonly int throttleMaximumRequests = 10;
        private readonly IDatabase database;

        public RateLimitHandler()
        {
            database = ConnectionMultiplexer.Connect("localhost").GetDatabase();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clientIp = request.Headers.From ?? "127.0.0.1";
            var key = string.Format("rate-limit:{0}", clientIp);

            var numberOfRequests = await database.StringIncrementAsync(key);
            if (numberOfRequests == 1)
                database.KeyExpire(key, throttleTimeWindow);

            var response = numberOfRequests > throttleMaximumRequests
                ? request.CreateErrorResponse((HttpStatusCode) 429, "Rate limit exceeded")
                : await base.SendAsync(request, cancellationToken);

            var timeToReset = await database.KeyTimeToLiveAsync(key) ?? throttleTimeWindow;

            response.Headers.Add("X-Rate-Limit-Limit", throttleMaximumRequests.ToString());
            response.Headers.Add("X-Rate-Limit-Remaining", Math.Max(throttleMaximumRequests - numberOfRequests, 0).ToString());
            response.Headers.Add("X-Rate-Limit-Reset", ((int)timeToReset.TotalSeconds).ToString());

            return response;
        }
    }
}
