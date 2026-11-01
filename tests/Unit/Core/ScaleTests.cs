using Blazorify.Charts.Core.Scales;
using Xunit;

namespace Blazorify.Charts.Tests.Unit.Core;

public class LinearScaleTests {
	[Fact]
	public void Map_IsLinearOverNiceDomain() {
		var scale = new LinearScale(3, 97);

		Assert.Equal(0, scale.Min);
		Assert.Equal(100, scale.Max);
		Assert.Equal(0.25, scale.Map(25), 12);
		Assert.Equal(25, scale.Invert(0.25), 12);
	}

	[Fact]
	public void Constructor_WhenNotNice_KeepsDomain() {
		var scale = new LinearScale(3, 97, nice: false);

		Assert.Equal(3, scale.Min);
		Assert.Equal(97, scale.Max);
	}

	[Fact]
	public void Constructor_WhenReversed_Swaps() {
		var scale = new LinearScale(10, 0, nice: false);

		Assert.Equal(0, scale.Min);
		Assert.Equal(10, scale.Max);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(50)]
	[InlineData(-3)]
	public void Constructor_WhenFlat_CentresTheValue(double value) {
		var scale = new LinearScale(value, value, nice: false);

		Assert.Equal(0.5, scale.Map(value), 12);
	}

	[Fact]
	public void Ticks_CoverDomain() {
		Assert.Equal([0, 20, 40, 60, 80, 100], new LinearScale(3, 97).Ticks());
	}
}

public class LogScaleTests {
	[Fact]
	public void Map_IsLogarithmic() {
		var scale = new LogScale(1, 100);

		Assert.Equal(0.5, scale.Map(10), 12);
		Assert.True(double.IsNaN(scale.Map(0)));
	}

	[Fact]
	public void Constructor_WhenNice_ExtendsToPowersOfTen() {
		var scale = new LogScale(3, 450);

		Assert.Equal(1, scale.Min);
		Assert.Equal(1000, scale.Max);
		Assert.Equal([1, 10, 100, 1000], scale.Ticks());
	}

	[Fact]
	public void Ticks_BelowOne_AreExact() {
		Assert.Equal([0.001, 0.01, 0.1, 1], new LogScale(0.001, 1).Ticks());
	}

	[Theory]
	[InlineData(0, 10)]
	[InlineData(-1, 10)]
	public void Constructor_WhenNotPositive_Throws(double min, double max) {
		Assert.Throws<ArgumentException>(() => new LogScale(min, max));
	}
}

public class BandScaleTests {
	[Fact]
	public void Keys_AreDistinctInOrderOfAppearance() {
		var scale = new BandScale<string>(["Mar", "Jan", "Mar", "Feb", "Jan"]);

		Assert.Equal(["Mar", "Jan", "Feb"], scale.Keys);
	}

	[Fact]
	public void Bands_FillExactlyZeroToOne() {
		var scale = new BandScale<string>(["A", "B", "C", "D"], paddingInner: 0.2, paddingOuter: 0.1);

		var first = scale.Start("A");
		var last = scale.Start("D") + scale.Bandwidth;

		Assert.Equal(scale.Step * 0.1, first, 12);
		Assert.Equal(1 - first, last, 12);
	}

	[Fact]
	public void Map_IsEvenlySpacedBandCentres() {
		var scale = new BandScale<int>([1, 2, 3], paddingInner: 0, paddingOuter: 0);

		Assert.Equal(1 / 6d, scale.Map(1), 12);
		Assert.Equal(0.5, scale.Map(2), 12);
		Assert.Equal(5 / 6d, scale.Map(3), 12);
	}

	[Fact]
	public void Map_SingleCategory_IsCentred() {
		var scale = new BandScale<string>(["Only"], paddingInner: 0.5, paddingOuter: 0);

		Assert.Equal(0.5, scale.Map("Only"), 12);
	}

	[Fact]
	public void Map_UnknownCategory_IsNaN() {
		var scale = new BandScale<string>(["A"]);

		Assert.True(double.IsNaN(scale.Map("Z")));
		Assert.False(scale.Contains("Z"));
	}
}

public class TimeScaleTests {
	[Fact]
	public void Ticks_ForAYear_AreFirstOfEveryOtherMonthOrQuarter() {
		var scale = new TimeScale(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), tickCount: 6);

		var ticks = scale.Ticks();

		Assert.All(ticks, tick => Assert.Equal(1, tick.Day));
		Assert.Equal(TimeUnit.Month, scale.Interval().Unit);
		Assert.Equal(new DateTime(2026, 1, 1), ticks[0]);
	}

	[Fact]
	public void Ticks_ForADay_AreWholeHours() {
		var scale = new TimeScale(new DateTime(2026, 3, 1, 0, 10, 0), new DateTime(2026, 3, 1, 23, 50, 0), tickCount: 8);

		var ticks = scale.Ticks();

		Assert.Equal((TimeUnit.Hour, 3), scale.Interval());
		Assert.Equal(new DateTime(2026, 3, 1, 3, 0, 0), ticks[0]);
		Assert.All(ticks, tick => Assert.Equal(0, tick.Hour % 3));
	}

	[Fact]
	public void Ticks_ForWeeks_AreMondays() {
		var scale = new TimeScale(new DateTime(2026, 1, 1), new DateTime(2026, 2, 20), tickCount: 8);

		var ticks = scale.Ticks();

		Assert.Equal(TimeUnit.Week, scale.Interval().Unit);
		Assert.All(ticks, tick => Assert.Equal(DayOfWeek.Monday, tick.DayOfWeek));
	}

	[Fact]
	public void Ticks_ForDecades_UseNiceYearSteps() {
		var scale = new TimeScale(new DateTime(1990, 6, 1), new DateTime(2026, 1, 1), tickCount: 6);

		var ticks = scale.Ticks();

		Assert.Equal(TimeUnit.Year, scale.Interval().Unit);
		Assert.All(ticks, tick => Assert.Equal(0, tick.Year % scale.Interval().Count));
	}

	[Fact]
	public void Map_IsLinearInTime() {
		var scale = new TimeScale(new DateTime(2026, 1, 1), new DateTime(2026, 1, 3));

		Assert.Equal(0.5, scale.Map(new DateTime(2026, 1, 2)), 12);
	}
}
