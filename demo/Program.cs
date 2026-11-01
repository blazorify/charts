using Blazorify.Charts.Demo.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blazorify.Charts.Demo {
	public class Program {
		public static void Main(System.String[] args) {
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddRazorComponents()
				.AddInteractiveServerComponents();

			builder.Services.AddBlazorifyCharts();

			var app = builder.Build();

			if (!app.Environment.IsDevelopment()) {
				app.UseExceptionHandler("/Error", createScopeForErrors: true);
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseAntiforgery();

			app.MapStaticAssets();
			app.MapRazorComponents<App>()
				.AddInteractiveServerRenderMode();

			app.Run();
		}
	}
}
