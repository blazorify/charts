namespace Blazorify.Charts.Core.Data {
	internal enum StackMode {
		/// <summary>
		/// Values are added on top of each other.
		/// </summary>
		Normal,

		/// <summary>
		/// Values are added on top of each other and scaled so each category sums to 1 (100%).
		/// </summary>
		Percent,
	}
}
