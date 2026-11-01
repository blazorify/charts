using System.Globalization;
using System.Text.RegularExpressions;
using Blazorify.Charts.Core.Data;
using Blazorify.Charts.Core.Geometry;
using Xunit;

namespace Blazorify.Charts.Tests.Unit.Core;

public class PathBuilderTests {
	private static readonly PlotPoint[] Zigzag = [new(0, 1), new(1, 0), new(2, 1), new(3, 0.5)];

	[Fact]
	public void Line_Linear() {
		Assert.Equal("M0 1L1 0L2 1L3 0.5", PathBuilder.Line(Zigzag));
	}

	[Fact]
	public void Line_SinglePoint_IsJustAMove() {
		Assert.Equal("M1 2", PathBuilder.Line([new PlotPoint(1, 2)]));
	}

	[Fact]
	public void Line_Empty_IsEmpty() {
		Assert.Equal(string.Empty, PathBuilder.Line([]));
	}

	[Fact]
	public void Line_GapSplitsIntoSubPaths() {
		PlotPoint[] points = [new(0, 0), new(1, 1), PlotPoint.Gap(2), new(3, 0), new(4, 1)];

		Assert.Equal("M0 0L1 1M3 0L4 1", PathBuilder.Line(points));
	}

	[Fact]
	public void Line_SpanGaps_JoinsAcross() {
		PlotPoint[] points = [new(0, 0), PlotPoint.Gap(1), new(2, 1)];

		Assert.Equal("M0 0L2 1", PathBuilder.Line(points, spanGaps: true));
	}

	[Fact]
	public void Line_Step() {
		PlotPoint[] points = [new(0, 0), new(2, 1)];

		Assert.Equal("M0 0L1 0L1 1L2 1", PathBuilder.Line(points, Curve.Step));
		Assert.Equal("M0 0L2 0L2 1", PathBuilder.Line(points, Curve.StepAfter));
		Assert.Equal("M0 0L0 1L2 1", PathBuilder.Line(points, Curve.StepBefore));
	}

	[Fact]
	public void Line_Rounded_StaysInsideTheCorners() {
		var path = PathBuilder.Line(Zigzag, Curve.Rounded, 0.5);

		Assert.All(Numbers(path).Chunk(2), pair => {
			Assert.InRange(pair[0], 0, 3);
			Assert.InRange(pair[1], 0, 1);
		});
	}

	[Fact]
	public void Line_Rounded_ZeroAmount_IsLinear() {
		Assert.Equal(PathBuilder.Line(Zigzag), PathBuilder.Line(Zigzag, Curve.Rounded, 0));
	}

	[Fact]
	public void Line_Monotone_NeverOvershootsItsPoints() {
		var random = new Random(7);
		var points = Enumerable.Range(0, 50).Select(i => new PlotPoint(i, random.NextDouble())).ToArray();

		var path = PathBuilder.Line(points, Curve.Monotone);

		// Sample every cubic segment and check it stays between its two end values.
		var numbers = Numbers(path);
		var segments = 0;

		for (var i = 2; i + 5 < numbers.Count; i += 6) {
			var (y0, c1, c2, y3) = (numbers[i - 1], numbers[i + 1], numbers[i + 3], numbers[i + 5]);
			var low = Math.Min(y0, y3) - 1e-3;
			var high = Math.Max(y0, y3) + 1e-3;

			for (var t = 0d; t <= 1; t += 0.05) {
				var y = (Math.Pow(1 - t, 3) * y0) + (3 * Math.Pow(1 - t, 2) * t * c1) + (3 * (1 - t) * t * t * c2) + (Math.Pow(t, 3) * y3);

				Assert.InRange(y, low, high);
			}

			segments++;
		}

		Assert.Equal(49, segments);
	}

	[Fact]
	public void Line_Monotone_FlatRun_IsFlat() {
		PlotPoint[] points = [new(0, 0.5), new(1, 0.5), new(2, 0.5)];

		Assert.All(Numbers(PathBuilder.Line(points, Curve.Monotone)).Where((_, index) => index % 2 == 1), y => Assert.Equal(0.5, y, 4));
	}

	[Fact]
	public void Area_IsClosedAlongTopThenBackAlongBottom() {
		PlotPoint[] top = [new(0, 0), new(1, 0.5)];
		PlotPoint[] bottom = [new(0, 1), new(1, 1)];

		Assert.Equal("M0 0L1 0.5L1 1L0 1Z", PathBuilder.Area(top, bottom));
	}

	[Fact]
	public void Area_GapSplitsIntoClosedPieces() {
		PlotPoint[] top = [new(0, 0), new(1, 0), PlotPoint.Gap(2), new(3, 0), new(4, 0)];
		PlotPoint[] bottom = [new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1)];

		Assert.Equal("M0 0L1 0L1 1L0 1ZM3 0L4 0L4 1L3 1Z", PathBuilder.Area(top, bottom));
	}

	[Fact]
	public void Area_StepAfter_BottomMirrorsTop() {
		PlotPoint[] top = [new(0, 0), new(2, 0.5)];
		PlotPoint[] bottom = [new(0, 1), new(2, 1)];

		// Both edges change value at x = 2.
		Assert.Equal("M0 0L2 0L2 0.5L2 1L2 1L0 1Z", PathBuilder.Area(top, bottom, Curve.StepAfter));
	}

	[Fact]
	public void Area_MismatchedEdges_Throws() {
		Assert.Throws<ArgumentException>(() => PathBuilder.Area([new(0, 0)], []));
	}

	[Fact]
	public void Arc_QuarterPie_StartsAtTwelveAndRunsClockwise() {
		Assert.Equal("M50 0A50 50 0 0 1 100 50L50 50Z", PathBuilder.Arc(50, 50, 50, 0, 0, 90));
	}

	[Fact]
	public void Arc_Doughnut_ReturnsAlongTheInnerEdge() {
		Assert.Equal("M50 0A50 50 0 0 1 100 50L75 50A25 25 0 0 0 50 25Z", PathBuilder.Arc(50, 50, 50, 25, 0, 90));
	}

	[Fact]
	public void Arc_MoreThanHalf_UsesLargeArcFlag() {
		Assert.Contains(" 0 1 1 ", PathBuilder.Arc(50, 50, 50, 0, 0, 270));
	}

	[Fact]
	public void Arc_FullTurn_IsTwoHalves() {
		var path = PathBuilder.Arc(50, 50, 50, 0, 0, 360);

		Assert.Equal(2, path.Count(c => c == 'A'));
	}

	[Fact]
	public void Arc_EmptySweep_IsEmpty() {
		Assert.Equal(string.Empty, PathBuilder.Arc(50, 50, 50, 0, 90, 90));
	}

	[Fact]
	public void Paths_UnderCommaCulture_UseDots() {
		using var _ = new CultureScope();

		Assert.Equal("M0.5 0.25L1.5 0.75", PathBuilder.Line([new(0.5, 0.25), new(1.5, 0.75)]));
		Assert.DoesNotContain(",", PathBuilder.Line(Zigzag, Curve.Monotone));
	}

	private static List<double> Numbers(string path) {
		return [.. Regex.Matches(path, @"-?\d+(\.\d+)?").Select(match => double.Parse(match.Value, CultureInfo.InvariantCulture))];
	}
}
