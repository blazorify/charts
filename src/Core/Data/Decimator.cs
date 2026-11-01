using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Data {
	/// <summary>
	/// Reduces long series to a target number of points before they are drawn.
	/// </summary>
	/// <remarks>
	/// Thousands of points on a few hundred pixels cost bandwidth over a Blazor Server circuit and DOM time in
	/// WebAssembly without adding anything visible. Gaps are preserved: each run between gaps is reduced on its own.
	/// </remarks>
	internal static class Decimator {
		/// <summary>
		/// Largest-Triangle-Three-Buckets: keeps the overall shape, peaks included, of a line.
		/// </summary>
		public static IReadOnlyList<PlotPoint> Lttb(IReadOnlyList<PlotPoint> points, Int32 threshold) {
			return PerRun(points, threshold, LttbRun);
		}

		/// <summary>
		/// Keeps the first, minimum, maximum and last point of each bucket, so no extreme is ever lost.
		/// </summary>
		public static IReadOnlyList<PlotPoint> MinMax(IReadOnlyList<PlotPoint> points, Int32 threshold) {
			return PerRun(points, threshold, MinMaxRun);
		}

		private static IReadOnlyList<PlotPoint> PerRun(IReadOnlyList<PlotPoint> points, Int32 threshold, Action<List<PlotPoint>, List<PlotPoint>, Int32> reduce) {
			if (threshold < 3 || points.Count <= threshold) {
				return points;
			}

			var output = new List<PlotPoint>(threshold);
			var run = new List<PlotPoint>();
			var total = 0;

			foreach (var point in points) {
				if (!point.IsGap) {
					total++;
				}
			}

			foreach (var point in points) {
				if (point.IsGap) {
					Flush();
					output.Add(point);
				} else {
					run.Add(point);
				}
			}

			Flush();

			return output;

			void Flush() {
				if (run.Count > 0) {
					// Each run gets a share of the budget proportional to its length.
					var budget = Math.Max(3, (Int32)Math.Round((Double)threshold * run.Count / Math.Max(1, total)));

					if (run.Count <= budget) {
						output.AddRange(run);
					} else {
						reduce(run, output, budget);
					}

					run.Clear();
				}
			}
		}

		private static void LttbRun(List<PlotPoint> data, List<PlotPoint> output, Int32 threshold) {
			var bucketSize = (Double)(data.Count - 2) / (threshold - 2);
			var selected = 0;

			output.Add(data[0]);

			for (var bucket = 0; bucket < threshold - 2; bucket++) {
				// The average of the next bucket is the third corner of the triangle.
				var nextStart = (Int32)Math.Floor((bucket + 1) * bucketSize) + 1;
				var nextEnd = Math.Min((Int32)Math.Floor((bucket + 2) * bucketSize) + 1, data.Count);
				Double averageX = 0, averageY = 0;

				for (var i = nextStart; i < nextEnd; i++) {
					averageX += data[i].X;
					averageY += data[i].Y;
				}

				var length = Math.Max(1, nextEnd - nextStart);

				averageX /= length;
				averageY /= length;

				var start = (Int32)Math.Floor(bucket * bucketSize) + 1;
				var end = (Int32)Math.Floor((bucket + 1) * bucketSize) + 1;
				var anchor = data[selected];
				var largest = -1.0;
				var chosen = start;

				for (var i = start; i < end; i++) {
					var area = Math.Abs(((anchor.X - averageX) * (data[i].Y - anchor.Y)) - ((anchor.X - data[i].X) * (averageY - anchor.Y)));

					if (area > largest) {
						largest = area;
						chosen = i;
					}
				}

				output.Add(data[chosen]);
				selected = chosen;
			}

			output.Add(data[^1]);
		}

		private static void MinMaxRun(List<PlotPoint> data, List<PlotPoint> output, Int32 threshold) {
			var buckets = Math.Max(1, threshold / 4);
			var bucketSize = (Double)data.Count / buckets;

			for (var bucket = 0; bucket < buckets; bucket++) {
				var start = (Int32)Math.Floor(bucket * bucketSize);
				var end = Math.Min((Int32)Math.Floor((bucket + 1) * bucketSize), data.Count);

				if (start >= end) {
					continue;
				}

				Int32 min = start, max = start;

				for (var i = start; i < end; i++) {
					if (data[i].Y < data[min].Y) {
						min = i;
					}

					if (data[i].Y > data[max].Y) {
						max = i;
					}
				}

				// In order of appearance, without repeating an index.
				var picks = new SortedSet<Int32> { start, min, max, end - 1 };

				foreach (var index in picks) {
					output.Add(data[index]);
				}
			}
		}
	}
}
