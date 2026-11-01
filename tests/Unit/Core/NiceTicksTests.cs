using Blazorify.Charts.Core.Scales;
using Xunit;

namespace Blazorify.Charts.Tests.Unit.Core;

public class NiceTicksTests {
	[Theory]
	[InlineData(0, 100, 5, 20)]
	[InlineData(0, 1, 5, 0.2)]
	[InlineData(0, 0.3, 6, 0.05)]
	[InlineData(0, 7, 5, 2)]
	[InlineData(0, 1000000, 4, 500000)]
	public void Step_PicksOneTwoOrFiveTimesPowerOfTen(double min, double max, int count, double expected) {
		Assert.Equal(expected, NiceTicks.Step(min, max, count), 12);
	}

	[Fact]
	public void Values_ForTenths_HaveNoFloatingPointDrift() {
		var values = NiceTicks.Values(0, 1, 10);

		Assert.Equal([0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1], values);
	}

	[Fact]
	public void Values_ForSmallSteps_AreExactDecimals() {
		var values = NiceTicks.Values(0, 0.3, 6);

		Assert.Equal([0, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3], values);
		Assert.Contains(0.15, values);
	}

	[Fact]
	public void Values_AcrossZero_IncludeZero() {
		Assert.Equal([-20, -10, 0, 10, 20, 30], NiceTicks.Values(-25, 35, 6));
	}

	[Fact]
	public void Extend_RoundsOutwardsToStep() {
		Assert.Equal((0, 100), NiceTicks.Extend(3, 97, 5));
		Assert.Equal((-1, 1), NiceTicks.Extend(-0.93, 0.51, 5));
	}

	[Fact]
	public void Values_WhenRangeIsEmpty_ReturnsSingleValue() {
		Assert.Equal([5], NiceTicks.Values(5, 5, 5));
	}

	[Fact]
	public void Values_StayWithinRange() {
		var random = new Random(42);

		for (var i = 0; i < 1000; i++) {
			var min = (random.NextDouble() - 0.5) * Math.Pow(10, random.Next(-6, 9));
			var max = min + (random.NextDouble() * Math.Pow(10, random.Next(-6, 9)));
			var count = random.Next(2, 12);
			var values = NiceTicks.Values(min, max, count);
			var tolerance = NiceTicks.Step(min, max, count) * 1e-6;

			Assert.All(values, value => Assert.InRange(value, min - tolerance, max + tolerance));
			Assert.True(values.Count <= (count * 2) + 2, $"{values.Count} ticks for {count} requested over {min}..{max}");
		}
	}
}
