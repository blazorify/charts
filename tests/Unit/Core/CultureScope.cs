using System.Globalization;

namespace Blazorify.Charts.Tests.Unit.Core;

/// <summary>
/// Runs code under a culture that writes decimals with a comma, which is what breaks naive SVG formatting.
/// </summary>
internal sealed class CultureScope : IDisposable {
	private readonly CultureInfo culture = CultureInfo.CurrentCulture;
	private readonly CultureInfo uiCulture = CultureInfo.CurrentUICulture;

	public CultureScope(string name = "de-DE") {
		CultureInfo.CurrentCulture = new CultureInfo(name);
		CultureInfo.CurrentUICulture = new CultureInfo(name);
	}

	public void Dispose() {
		CultureInfo.CurrentCulture = this.culture;
		CultureInfo.CurrentUICulture = this.uiCulture;
	}
}
