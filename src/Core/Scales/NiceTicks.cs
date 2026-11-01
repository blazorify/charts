using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Scales {
	/// <summary>
	/// Chooses "nice" round tick values (1, 2 or 5 times a power of ten) covering a range.
	/// </summary>
	/// <remarks>
	/// Values are computed as an integer multiple of the step and then rounded to the step's own precision,
	/// never by repeated addition, so they carry no floating-point drift (0.30000000000000004).
	/// </remarks>
	internal static class NiceTicks {
		public static Double Step(Double min, Double max, Int32 count) {
			if (count < 1) {
				throw new ArgumentOutOfRangeException(nameof(count), count, "At least one tick is required.");
			}

			var span = max - min;

			if (span <= 0 || Double.IsNaN(span) || Double.IsInfinity(span)) {
				return 0;
			}

			var raw = span / count;
			var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
			var fraction = raw / magnitude;

			Double nice;

			if (fraction <= 1) {
				nice = 1;
			} else if (fraction <= 2) {
				nice = 2;
			} else if (fraction <= 5) {
				nice = 5;
			} else {
				nice = 10;
			}

			return nice * magnitude;
		}

		/// <summary>
		/// Expands a range outwards to the nearest multiples of its nice step.
		/// </summary>
		public static (Double Min, Double Max) Extend(Double min, Double max, Int32 count) {
			var step = Step(min, max, count);

			if (step == 0) {
				return (min, max);
			}

			var decimals = Decimals(step);

			return (
				Math.Round(Math.Floor(Round(min / step)) * step, decimals),
				Math.Round(Math.Ceiling(Round(max / step)) * step, decimals)
			);
		}

		public static IReadOnlyList<Double> Values(Double min, Double max, Int32 count) {
			var step = Step(min, max, count);

			if (step == 0) {
				return Double.IsNaN(min) || Double.IsInfinity(min) ? [] : [min];
			}

			var decimals = Decimals(step);
			var first = (Int64)Math.Ceiling(Round(min / step));
			var last = (Int64)Math.Floor(Round(max / step));
			var values = new List<Double>((Int32)Math.Max(0, (last - first) + 1));

			for (var i = first; i <= last; i++) {
				values.Add(Math.Round(i * step, decimals));
			}

			return values;
		}

		// The number of decimals a step needs, e.g. 0.05 -> 2, 20 -> 0. Rounding to it removes drift.
		internal static Int32 Decimals(Double step) {
			return Math.Clamp(-(Int32)Math.Floor(Math.Log10(step)) + 1, 0, 15);
		}

		// Ratios such as 0.3 / 0.1 come out as 2.9999999999999996; snapping near-integers keeps them on the grid.
		private static Double Round(Double ratio) {
			var nearest = Math.Round(ratio);

			return Math.Abs(ratio - nearest) < 1e-9 ? nearest : ratio;
		}
	}
}
