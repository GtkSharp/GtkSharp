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
    DotNetPack("Source/Templates/GtkSharp.Template.CSharp/GtkSharp.Template.FSharp.csproj", settings);
    DotNetPack("Source/Templates/GtkSharp.Template.CSharp/GtkSharp.Template.VBNet.csproj", settings);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageTemplates");

// EXECUTION

RunTarget(Settings.BuildTarget);
