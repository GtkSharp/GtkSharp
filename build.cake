#load CakeScripts\GAssembly.cs
#load CakeScripts\Settings.cs
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"

// VARS

Settings.Cake = Context;
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");

var msbuildsettings = new DotNetCoreMSBuildSettings();
var list = new List<GAssembly>();
var glist = new List<GAssembly>()
{
    new GAssembly("GLibSharp")
    {
        NativeDeps = new[] {
            "libglib-2.0.so.0", "libglib-2.0-0",
            "libgobject-2.0.so.0", "libgobject-2.0-0",
            "libgthread-2.0.so.0", "libgthread-2.0-0"
        }
    },
    new GAssembly("GioSharp")
    {
        Deps = new[] { "GLibSharp" },
        NativeDeps = new[] { "libgio-2.0.so.0", "libgio-2.0-0" }
    },
    new GAssembly("AtkSharp")
    {
        Deps = new[] { "GLibSharp" },
        NativeDeps = new[] { "libatk-1.0.so.0", "libatk-1.0-0" },
        ExtraArgs = "--abi-cs-usings=Atk,GLib"
    },
    new GAssembly("CairoSharp")
    {
        NativeDeps = new[] { "libcairo.so.2", "libcairo-2" }
    },
    new GAssembly("PangoSharp")
    {
        Deps = new[] { "GLibSharp", "CairoSharp" },
        NativeDeps = new[] { "libpango-1.0.so.0", "libpango-1.0-0" }
    },
    new GAssembly("GdkSharp")
    {
        Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp" },
        NativeDeps = new[] {
            "libgdk-3.so.0", "libgdk-3-0",
            "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0-0"
        }
    },
    new GAssembly("GtkSharp")
    {
        Deps = new[] { "GLibSharp", "GioSharp", "AtkSharp", "CairoSharp", "PangoSharp", "GdkSharp" },
        NativeDeps = new[] { "libgtk-3.so.0", "libgtk-3-0" },
        ExtraArgs = "--abi-cs-usings=Gtk,GLib"
    }
};

// TASKS

Task("Init")
    .Does(() =>
{
    // Assign version
    msbuildsettings = msbuildsettings.WithProperty("Version", "3.0.0.0");
    msbuildsettings = msbuildsettings.WithProperty("Authors", "'GLibSharp Team'");

    // Add stuff to list
    foreach(var gassembly in glist)
        if(string.IsNullOrEmpty(Settings.Assembly) || Settings.Assembly == gassembly.Name)
            list.Add(gassembly);
});

Task("Prepare")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // Build tools
    DotNetCoreRestore("Source/Tools/Tools.sln");
    MSBuild("Source/Tools/Tools.sln", new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        Configuration = "Release",
    });

    // Generate code and prepare libs projects
    foreach(var gassembly in list)
        gassembly.Prepare();
    DotNetCoreRestore("Source/Libs/GtkSharp.sln");
});

Task("GenerateLinuxStubs")
    .IsDependentOn("Init")
    .Does(() => 
{
    CreateDirectory("BuildOutput/LinuxStubs");
    FileWriteText("BuildOutput/LinuxStubs/empty.c", "");
    foreach(var gassembly in list)
        gassembly.GenerateLinuxStubs();
    DeleteDirectory("BuildOutput/LinuxStubs", new DeleteDirectorySettings { Recursive = true, Force = true });
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
        Configuration = "Release",
        MSBuildSettings = msbuildsettings
    };

    if (list.Count == glist.Count)
        DotNetCoreBuild("Source/Libs/GtkSharp.sln", settings);
    else
    {
        foreach(var gassembly in list)
            DotNetCoreBuild(gassembly.Csproj, settings);
    }
});

Task("PackageNuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        MSBuildSettings = msbuildsettings,
        Configuration = "Release",
        OutputDirectory = "BuildOutput/NugetPackages",
        NoBuild = true,

    };

    foreach(var gassembly in list)
        DotNetCorePack(gassembly.Csproj, settings);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build");

// EXECUTION

RunTarget(Settings.BuildTarget);
