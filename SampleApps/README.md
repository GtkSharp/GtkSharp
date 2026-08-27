# SampleApps

Standalone sample applications that consume GtkSharp **as a NuGet package**,
the way an application author would. They are deliberately outside
`Source/GtkSharp.sln` and use `PackageReference` rather than `ProjectReference`,
so they exercise the packaging as well as the binding.

That distinction earns its keep: the first build of `GettingStarted` failed with
`MSB4024` because `Source/Libs/GtkSharp/GtkSharp.targets` — which ships inside
the package and is imported by the **consumer's** build — contained a double
hyphen inside an XML comment. Nothing in this repository imports that file, so
no amount of building `GtkSharp.sln` could have caught it.

| Sample | What it covers |
|:--|:--|
| [GettingStarted](GettingStarted/) | One page per section of [`Docs/getting-started.md`](../Docs/getting-started.md), demonstrated by running rather than described. |

## Building

The packages must be resolvable. [`NuGet.config`](NuGet.config) lists
`../BuildOutput/NugetPackages` before nuget.org, so packages built in this
repository are picked up first:

```sh
# from the repository root, once
dotnet cake build.cake --BuildTarget=PackageNuGet --Configuration=Release

# then
cd SampleApps
dotnet build GettingStarted.slnx
dotnet run --project GettingStarted
```

You also need a **Gtk 4 runtime**. On Windows the package's `GtkSharp.targets`
downloads a gvsbuild bundle into `%LOCALAPPDATA%\Gtk\4.22.4` on first build
(`-p:SkipGtkInstall=True` opts out); elsewhere install `libgtk-4-1` / `gtk4`
from your distribution.

## The package-id prefix

This branch packs the package ids unprefixed (`GtkSharp4`, `GLibSharp4`, …). The
`net4x.gtk4` branch publishes the same packages with a `4` suffix on the
feed id. **Only the id differs** — the assemblies inside are `GtkSharp.dll`,
`GLibSharp.dll` and so on, and the namespaces are unprefixed, so not a line of
sample code changes between the two.

The projects here reference `$(GtkSharpPackagePrefix)GtkSharp`, with the
property empty by default. To build against the prefixed packages:

```sh
dotnet build GettingStarted.slnx
```

Set `$(GtkSharpVersion)` in the `.csproj` to move to another release.
