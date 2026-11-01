using System;

namespace Blazorify.Charts.Core.Data {
	/// <summary>
	/// A point in plot space, where both axes run from 0 to 1 and Y grows downwards as in SVG.
	/// </summary>
	/// <remarks>
	/// A point whose <see cref="Y"/> is NaN is a gap: lines and areas break there instead of drawing through it.
	/// </remarks>
	internal readonly record struct PlotPoint(Double X, Double Y) {
		public Boolean IsGap {
			get {
				return Double.IsNaN(this.X) || Double.IsNaN(this.Y);
			}
		}

		public static PlotPoint Gap(Double x) {
			return new PlotPoint(x, Double.NaN);
		}
	}
}
