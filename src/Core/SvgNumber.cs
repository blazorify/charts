using System;
using System.Globalization;

namespace Blazorify.Charts.Core {
	/// <summary>
	/// Formats numbers for SVG attributes: invariant culture, fixed precision, no exponent, no negative zero.
	/// </summary>
	/// <remarks>
	/// Every number written into geometry goes through here. Interpolating a Double directly would use the
	/// current culture and write "1,5" under de-DE, which corrupts path data.
	/// </remarks>
	internal static class SvgNumber {
		public const Int32 Precision = 4;

		public static String Format(Double value) {
			if (Double.IsNaN(value) || Double.IsInfinity(value)) {
				throw new ArgumentOutOfRangeException(nameof(value), value, "SVG geometry cannot contain NaN or infinity.");
			}

			var rounded = Math.Round(value, Precision, MidpointRounding.AwayFromZero);

			// Math.Round can return -0, which would be written as "-0".
			if (rounded == 0) {
				return "0";
			}

			return rounded.ToString("0.####", CultureInfo.InvariantCulture);
		}
	}
}
