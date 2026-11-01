using Blazorify.Charts.Options;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Blazorify.Charts.Tests.Components;

public class BunitSetupTests : BunitContext {
	[Fact]
	public void AddBlazorifyCharts_InBunitContext_ResolvesOptions() {
		this.Services.AddBlazorifyCharts();

		var options = this.Services.GetRequiredService<IOptions<BlazorifyChartsOptions>>().Value;

		Assert.True(options.EnableAnimations);
	}
}
