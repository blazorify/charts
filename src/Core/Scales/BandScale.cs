using System;
using System.Collections.Generic;

namespace Blazorify.Charts.Core.Scales {
	/// <summary>
	/// Divides 0..1 into equal bands, one per distinct category, in order of first appearance.
	/// </summary>
	/// <remarks>
	/// Categories are matched by value, never by index, so series with different lengths or gaps still line
	/// up under the same label.
	/// </remarks>
	internal sealed class BandScale<TKey> where TKey : notnull {
		private readonly Dictionary<TKey, Int32> indexes;
		private readonly List<TKey> keys;

		public BandScale(IEnumerable<TKey> categories, Double paddingInner = 0.2, Double paddingOuter = 0.1, IEqualityComparer<TKey>? comparer = null) {
			this.indexes = new Dictionary<TKey, Int32>(comparer);
			this.keys = [];

			foreach (var category in categories) {
				if (this.indexes.TryAdd(category, this.keys.Count)) {
					this.keys.Add(category);
				}
			}

			this.PaddingInner = Math.Clamp(paddingInner, 0, 1);
			this.PaddingOuter = Math.Max(0, paddingOuter);

			var count = Math.Max(1, this.keys.Count);

			// Width of one step (band + inner gap), so that count bands, count-1 inner gaps and two outer
			// paddings fill exactly 0..1.
			this.Step = 1 / Math.Max(1e-9, count - this.PaddingInner + (this.PaddingOuter * 2));
			this.Bandwidth = this.Step * (1 - this.PaddingInner);
			this.Offset = this.Step * this.PaddingOuter;
		}

		public IReadOnlyList<TKey> Keys {
			get {
				return this.keys;
			}
		}

		public Double PaddingInner { get; }

		public Double PaddingOuter { get; }

		public Double Step { get; }

		public Double Bandwidth { get; }

		private Double Offset { get; }

		public Boolean Contains(TKey key) {
			return this.indexes.ContainsKey(key);
		}

		/// <summary>
		/// The start of a category's band, or NaN when the category is unknown.
		/// </summary>
		public Double Start(TKey key) {
			return this.indexes.TryGetValue(key, out var index) ? this.Offset + (index * this.Step) : Double.NaN;
		}

		/// <summary>
		/// The centre of a category's band, which is where points and labels sit.
		/// </summary>
		public Double Map(TKey key) {
			return this.Start(key) + (this.Bandwidth / 2);
		}
	}
}
