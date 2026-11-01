# Blazorify.Charts

Blazorify.Charts is a lightweight, dependency-free SVG charting library for Blazor, with a flexible, composable API and built-in animations.

> **Status:** early preview. The public API is being designed and will change.

## Installation

Install the package via NuGet:

```bash
dotnet add package Blazorify.Charts
```

## Usage

### 1. Register the services
Add `Blazorify.Charts` services in `Program.cs`:

```csharp
builder.Services.AddBlazorifyCharts();
```

Optionally configure the library defaults:

```csharp
builder.Services.AddBlazorifyCharts(options => {
	options.EnableAnimations = true;
	options.AnimationDuration = TimeSpan.FromMilliseconds(400);
});
```

---

## Documentation

More details coming soon.

## Prerequisites

- .NET 10 SDK

## Contributing

Contributions are welcome. Please open an issue to discuss a change before sending a pull request.

## License

This project is licensed under the [MIT License](LICENSE).
