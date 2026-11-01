using System;

namespace Blazorify.Charts.Options {
	public class BlazorifyChartsOptions {
		/// <summary>
		/// Library-wide default for whether charts animate on first render and on data changes.
		/// </summary>
		public Boolean EnableAnimations { get; set; } = true;

		/// <summary>
		/// Library-wide default duration of chart animations.
		/// </summary>
		public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(400);
	}
}
