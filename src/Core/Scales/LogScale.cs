using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Scales {
	/// <summary>
	/// Maps a strictly positive domain onto 0..1 by its base-10 logarithm.
	/// </summary>
	internal sealed class LogScale {
		public LogScale(Double min, Double max, Boolean nice = true) {
			if (min <= 0 || max <= 0 || Double.IsNaN(min) || Double.IsNaN(max)) {
				throw new ArgumentException("A logarithmic scale needs a strictly positive domain.");
			}

			if (min > max) {
				(min, max) = (max, min);
			}

			if (nice) {
				min = Math.Pow(10, Math.Floor(Math.Log10(min)));
				max = Math.Pow(10, Math.Ceiling(Math.Log10(max)));
			}

			if (min == max) {
				min /= 10;
				max *= 10;
			}

			this.Min = min;
			this.Max = max;
		}

		public Double Min { get; }

		public Double Max { get; }

		public Double Map(Double value) {
			if (value <= 0) {
				return Double.NaN;
			}

			return (Math.Log10(value) - Math.Log10(this.Min)) / (Math.Log10(this.Max) - Math.Log10(this.Min));
		}

		/// <summary>
		/// One tick per power of ten in the domain.
		/// </summary>
		public IReadOnlyList<Double> Ticks() {
			var ticks = new List<Double>();
			var first = (Int32)Math.Ceiling(Math.Log10(this.Min) - 1e-9);
			var last = (Int32)Math.Floor(Math.Log10(this.Max) + 1e-9);

			for (var exponent = first; exponent <= last; exponent++) {
				// Parsing "1e-3" gives the exact nearest Double, unlike Math.Pow(10, -3).
				ticks.Add(Double.Parse($"1e{exponent}", System.Globalization.CultureInfo.InvariantCulture));
			}

			return ticks;
		}
	}
}
