using Microsoft.AspNetCore.WebUtilities;

namespace Web.UI.Endpoints;

public static class MapEndpoints
{
    public static void MapProtomapsTileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/maps/protomaps/tiles/v4/{z:int}/{x:int}/{y:int}.mvt", async (
            int z,
            int x,
            int y,
            HttpContext httpContext,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            CancellationToken cancellationToken) =>
        {
            var protomapsApiKey = configuration["Protomaps:ApiKey"] ?? configuration["PROTOMAPS_API_KEY"];
            if (string.IsNullOrWhiteSpace(protomapsApiKey))
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync("Protomaps API key is not configured.", cancellationToken);
                return;
            }

            var upstreamQuery = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var queryItem in httpContext.Request.Query)
            {
                if (string.Equals(queryItem.Key, "key", StringComparison.OrdinalIgnoreCase))
                    continue;

                upstreamQuery[queryItem.Key] = queryItem.Value.ToString();
            }

            upstreamQuery["key"] = protomapsApiKey;

            var upstreamUrl = QueryHelpers.AddQueryString(
                $"https://api.protomaps.com/tiles/v4/{z}/{x}/{y}.mvt",
                upstreamQuery);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, upstreamUrl);
            if (httpContext.Request.Headers.TryGetValue("Accept", out var acceptHeader) && !string.IsNullOrWhiteSpace(acceptHeader))
            {
                requestMessage.Headers.TryAddWithoutValidation("Accept", acceptHeader.ToString());
            }

            var client = httpClientFactory.CreateClient();
            using var upstreamResponse = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            httpContext.Response.StatusCode = (int)upstreamResponse.StatusCode;

            foreach (var header in upstreamResponse.Headers)
            {
                httpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in upstreamResponse.Content.Headers)
            {
                httpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // 🚀 Performance: Cache vector map tiles on the client/CDN for 24 hours (immutable)
            if (upstreamResponse.IsSuccessStatusCode)
            {
                httpContext.Response.Headers.CacheControl = "public, max-age=86400, immutable";
            }

            await upstreamResponse.Content.CopyToAsync(httpContext.Response.Body, cancellationToken);
        });
    }
}
