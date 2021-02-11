using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Prometheus.Client;

namespace Itage.Prometheus.RouteMetrics
{
    /// <summary>
    /// EGW Metrics path collection middleware
    /// </summary>
    public sealed class PrometheusMetricsMiddleware : IMiddleware
    {
        private static readonly string[] Slashes = {"/"};

        private readonly IMetricFamily<IHistogram> _histogram;
        private readonly IOptions<PrometheusMetricsOptions> _options;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Prometheus metrics options</param>
        public PrometheusMetricsMiddleware(IOptions<PrometheusMetricsOptions> options)
        {
            _options = options;
            _histogram = Metrics.DefaultFactory
                .CreateHistogram(
                    "http_request_duration_seconds",
                    "duration histogram of http responses labeled with: status_code, method, path",
                    "status_code", "method", "path");
        }

        private void Report(int statusCode, string method, string path, TimeSpan value)
        {
            _histogram.WithLabels(statusCode.ToString(), method, string.IsNullOrWhiteSpace(path) ? "/" : path)
                .Observe(value.TotalSeconds);
        }

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            await next(context);
            TimeSpan requestDuration = (DateTimeOffset.UtcNow - startTime);

            string routePath = context.Request.Path.HasValue
                ? context.Request.Path.Value
                : "/";

            foreach (var prefix in _options.Value.IgnoreExact)
            {
                if (routePath == prefix) return;
            }

            foreach (var prefix in _options.Value.IgnorePrefixes)
            {
                if (routePath.StartsWith(prefix)) return;
            }

            string routeTemplate = GetControllerRouteTemplate(context) ?? GetOcelotRouteTemplate(context);

            Report(context.Response.StatusCode, context.Request.Method, routeTemplate, requestDuration);
        }


        private static string? GetControllerRouteTemplate(HttpContext context)
        {
            var actionDescriptor = context.Features.Get<IEndpointFeature?>()?
                .Endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            return actionDescriptor?.AttributeRouteInfo?.Template;
        }

        private static string GetOcelotRouteTemplate(HttpContext context)
        {
            string[] data = context.Request.Path.HasValue
                ? context.Request.Path.Value!.Split(Slashes, 2, StringSplitOptions.RemoveEmptyEntries)
                : Slashes;
            return data.FirstOrDefault() ?? "/";
        }
    }
}