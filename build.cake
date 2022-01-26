#load CakeScripts\GAssembly.cake
#load CakeScripts\Settings.cake
#addin "Cake.FileHelpers&version=5.0.0"
#addin "Cake.Incubator&version=7.0.0"
#tool nuget:?package=NuGet.CommandLine&version=6.0.0

// VARS

Settings.Cake = Context;
Settings.Version = Argument("BuildVersion", "3.24.24.1");
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");
var configuration = Argument("Configuration", "Release");

var msbuildsettings = new DotNetCoreMSBuildSettings();
var list = new List<GAssembly>();

private void ParseVersion()
{
    if (!string.IsNullOrEmpty(EnvironmentVariable("GITHUB_ACTIONS")))
    {
        var version = "3.24.24." + EnvironmentVariable("GITHUB_RUN_NUMBER");

        var upstreamUrl = "GtkSharp/GtkSharp";
        var repositoryUrl = EnvironmentVariable("GITHUB_REPOSITORY");
        var branch = EnvironmentVariable("GITHUB_REF");

        if (string.IsNullOrEmpty(repositoryUrl))
            return;

        if (repositoryUrl != upstreamUrl) // If we are building a PR
        {
            var split = repositoryUrl.Split('/');
            version = version + "-" + split[0];
        }
        else if (repositoryUrl == upstreamUrl &&
            !string.IsNullOrEmpty(branch) &&
            branch != " refs/heads/master") // If we are building our repository
        {
            var branchName = branch.Substring(11);
            version = version + "-" + branchName;
        }

        Settings.Version = version;

        Console.WriteLine("Branch: " + branch);
    }

    Console.WriteLine("Version: " + Settings.Version);
}

// TASKS

Task("Init")
    .Does(() =>
{
    ParseVersion();

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
    DotNetCoreRestore("Source/Tools/Tools.sln");
    DotNetCoreBuild("Source/Tools/Tools.sln", new DotNetCoreBuildSettings {
        Verbosity = DotNetCoreVerbosity.Minimal,
        Configuration = configuration
    });

    // Generate code and prepare libs projects
    foreach(var gassembly in list)
        gassembly.Prepare();
    DotNetCoreRestore("Source/GtkSharp.sln");

    // Addin
    DotNetCoreRestore("Source/Addins/MonoDevelop.GtkSharp.Addin/MonoDevelop.GtkSharp.Addin.sln");
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
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = msbuildsettings
    };

    if (list.Count == Settings.AssemblyList.Count)
        DotNetCoreBuild("Source/GtkSharp.sln", settings);
    else
    {
        foreach(var gassembly in list)
            DotNetCoreBuild(gassembly.Csproj, settings);
    }
});

Task("RunSamples")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        MSBuildSettings = msbuildsettings
    };

    DotNetCoreBuild("Source/Samples/Samples.csproj", settings);
    DotNetCoreRun("Source/Samples/Samples.csproj");
});

Task("PackageNuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        MSBuildSettings = msbuildsettings,
        Configuration = configuration,
        OutputDirectory = "BuildOutput/NugetPackages",
        NoBuild = true,

    };

    foreach(var gassembly in list)
        DotNetCorePack(gassembly.Csproj, settings);
});

Task("PackageTemplates")
    .IsDependentOn("Init")
    .Does(() =>
{
    var settings = new NuGetPackSettings
    {
        OutputDirectory = "BuildOutput/NugetPackages",
        Version = Settings.Version
    };

    settings.BasePath = "Source/Templates/GtkSharp.Template.CSharp";
    NuGetPack("Source/Templates/GtkSharp.Template.CSharp/GtkSharp.Template.CSharp.nuspec", settings);

    settings.BasePath = "Source/Templates/GtkSharp.Template.FSharp";
    NuGetPack("Source/Templates/GtkSharp.Template.FSharp/GtkSharp.Template.FSharp.nuspec", settings);

    settings.BasePath = "Source/Templates/GtkSharp.Template.VBNet";
    NuGetPack("Source/Templates/GtkSharp.Template.VBNet/GtkSharp.Template.VBNet.nuspec", settings);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageTemplates");

// EXECUTION

RunTarget(Settings.BuildTarget);
