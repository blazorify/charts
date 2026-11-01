using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Scales {
	/// <summary>
	/// Maps a <see cref="DateTime"/> domain linearly onto 0..1, with ticks on calendar boundaries.
	/// </summary>
	/// <remarks>
	/// Ticks fall on whole seconds, minutes, hours, days, weeks (Mondays), months or years rather than on
	/// round numbers of ticks, so they line up with what a reader expects of a time axis. Month and year
	/// steps use calendar arithmetic, so they stay on the first of the month however long the months are.
	/// </remarks>
	internal sealed class TimeScale {
		private static readonly (TimeUnit Unit, Int32 Count)[] Intervals = [
			(TimeUnit.Second, 1), (TimeUnit.Second, 5), (TimeUnit.Second, 15), (TimeUnit.Second, 30),
			(TimeUnit.Minute, 1), (TimeUnit.Minute, 5), (TimeUnit.Minute, 15), (TimeUnit.Minute, 30),
			(TimeUnit.Hour, 1), (TimeUnit.Hour, 3), (TimeUnit.Hour, 6), (TimeUnit.Hour, 12),
			(TimeUnit.Day, 1), (TimeUnit.Day, 2), (TimeUnit.Week, 1),
			(TimeUnit.Month, 1), (TimeUnit.Month, 3), (TimeUnit.Month, 6),
			(TimeUnit.Year, 1),
		];

		public TimeScale(DateTime min, DateTime max, Int32 tickCount = 6) {
			if (min > max) {
				(min, max) = (max, min);
			}

			if (min == max) {
				min = min.AddHours(-12);
				max = max.AddHours(12);
			}

			this.Min = min;
			this.Max = max;
			this.TickCount = Math.Max(1, tickCount);
		}

		public DateTime Min { get; }

		public DateTime Max { get; }

		public Int32 TickCount { get; }

		public Double Map(DateTime value) {
			return (Double)(value.Ticks - this.Min.Ticks) / (this.Max.Ticks - this.Min.Ticks);
		}

		/// <summary>
		/// The calendar interval the ticks are spaced by.
		/// </summary>
		public (TimeUnit Unit, Int32 Count) Interval() {
			var span = this.Max - this.Min;

			foreach (var interval in Intervals) {
				if (Approximate(interval) * this.TickCount >= span) {
					return interval;
				}
			}

			// Beyond yearly ticks the year itself is a number, so let the numeric nice-step pick 2, 5, 10 ...
			var years = NiceTicks.Step(this.Min.Year, this.Max.Year + 1, this.TickCount);

			return (TimeUnit.Year, Math.Max(1, (Int32)years));
		}

		public IReadOnlyList<DateTime> Ticks() {
			var (unit, count) = this.Interval();
			var ticks = new List<DateTime>();
			var current = Floor(this.Min, unit, count);

			if (current < this.Min) {
				current = Advance(current, unit, count);
			}

			while (current <= this.Max) {
				ticks.Add(current);
				current = Advance(current, unit, count);
			}

			return ticks;
		}

		private static TimeSpan Approximate((TimeUnit Unit, Int32 Count) interval) {
			return interval.Unit switch {
				TimeUnit.Second => TimeSpan.FromSeconds(interval.Count),
				TimeUnit.Minute => TimeSpan.FromMinutes(interval.Count),
				TimeUnit.Hour => TimeSpan.FromHours(interval.Count),
				TimeUnit.Day => TimeSpan.FromDays(interval.Count),
				TimeUnit.Week => TimeSpan.FromDays(7 * interval.Count),
				TimeUnit.Month => TimeSpan.FromDays(30.44 * interval.Count),
				_ => TimeSpan.FromDays(365.25 * interval.Count),
			};
		}

		private static DateTime Floor(DateTime value, TimeUnit unit, Int32 count) {
			return unit switch {
				TimeUnit.Second => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second - (value.Second % count), value.Kind),
				TimeUnit.Minute => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute - (value.Minute % count), 0, value.Kind),
				TimeUnit.Hour => new DateTime(value.Year, value.Month, value.Day, value.Hour - (value.Hour % count), 0, 0, value.Kind),
				TimeUnit.Day => new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind),
				TimeUnit.Week => new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind).AddDays(-(((Int32)value.DayOfWeek + 6) % 7)),
				TimeUnit.Month => new DateTime(value.Year, value.Month - ((value.Month - 1) % count), 1, 0, 0, 0, value.Kind),
				_ => new DateTime(value.Year - (value.Year % count), 1, 1, 0, 0, 0, value.Kind),
			};
		}

		private static DateTime Advance(DateTime value, TimeUnit unit, Int32 count) {
			return unit switch {
				TimeUnit.Second => value.AddSeconds(count),
				TimeUnit.Minute => value.AddMinutes(count),
				TimeUnit.Hour => value.AddHours(count),
				TimeUnit.Day => value.AddDays(count),
				TimeUnit.Week => value.AddDays(7 * count),
				TimeUnit.Month => value.AddMonths(count),
				_ => value.AddYears(count),
			};
		}
	}

	internal enum TimeUnit {
		Second,
		Minute,
		Hour,
		Day,
		Week,
		Month,
		Year,
	}
}
