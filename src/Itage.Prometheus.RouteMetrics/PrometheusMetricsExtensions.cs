using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.AspNetCore;
using System;
using Itage.Prometheus.RouteMetrics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extensions for prometheus metrics middleware
    /// </summary>
    public static class PrometheusMetricsExtensions
    {
        /// <summary>
        /// Injects metrics middleware and options to collection
        /// </summary>
        /// <param name="services">Services collection</param>
        /// <param name="configureOptions">Configuration callback</param>
        /// <returns></returns>
        public static IServiceCollection AddPrometheusMetrics(
            this IServiceCollection services,
            Action<PrometheusMetricsOptions>? configureOptions = null)
        {
            services.AddTransient<PrometheusMetricsMiddleware>();
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            return services;
        }

        /// <summary>
        /// Uses prometheus metrics middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="setupOptions">Prometheus setup options callback</param>
        /// <returns></returns>
        public static IApplicationBuilder UsePrometheusMetrics(
            this IApplicationBuilder app,
            Action<PrometheusOptions>? setupOptions = null)
        {
            return app
                .UsePrometheusServer(options =>
                {
                    options.MapPath = "/metrics";
                    setupOptions?.Invoke(options);
                })
                .UseMiddleware<PrometheusMetricsMiddleware>();
        }
    }
}