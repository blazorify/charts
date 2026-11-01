using System;
using Blazorify.Charts.Options;

namespace Microsoft.Extensions.DependencyInjection {
	public static class AddBlazorifyChartsExtension {
		public static IServiceCollection AddBlazorifyCharts(this IServiceCollection services) {
			return services.AddBlazorifyCharts(_ => { });
		}

		public static IServiceCollection AddBlazorifyCharts(this IServiceCollection services, Action<BlazorifyChartsOptions> configure) {
			services.Configure(configure);

			return services;
		}
	}
}
