using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Data {
	/// <summary>
	/// Stacks series on top of each other, matching their points by category key rather than by position.
	/// </summary>
	/// <remarks>
	/// The result is computed from scratch on every call and never cached on chart elements, which is what let
	/// stale stacks survive a dataset swap in Chart.js. Positive and negative values stack away from zero in
	/// their own direction. A missing or NaN value takes no space and leaves the baseline where it was.
	/// A key that repeats within one series stacks on top of itself, so every input point gets its own segment.
	/// </remarks>
	internal static class Stacker {
		public static IReadOnlyList<(Double Low, Double High)[]> Stack<TKey>(
			IReadOnlyList<IReadOnlyList<(TKey Key, Double? Value)>> series,
			StackMode mode = StackMode.Normal,
			IEqualityComparer<TKey>? comparer = null
		) where TKey : notnull {
			Dictionary<TKey, Double>? totals = null;

			if (mode == StackMode.Percent) {
				totals = new Dictionary<TKey, Double>(comparer);

				foreach (var points in series) {
					foreach (var (key, value) in points) {
						if (IsValue(value)) {
							totals[key] = totals.GetValueOrDefault(key) + Math.Abs(value!.Value);
						}
					}
				}
			}

			var positive = new Dictionary<TKey, Double>(comparer);
			var negative = new Dictionary<TKey, Double>(comparer);
			var result = new List<(Double Low, Double High)[]>(series.Count);

			foreach (var points in series) {
				var segments = new (Double Low, Double High)[points.Count];

				for (var i = 0; i < points.Count; i++) {
					var (key, value) = points[i];

					if (!IsValue(value)) {
						segments[i] = (Double.NaN, Double.NaN);
						continue;
					}

					var amount = value!.Value;

					if (totals is not null) {
						var total = totals[key];

						amount = total == 0 ? 0 : amount / total;
					}

					var baselines = amount < 0 ? negative : positive;
					var low = baselines.GetValueOrDefault(key);
					var high = low + amount;

					baselines[key] = high;
					segments[i] = (low, high);
				}

				result.Add(segments);
			}

			return result;
		}

		private static Boolean IsValue(Double? value) {
			return value.HasValue && !Double.IsNaN(value.Value) && !Double.IsInfinity(value.Value);
		}
	}
}
