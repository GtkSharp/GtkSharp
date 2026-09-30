#load CakeScripts\GAssembly.cake
#load CakeScripts\Settings.cake
#load CakeScripts\TargetEnvironment.cake

// Cake.FileHelpers and Cake.Incubator used to be loaded here. Nothing in this
// script or in CakeScripts/ ever called into either -- no FileWriteText, no
// ReplaceTextInFiles, no Dump() -- so they were two downloads and two chances
// to fail for nothing, and on a runner carrying only the .NET 10 runtime that
// is exactly what happened: "Failed to install addin 'Cake.FileHelpers'"
// before a single task ran.
//
// The one addin that IS used is in TargetEnvironment.cake, which needs
// Microsoft.Win32.Registry to find the Gtk install on Windows.

// VARS

Settings.Cake = Context;

// 4.22.4 is the Gtk release these bindings describe and stays the version's
// first three components. The fourth is the build date: the last two digits of
// the year followed by the day of the year, zero padded to three so that
// 7 January (4.22.4.26007) still sorts below 7 August (4.22.4.26219).
// Computed on every run rather than written down, so it moves on its own.
//
// That shape is also what everything downstream already expects: the workload
// manifest splits the first three components off with a regex, and
// rebuilds the version as $(VersionPrefix).$(VersionSuffix),
// which here is 4.22.4 and the date.
//
// Source/Directory.Build.props carries the identical expression as its
// fallback, so a bare `dotnet build` agrees with what Cake passes. The base
// below and _GtkSharpBaseVersion there have to move together.
const string baseVersion = "4.22.4";

static string DateVersion()
{
    var today = DateTime.Now;
    return $"{baseVersion}.{today:yy}{today.DayOfYear:000}";
}

Settings.Version = Argument("BuildVersion", DateVersion());
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");
var configuration = Argument("Configuration", "Release");

var msbuildsettings = new DotNetMSBuildSettings();
var list = new List<GAssembly>();
var supportedVersionBands = new List<string>() {"10.0.100", "10.0.200", "10.0.300", "10.0.400"};

// TASKS

Task("Init")
    .Does(() =>
{
    if (!string.IsNullOrEmpty(EnvironmentVariable("GITHUB_ACTIONS")))
    {
        // The date is the version on CI too, so a package built here and one
        // built locally on the same day are the same version.
        //
        // Note what that means for the push step: the date does not change
        // within a day, so a second publish on the same day is a duplicate and
        // the feed will reject it. Append the run number here if that becomes a
        // problem.
        Settings.Version = DateVersion();

        if (EnvironmentVariable("GITHUB_REF") != "refs/heads/gtk4")
            Settings.Version += "-develop";
    }

    Console.WriteLine("Version: " + Settings.Version);

    // Assign some common properties
    msbuildsettings = msbuildsettings.WithProperty("Version", Settings.Version);
    msbuildsettings = msbuildsettings.WithProperty("Authors", "'GtkSharp Contributors'");
    msbuildsettings = msbuildsettings.WithProperty("PackageLicenseUrl", "'https://github.com/GtkSharp/GtkSharp/blob/cakecore/LICENSE'");

    // Add stuff to list
    Settings.Init();
    foreach(var gassembly in Settings.AssemblyList)
        if(string.IsNullOrEmpty(Settings.Assembly) || Settings.Assembly == gassembly.Name)
            list.Add(gassembly);
});

Task("Prepare")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // Build tools
    DotNetRestore("Source/Tools/Tools.sln");
    DotNetBuild("Source/Tools/Tools.sln", new DotNetBuildSettings {
        Verbosity = DotNetVerbosity.Minimal,
        Configuration = configuration
    });

    // Generate code and prepare libs projects
    foreach(var gassembly in list)
        gassembly.Prepare();
    DotNetRestore("Source/GtkSharp.sln");

    // Addin
    DotNetRestore("Source/Addins/MonoDevelop.GtkSharp.Addin/MonoDevelop.GtkSharp.Addin.sln");
});

