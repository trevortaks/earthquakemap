# earthquakemap

This project visualises recent earthquakes on a world map using WPF.
It has been upgraded to target **.NET 8** and now uses the modern SDK
style project format. To build the application on Windows run:

```bash
dotnet build
```

WPF projects require the Windows Desktop SDK, so building should be done on a
Windows machine with the .NET 8 SDK installed.

The application fetches data from the USGS earthquake API and plots
markers on a map.
