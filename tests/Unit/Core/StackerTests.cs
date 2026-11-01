using Blazorify.Charts.Core.Data;
using Xunit;

namespace Blazorify.Charts.Tests.Unit.Core;

public class StackerTests {
	[Fact]
	public void Stack_MatchesPointsByKeyNotPosition() {
		IReadOnlyList<IReadOnlyList<(string, double?)>> series = [
			[("Jan", 1), ("Feb", 2), ("Mar", 3)],
			[("Mar", 10), ("Jan", 20)],
		];

		var result = Stacker.Stack(series);

		Assert.Equal((3, 13), result[1][0]);
		Assert.Equal((1, 21), result[1][1]);
	}

	[Fact]
	public void Stack_NegativesStackDownwardsSeparately() {
		IReadOnlyList<IReadOnlyList<(string, double?)>> series = [
			[("A", 5)],
			[("A", -3)],
			[("A", 2)],
			[("A", -1)],
		];

		var result = Stacker.Stack(series);

		Assert.Equal((0, 5), result[0][0]);
		Assert.Equal((0, -3), result[1][0]);
		Assert.Equal((5, 7), result[2][0]);
		Assert.Equal((-3, -4), result[3][0]);
	}

	[Fact]
	public void Stack_MissingValuesTakeNoSpace() {
		IReadOnlyList<IReadOnlyList<(string, double?)>> series = [
			[("A", 1)],
			[("A", null)],
			[("A", double.NaN)],
			[("A", 2)],
		];

		var result = Stacker.Stack(series);

		Assert.True(double.IsNaN(result[1][0].Low));
		Assert.True(double.IsNaN(result[2][0].High));
		Assert.Equal((1, 3), result[3][0]);
	}

	[Fact]
	public void Stack_Percent_SumsToOnePerKey() {
		IReadOnlyList<IReadOnlyList<(string, double?)>> series = [
			[("A", 1), ("B", 3)],
			[("A", 3), ("B", 1)],
		];

		var result = Stacker.Stack(series, StackMode.Percent);

		Assert.Equal((0.25, 1), result[1][0]);
		Assert.Equal((0.75, 1), result[1][1]);
	}

	[Fact]
	public void Stack_Percent_AllZero_DoesNotDivideByZero() {
		IReadOnlyList<IReadOnlyList<(string, double?)>> series = [[("A", 0)], [("A", 0)]];

		var result = Stacker.Stack(series, StackMode.Percent);

		Assert.Equal((0, 0), result[1][0]);
	}

	[Fact]
	public void Stack_RepeatedKeyInOneSeries_GetsItsOwnSegment() {
		IReadOnlyList<IReadOnlyList<(string, double?)>> series = [[("A", 1), ("A", 2)]];

		var result = Stacker.Stack(series);

		Assert.Equal((0, 1), result[0][0]);
		Assert.Equal((1, 3), result[0][1]);
	}

	// Chart.js #12154: stack state survived a dataset swap and drew a lower line on top of a higher one.
	[Fact]
	public void Stack_AfterSwappingSeries_HoldsNoState() {
		IReadOnlyList<(string, double?)> low = [("A", 1), ("B", 1)];
		IReadOnlyList<(string, double?)> high = [("A", 5), ("B", 5)];

		Stacker.Stack([low, high]);
		var swapped = Stacker.Stack([high, low]);

		Assert.Equal((0, 5), swapped[0][0]);
		Assert.Equal((5, 6), swapped[1][1]);
	}
}

public class DecimatorTests {
	private static List<PlotPoint> Wave(int count) {
		return [.. Enumerable.Range(0, count).Select(i => new PlotPoint(i, Math.Sin(i / 50d) + (i == 777 ? 10 : 0)))];
	}

	[Fact]
	public void Lttb_ReducesToThresholdKeepingEnds() {
		var points = Wave(10_000);

		var result = Decimator.Lttb(points, 500);

		Assert.InRange(result.Count, 490, 510);
		Assert.Equal(points[0], result[0]);
		Assert.Equal(points[^1], result[^1]);
	}

	[Fact]
	public void Lttb_KeepsASpike() {
		var result = Decimator.Lttb(Wave(10_000), 500);

		Assert.Contains(result, point => point.X == 777);
	}

	[Fact]
	public void MinMax_KeepsEveryExtreme() {
		var points = Wave(10_000);

		var result = Decimator.MinMax(points, 400);

		Assert.Contains(result, point => point.Y == points.Max(p => p.Y));
		Assert.Contains(result, point => point.Y == points.Min(p => p.Y));
		Assert.True(result.Count <= 400);
	}

	[Fact]
	public void Decimate_BelowThreshold_ReturnsInput() {
		var points = Wave(100);

		Assert.Same(points, Decimator.Lttb(points, 500));
	}

	[Fact]
	public void Decimate_KeepsGaps() {
		var points = Wave(2_000);

		points[1_000] = PlotPoint.Gap(1_000);

		var result = Decimator.Lttb(points, 100);

		Assert.Single(result, point => point.IsGap);
		Assert.Contains(result, point => point.X == 999);
		Assert.Contains(result, point => point.X == 1_001);
	}

	[Fact]
	public void Decimate_KeepsXOrder() {
		var result = Decimator.Lttb(Wave(5_000), 200);

		Assert.Equal(result.OrderBy(point => point.X), result);
	}
}