Task("Clean")
    .IsDependentOn("Init")
    .Does(() =>
{
    foreach(var gassembly in list)
        gassembly.Clean();
});

// Rewrites the checked-in Source/Libs/<Name>/<Name>-api.xml from the vendored
// .gir files in Source/Gir. Deliberately NOT in the Default chain: a normal
// build stays hermetic and offline, and regenerating an api.xml stays an
// explicit, reviewable, committed act.
Task("RegenerateApi")
    .IsDependentOn("Init")
    .Does(() =>
{
    DotNetRestore("Source/Tools/Tools.sln");
    DotNetBuild("Source/Tools/Tools.sln", new DotNetBuildSettings {
        Verbosity = DotNetVerbosity.Minimal,
        Configuration = configuration
    });

    foreach(var gassembly in list)
        gassembly.RegenerateApi();
});

Task("FullClean")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DeleteDirectory("BuildOutput", new DeleteDirectorySettings {
        Recursive = true,
        Force = true
    });
});

Task("Build")
    .IsDependentOn("Prepare")
    .Does(() =>
{
    var settings = new DotNetBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = msbuildsettings
    };

    if (list.Count == Settings.AssemblyList.Count)
        DotNetBuild("Source/GtkSharp.sln", settings);
    else
    {
        foreach(var gassembly in list)
            DotNetBuild(gassembly.Csproj, settings);
    }
});

Task("RunSamples")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = msbuildsettings
    };

    DotNetBuild("Source/Samples/Samples.csproj", settings);
    DotNetRun("Source/Samples/Samples.csproj");
});

Task("PackageNuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetPackSettings
    {
        MSBuildSettings = msbuildsettings,
        Configuration = configuration,
        OutputDirectory = "BuildOutput/NugetPackages",
        NoBuild = true
    };

    foreach(var gassembly in list)
        DotNetPack(gassembly.Csproj, settings);
});

Task("PackageWorkload")
    .IsDependentOn("Build")
    .Does(() =>
{
    var packSettings = new DotNetPackSettings
    {
        MSBuildSettings = msbuildsettings,
        Configuration = configuration,
        OutputDirectory = "BuildOutput/NugetPackages",
        // Some of the nugets here depend on output generated during build.
        NoBuild = false
    };

    DotNetPack("Source/Workload/GtkSharp.Workload.Template.CSharp/GtkSharp.Workload.Template.CSharp.csproj", packSettings);
    DotNetPack("Source/Workload/GtkSharp.Workload.Template.FSharp/GtkSharp.Workload.Template.FSharp.csproj", packSettings);
    DotNetPack("Source/Workload/GtkSharp.Workload.Template.VBNet/GtkSharp.Workload.Template.VBNet.csproj", packSettings);
    DotNetPack("Source/Workload/GtkSharp.Ref/GtkSharp.Ref.csproj", packSettings);
    DotNetPack("Source/Workload/GtkSharp.Runtime/GtkSharp.Runtime.csproj", packSettings);
    DotNetPack("Source/Workload/GtkSharp.Sdk/GtkSharp.Sdk.csproj", packSettings);

    foreach (var band in supportedVersionBands)
    {
        packSettings.MSBuildSettings = packSettings.MSBuildSettings.WithProperty("_GtkSharpManifestVersionBand", band);
        DotNetPack("Source/Workload/GtkSharp.NET.Sdk.Gtk/GtkSharp.NET.Sdk.Gtk.csproj", packSettings);
    }
});

Task("PackageTemplates")
    .IsDependentOn("Init")
    .Does(() =>
{
    var settings = new DotNetPackSettings
    {
        MSBuildSettings = msbuildsettings,
        Configuration = configuration,
        OutputDirectory = "BuildOutput/NugetPackages"
    };

    DotNetPack("Source/Templates/GtkSharp.Template.CSharp/GtkSharp.Template.CSharp.csproj", settings);
    DotNetPack("Source/Templates/GtkSharp.Template.FSharp/GtkSharp.Template.FSharp.csproj", settings);
    DotNetPack("Source/Templates/GtkSharp.Template.VBNet/GtkSharp.Template.VBNet.csproj", settings);
});

