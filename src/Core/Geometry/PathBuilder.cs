using System;
using System.Collections.Generic;
using System.Text;
using Blazorify.Charts.Core.Data;

namespace Blazorify.Charts.Core.Geometry {
	/// <summary>
	/// Builds SVG path data for lines, areas and arcs.
	/// </summary>
	/// <remarks>
	/// Builders are coordinate-agnostic: they write whatever coordinates they are given, so the caller decides the
	/// space (plot units for cartesian charts, pixels for radial ones). Gaps (NaN points) break the path into
	/// separate sub-paths. All numbers go through <see cref="SvgNumber"/>.
	/// </remarks>
	internal static class PathBuilder {
		public static String Line(IReadOnlyList<PlotPoint> points, Curve curve = Curve.Linear, Double rounding = 0.25, Boolean spanGaps = false) {
			var path = new StringBuilder();

			foreach (var run in Runs(points, spanGaps)) {
				WriteRun(path, run, curve, rounding, moveFirst: true);
			}

			return path.ToString();
		}

		/// <summary>
		/// A closed area between a top edge and a bottom edge, sampled at the same points.
		/// </summary>
		/// <remarks>
		/// Gaps in either edge break the area. Each run is drawn along the top, back along the bottom and closed,
		/// using the same curve for both edges so the band stays even.
		/// </remarks>
		public static String Area(IReadOnlyList<PlotPoint> top, IReadOnlyList<PlotPoint> bottom, Curve curve = Curve.Linear, Double rounding = 0.25, Boolean spanGaps = false) {
			if (top.Count != bottom.Count) {
				throw new ArgumentException("An area needs the same number of top and bottom points.", nameof(bottom));
			}

			var pairs = new List<(PlotPoint Top, PlotPoint Bottom)>(top.Count);

			for (var i = 0; i < top.Count; i++) {
				pairs.Add((top[i], bottom[i]));
			}

			var path = new StringBuilder();
			var run = new List<(PlotPoint Top, PlotPoint Bottom)>();

			foreach (var pair in pairs) {
				if (pair.Top.IsGap || pair.Bottom.IsGap) {
					if (!spanGaps) {
						WriteArea(path, run, curve, rounding);
						run.Clear();
					}

					continue;
				}

				run.Add(pair);
			}

			WriteArea(path, run, curve, rounding);

			return path.ToString();
		}

		/// <summary>
		/// An annular sector (a pie or doughnut slice) in degrees, 0 at twelve o'clock, growing clockwise.
		/// </summary>
		public static String Arc(Double centerX, Double centerY, Double outerRadius, Double innerRadius, Double startAngle, Double endAngle) {
			var sweep = endAngle - startAngle;

			if (sweep <= 0 || outerRadius <= 0) {
				return String.Empty;
			}

			innerRadius = Math.Clamp(innerRadius, 0, outerRadius);

			// A full turn cannot be one arc command (start and end coincide), so draw it as two halves.
			if (sweep >= 360) {
				var middle = startAngle + 180;

				return Arc(centerX, centerY, outerRadius, innerRadius, startAngle, middle) + Arc(centerX, centerY, outerRadius, innerRadius, middle, startAngle + 360);
			}

			var large = sweep > 180 ? 1 : 0;
			var path = new StringBuilder();
			var outerStart = Polar(centerX, centerY, outerRadius, startAngle);
			var outerEnd = Polar(centerX, centerY, outerRadius, endAngle);

			path.Append('M').Append(Pair(outerStart));
			path.Append('A').Append(SvgNumber.Format(outerRadius)).Append(' ').Append(SvgNumber.Format(outerRadius)).Append(" 0 ").Append(large).Append(" 1 ").Append(Pair(outerEnd));

			if (innerRadius > 0) {
				var innerEnd = Polar(centerX, centerY, innerRadius, endAngle);
				var innerStart = Polar(centerX, centerY, innerRadius, startAngle);

				path.Append('L').Append(Pair(innerEnd));
				path.Append('A').Append(SvgNumber.Format(innerRadius)).Append(' ').Append(SvgNumber.Format(innerRadius)).Append(" 0 ").Append(large).Append(" 0 ").Append(Pair(innerStart));
			} else {
				path.Append('L').Append(SvgNumber.Format(centerX)).Append(' ').Append(SvgNumber.Format(centerY));
			}

			path.Append('Z');

			return path.ToString();
		}

		/// <summary>
		/// The point at an angle in degrees, 0 at twelve o'clock, growing clockwise (SVG's Y grows downwards).
		/// </summary>
		public static (Double X, Double Y) Polar(Double centerX, Double centerY, Double radius, Double angle) {
			var radians = angle * Math.PI / 180;

			return (centerX + (radius * Math.Sin(radians)), centerY - (radius * Math.Cos(radians)));
		}

		private static IEnumerable<List<PlotPoint>> Runs(IReadOnlyList<PlotPoint> points, Boolean spanGaps) {
			var run = new List<PlotPoint>();

			foreach (var point in points) {
				if (point.IsGap) {
					if (!spanGaps && run.Count > 0) {
						yield return run;
						run = [];
					}

					continue;
				}

				run.Add(point);
			}

			if (run.Count > 0) {
				yield return run;
			}
		}

