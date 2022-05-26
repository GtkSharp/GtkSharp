#load CakeScripts\GAssembly.cake
#load CakeScripts\Settings.cake
#addin "Cake.FileHelpers&version=5.0.0"
#addin "Cake.Incubator&version=7.0.0"

// VARS

Settings.Cake = Context;
Settings.Version = Argument("BuildVersion", "3.24.24.1");
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");
var configuration = Argument("Configuration", "Release");

var msbuildsettings = new DotNetMSBuildSettings();
var list = new List<GAssembly>();
var supportedVersionBands = new List<string>() {"6.0.100", "6.0.200", "6.0.300"};

// TASKS

Task("Init")
    .Does(() =>
{
    if (!string.IsNullOrEmpty(EnvironmentVariable("GITHUB_ACTIONS")))
    {
        Settings.Version = "3.24.24." + EnvironmentVariable("GITHUB_RUN_NUMBER");

        if (EnvironmentVariable("GITHUB_REF") != "refs/heads/master")
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

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("PackageNuGet")
    .IsDependentOn("PackageWorkload")
	.IsDependentOn("PackageTemplates");

// EXECUTION

RunTarget(Settings.BuildTarget);
