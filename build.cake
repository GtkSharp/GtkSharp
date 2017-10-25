#load CakeScripts\GAssembly.cs
#load CakeScripts\GapiFixup.cs
#load CakeScripts\Settings.cs
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"

// VARS

Settings.Cake = Context;
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");

var configuration = Argument("Configuration", "Release");
var list = new List<GAssembly>();
var glist = new List<GAssembly>()
{
    new GAssembly("GLibSharp"),
    new GAssembly("GioSharp")
    {
        Deps = new[] { "GLibSharp" }
    },
    new GAssembly("AtkSharp")
    {
        Deps = new[] { "GLibSharp" },
        ExtraArgs = "--abi-cs-usings=Atk,GLib"
    },
    new GAssembly("CairoSharp"),
    new GAssembly("PangoSharp")
    {
        Deps = new[] { "GLibSharp", "CairoSharp" }
    },
    new GAssembly("GdkSharp")
    {
        Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp" }
    },
    new GAssembly("GtkSharp")
    {
        Deps = new[] { "GLibSharp", "GioSharp", "AtkSharp", "CairoSharp", "PangoSharp", "GdkSharp" },
        ExtraArgs = "--abi-cs-usings=Gtk,GLib"
    }
};

// TASKS

Task("Init")
    .Does(() =>
{
    // Add stuff to list
    foreach(var gassembly in glist)
        if(string.IsNullOrEmpty(Settings.Assembly) || Settings.Assembly == gassembly.Name)
            list.Add(gassembly);
});

Task("Prepare")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // Build Tools
    DotNetCoreRestore("Source/Tools/GapiCodegen/GapiCodegen.csproj");
    MSBuild("Source/Tools/GapiCodegen/GapiCodegen.csproj", new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        Configuration = "Release",
    });

    // Generate code and prepare libs projects
    foreach(var gassembly in list)
        gassembly.Prepare();
    DotNetCoreRestore("Source/Libs/GLibSharp.sln");
});

Task("Test")
    .Does(() => 
{

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
    if (list.Count == glist.Count)
    {
        MSBuild("Source/Libs/GLibSharp.sln", new MSBuildSettings {
            Verbosity = Verbosity.Minimal,
            Configuration = "Release",
        });
    }
    else
    {
        foreach(var gassembly in list)
        {
            MSBuild(gassembly.Csproj, new MSBuildSettings {
                Verbosity = Verbosity.Minimal,
                Configuration = "Release",
            });
        }
    }
});

Task("PackageNuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        OutputDirectory = "BuildOutput/NugetPackages",
        NoBuild = true
    };

    foreach(var gassembly in list)
        DotNetCorePack(gassembly.Csproj, settings);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build");

// EXECUTION

RunTarget(Settings.BuildTarget);