		private static void WriteArea(StringBuilder path, List<(PlotPoint Top, PlotPoint Bottom)> run, Curve curve, Double rounding) {
			if (run.Count == 0) {
				return;
			}

			var top = new List<PlotPoint>(run.Count);
			var bottom = new List<PlotPoint>(run.Count);

			foreach (var (upper, lower) in run) {
				top.Add(upper);
				bottom.Add(lower);
			}

			bottom.Reverse();

			WriteRun(path, top, curve, rounding, moveFirst: true);
			WriteRun(path, bottom, Mirror(curve), rounding, moveFirst: false);
			path.Append('Z');
		}

		// Walking the bottom edge backwards turns "change after the point" into "change before it".
		private static Curve Mirror(Curve curve) {
			return curve switch {
				Curve.StepAfter => Curve.StepBefore,
				Curve.StepBefore => Curve.StepAfter,
				_ => curve,
			};
		}

		private static void WriteRun(StringBuilder path, IReadOnlyList<PlotPoint> run, Curve curve, Double rounding, Boolean moveFirst) {
			if (run.Count == 0) {
				return;
			}

			Point(path, moveFirst ? 'M' : 'L', run[0]);

			if (run.Count == 1) {
				return;
			}

			switch (curve) {
				case Curve.Rounded:
					WriteRounded(path, run, Math.Clamp(rounding, 0, 0.5));
					break;
				case Curve.Monotone:
					WriteMonotone(path, run);
					break;
				case Curve.Step:
				case Curve.StepAfter:
				case Curve.StepBefore:
					WriteStep(path, run, curve);
					break;
				default:
					for (var i = 1; i < run.Count; i++) {
						Point(path, 'L', run[i]);
					}

					break;
			}
		}

		// Each corner is cut back along both segments meeting at it and replaced by a quadratic through the
		// vertex, so the curve stays inside the corner it replaces however sharp it is.
		private static void WriteRounded(StringBuilder path, IReadOnlyList<PlotPoint> run, Double amount) {
			if (amount == 0) {
				for (var i = 1; i < run.Count; i++) {
					Point(path, 'L', run[i]);
				}

				return;
			}

			for (var i = 1; i < run.Count - 1; i++) {
				var previous = run[i - 1];
				var corner = run[i];
				var next = run[i + 1];

				Point(path, 'L', new PlotPoint(corner.X + ((previous.X - corner.X) * amount), corner.Y + ((previous.Y - corner.Y) * amount)));
				Point(path, 'Q', corner);
				Point(path, ' ', new PlotPoint(corner.X + ((next.X - corner.X) * amount), corner.Y + ((next.Y - corner.Y) * amount)));
			}

			Point(path, 'L', run[^1]);
		}

		// Monotone cubic interpolation along X (Fritsch-Carlson, as in d3's curveMonotoneX): tangents are limited
		// so each segment stays between its two end values. Works for X running in either direction.
		private static void WriteMonotone(StringBuilder path, IReadOnlyList<PlotPoint> run) {
			if (run.Count == 2) {
				Point(path, 'L', run[1]);
				return;
			}

			var count = run.Count;
			var slopes = new Double[count - 1];
			var tangents = new Double[count];

			for (var i = 0; i < count - 1; i++) {
				var dx = run[i + 1].X - run[i].X;

				slopes[i] = dx == 0 ? 0 : (run[i + 1].Y - run[i].Y) / dx;
			}

			tangents[0] = slopes[0];
			tangents[count - 1] = slopes[count - 2];

			for (var i = 1; i < count - 1; i++) {
				tangents[i] = (slopes[i - 1] * slopes[i]) <= 0 ? 0 : (slopes[i - 1] + slopes[i]) / 2;
			}

			for (var i = 0; i < count - 1; i++) {
				if (slopes[i] == 0) {
					tangents[i] = 0;
					tangents[i + 1] = 0;
					continue;
				}

				var a = tangents[i] / slopes[i];
				var b = tangents[i + 1] / slopes[i];
				var length = (a * a) + (b * b);

				if (length > 9) {
					var scale = 3 / Math.Sqrt(length);

					tangents[i] = scale * a * slopes[i];
					tangents[i + 1] = scale * b * slopes[i];
				}
			}

			for (var i = 0; i < count - 1; i++) {
				var start = run[i];
				var end = run[i + 1];
				var third = (end.X - start.X) / 3;

				Point(path, 'C', new PlotPoint(start.X + third, start.Y + (tangents[i] * third)));
				Point(path, ' ', new PlotPoint(end.X - third, end.Y - (tangents[i + 1] * third)));
				Point(path, ' ', end);
			}
		}

		private static void WriteStep(StringBuilder path, IReadOnlyList<PlotPoint> run, Curve curve) {
			for (var i = 1; i < run.Count; i++) {
				var previous = run[i - 1];
				var current = run[i];

				switch (curve) {
					case Curve.StepAfter:
						Point(path, 'L', new PlotPoint(current.X, previous.Y));
						break;
					case Curve.StepBefore:
						Point(path, 'L', new PlotPoint(previous.X, current.Y));
						break;
					default:
						var middle = (previous.X + current.X) / 2;

						Point(path, 'L', new PlotPoint(middle, previous.Y));
						Point(path, 'L', new PlotPoint(middle, current.Y));
						break;
				}

				Point(path, 'L', current);
			}
		}

		private static void Point(StringBuilder path, Char command, PlotPoint point) {
			path.Append(command).Append(SvgNumber.Format(point.X)).Append(' ').Append(SvgNumber.Format(point.Y));
		}

		private static String Pair((Double X, Double Y) point) {
			return SvgNumber.Format(point.X) + " " + SvgNumber.Format(point.Y);
		}
	}
}
