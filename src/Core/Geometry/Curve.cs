namespace Blazorify.Charts.Core.Geometry {
	/// <summary>
	/// How a line travels between its points.
	/// </summary>
	public enum Curve {
		/// <summary>
		/// Straight segments.
		/// </summary>
		Linear,

		/// <summary>
		/// Straight segments with rounded corners. The curve stays inside the corner, so it never draws a value
		/// beyond the points it joins.
		/// </summary>
		Rounded,

		/// <summary>
		/// A smooth curve that is monotone between points: it never overshoots a peak or dips below a trough.
		/// </summary>
		Monotone,

		/// <summary>
		/// Steps that change value half-way between points.
		/// </summary>
		Step,

		/// <summary>
		/// Steps that change value at each point and hold it until the next.
		/// </summary>
		StepAfter,

		/// <summary>
		/// Steps that change value just before each point.
		/// </summary>
		StepBefore,
	}
}
