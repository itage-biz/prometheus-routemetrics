using System;

namespace Itage.Prometheus.RouteMetrics
{
    /// <summary>
    /// Prometheus metrics middleware options
    /// </summary>
    public class PrometheusMetricsOptions
    {
        /// <summary>
        /// Prefixes to ignore (should start with / )
        /// </summary>
        public string[] IgnorePrefixes { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Exact paths to ignore (should start with / )
        /// </summary>
        public string[] IgnoreExact { get; set; } = Array.Empty<string>();
    }
}