#load CakeScripts\GAssembly.cs
#load CakeScripts\Settings.cs
#addin "Cake.FileHelpers&version=2.0.0"
#addin "Cake.Incubator&version=1.7.2"

// VARS

Settings.Cake = Context;
Settings.Version = "3.22.24.30";
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");
var configuration = Argument("Configuration", "Release");

var msbuildsettings = new DotNetCoreMSBuildSettings();
var list = new List<GAssembly>();

// TASKS

Task("Init")
    .Does(() =>
{
    var version = System.Environment.GetEnvironmentVariable("TRAVIS_TAG");
    if (!string.IsNullOrEmpty(version))
        Settings.Version = version;
	
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
    DeleteDirectory("BuildOutput", true);
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

Task("PackageAddin")
    .IsDependentOn("PackageTemplates")
    .Does(() =>
{
    // Copy the current version nuget templates
    CopyFile(
        "BuildOutput/NugetPackages/GtkSharp.Template.CSharp." + Settings.Version + ".nupkg",
        "Source/Addins/MonoDevelop.GtkSharp.Addin/Templates/GtkSharp.Template.CSharp.nupkg"
    );
    CopyFile(
        "BuildOutput/NugetPackages/GtkSharp.Template.FSharp." + Settings.Version + ".nupkg",
        "Source/Addins/MonoDevelop.GtkSharp.Addin/Templates/GtkSharp.Template.FSharp.nupkg"
    );

    // Generate version code info
    var versionline = "[assembly: Mono.Addins.Addin(\"MonoDevelop.GtkSharp.Addin\", Version = \"" + Settings.Version + "\")]";
    FileWriteText("Source/Addins/MonoDevelop.GtkSharp.Addin/Properties/AddinInfo.Version.cs", versionline);

    // Build MonoDevelop addin
    var msbuildsettings = new MSBuildSettings
    {
        Configuration = configuration,
    };
    msbuildsettings = msbuildsettings.WithProperty("Version", Settings.Version);
    msbuildsettings = msbuildsettings.WithProperty("MDBinDir", "/opt/MonoDevelop/bin/");
    msbuildsettings = msbuildsettings.WithTarget("PackageAddin");

    // We need monodevelop on the build system to build the addin
    // lets wait for its packaging to be finished.
    // MSBuild("Source/Addins/MonoDevelop.GtkSharp.Addin/MonoDevelop.GtkSharp.Addin.sln", msbuildsettings);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build");
    
Task("FullBuild")
    .IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageTemplates")
	.IsDependentOn("PackageAddin");

// EXECUTION

RunTarget(Settings.BuildTarget);