const string manifestName = "GtkSharp.NET.Sdk.Gtk";
var manifestPack = $"{manifestName}.Manifest-{TargetEnvironment.DotNetCliFeatureBand}.{Settings.Version}.nupkg";
var manifestPackPath = $"BuildOutput/NugetPackages/{manifestPack}";

// These are package ids on the feed and the pack ids the workload manifest
// resolves by name, so they match PackageId in Source/Workload/Shared/Common.targets
// and the entries in WorkloadManifest.in.json.
var packNames = new List<string>()
{
    "GtkSharp.Ref",
    "GtkSharp.Runtime",
    "GtkSharp.Sdk"
};

var templateLanguages = new List<string>()
{
    "CSharp",
    "FSharp",
    "VBNet"
};

Task("InstallWorkload")
    .IsDependentOn("PackageWorkload")
    .IsDependentOn("PackageTemplates")
    .Does(() =>
{
    Console.WriteLine($"Installing workload for SDK version {TargetEnvironment.DotNetCliFeatureBand}, at {TargetEnvironment.DotNetInstallPath}");
    Console.WriteLine($"Installing manifests to {TargetEnvironment.DotNetManifestPath}");
    TargetEnvironment.InstallManifests(manifestName, manifestPackPath);
    Console.WriteLine($"Installing packs to {TargetEnvironment.DotNetPacksPath}");
    foreach (var name in packNames)
    {
        Console.WriteLine($"Installing {name}");
        var pack = $"{name}.{Settings.Version}.nupkg";
        var packPath = $"BuildOutput/NugetPackages/{pack}";
        TargetEnvironment.InstallPack(name, Settings.Version, packPath);
    }
    Console.WriteLine($"Installing templates to {TargetEnvironment.DotNetTemplatePacksPath}");
    foreach (var language in templateLanguages)
    {
        Console.WriteLine($"Installing {language} templates");
        var pack = $"GtkSharp.Workload.Template.{language}.{Settings.Version}.nupkg";
        var packPath = $"BuildOutput/NugetPackages/{pack}";
        TargetEnvironment.InstallTemplatePack(pack, packPath);
    }
    Console.WriteLine($"Registering \"gtk\" installed workload...");
    TargetEnvironment.RegisterInstalledWorkload("gtk");
});

Task("UninstallWorkload")
    .Does(() =>
{
    Console.WriteLine($"Uninstalling workload for SDK version {TargetEnvironment.DotNetCliFeatureBand}, at {TargetEnvironment.DotNetInstallPath}");
    Console.WriteLine($"Removing manifests from {TargetEnvironment.DotNetManifestPath}");
    TargetEnvironment.UninstallManifests(manifestName);
    Console.WriteLine($"Removing packs from {TargetEnvironment.DotNetPacksPath}");
    foreach (var name in packNames)
    {
        Console.WriteLine($"Removing {name}");
        TargetEnvironment.UninstallPack(name, Settings.Version);
    }
    Console.WriteLine($"Removing templates from {TargetEnvironment.DotNetTemplatePacksPath}");
    foreach (var language in templateLanguages)
    {
        Console.WriteLine($"Removing {language} templates");
        var pack = $"GtkSharp.Workload.Template.{language}.{Settings.Version}.nupkg";
        TargetEnvironment.UninstallTemplatePack(pack);
    }
    Console.WriteLine($"Unregistering \"gtk\" installed workload...");
    TargetEnvironment.UnregisterInstalledWorkload("gtk");
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("PackageNuGet")
    .IsDependentOn("PackageWorkload")
	.IsDependentOn("PackageTemplates");

// EXECUTION

RunTarget(Settings.BuildTarget);
