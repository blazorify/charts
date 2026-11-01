using Blazorify.Charts.Core;
using Xunit;

namespace Blazorify.Charts.Tests.Unit.Core;

public class SvgNumberTests {
	[Theory]
	[InlineData(0, "0")]
	[InlineData(1, "1")]
	[InlineData(1.5, "1.5")]
	[InlineData(-2.25, "-2.25")]
	[InlineData(0.123456, "0.1235")]
	[InlineData(1e-7, "0")]
	[InlineData(-1e-7, "0")]
	[InlineData(123456789, "123456789")]
	public void Format_WritesInvariantFixedPrecision(double value, string expected) {
		Assert.Equal(expected, SvgNumber.Format(value));
	}

	[Fact]
	public void Format_UnderCommaCulture_StillWritesDot() {
		using var _ = new CultureScope();

		Assert.Equal("1.5", SvgNumber.Format(1.5));
	}

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Format_WhenNotFinite_Throws(double value) {
		Assert.Throws<ArgumentOutOfRangeException>(() => SvgNumber.Format(value));
	}
}
