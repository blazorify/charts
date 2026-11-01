using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Scales {
	/// <summary>
	/// Maps a continuous numeric domain linearly onto 0..1.
	/// </summary>
	internal sealed class LinearScale {
		public LinearScale(Double min, Double max, Boolean nice = true, Int32 tickCount = 5) {
			if (Double.IsNaN(min) || Double.IsNaN(max)) {
				throw new ArgumentException("A scale domain cannot be NaN.");
			}

			if (min > max) {
				(min, max) = (max, min);
			}

			// A flat domain has nothing to spread across the axis. Pad it so the values sit in the middle
			// instead of on the floor, which would read as "always at its lowest".
			if (min == max) {
				var pad = min == 0 ? 1 : Math.Abs(min) * 0.05;

				min -= pad;
				max += pad;
			}

			if (nice) {
				(min, max) = NiceTicks.Extend(min, max, tickCount);
			}

			this.Min = min;
			this.Max = max;
			this.TickCount = tickCount;
		}

		public Double Min { get; }

		public Double Max { get; }

		public Int32 TickCount { get; }

		public Double Map(Double value) {
			return (value - this.Min) / (this.Max - this.Min);
		}

		public Double Invert(Double position) {
			return this.Min + (position * (this.Max - this.Min));
		}

		public IReadOnlyList<Double> Ticks() {
			return NiceTicks.Values(this.Min, this.Max, this.TickCount);
		}
	}
}
