using Blazorify.Charts.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Blazorify.Charts.Tests.Unit;

public class AddBlazorifyChartsExtensionTests {
	[Fact]
	public void AddBlazorifyCharts_WithoutConfigure_RegistersDefaultOptions() {
		var services = new ServiceCollection().AddBlazorifyCharts();

		var options = services.BuildServiceProvider().GetRequiredService<IOptions<BlazorifyChartsOptions>>().Value;

		Assert.True(options.EnableAnimations);
		Assert.Equal(TimeSpan.FromMilliseconds(400), options.AnimationDuration);
	}

	[Fact]
	public void AddBlazorifyCharts_WithConfigure_AppliesOptions() {
		var services = new ServiceCollection().AddBlazorifyCharts(options => {
			options.EnableAnimations = false;
			options.AnimationDuration = TimeSpan.FromSeconds(1);
		});

		var options = services.BuildServiceProvider().GetRequiredService<IOptions<BlazorifyChartsOptions>>().Value;

		Assert.False(options.EnableAnimations);
		Assert.Equal(TimeSpan.FromSeconds(1), options.AnimationDuration);
	}
}
