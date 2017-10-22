#load CakeScripts\GAssembly.cs
#load CakeScripts\GapiFixup.cs
#addin "Cake.FileHelpers"

// VARS

GAssembly.Cake = Context;

var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");
var glist = new List<GAssembly>()
{
    new GAssembly("GLibSharp"),
    new GAssembly("GioSharp")
    {
        Includes = new[] { "GLibSharp" }
    },
    new GAssembly("AtkSharp")
    {
        Includes = new[] { "GLibSharp" },
        ExtraArgs = "--abi-cs-usings=Atk,GLib"
    },
    new GAssembly("CairoSharp"),
    new GAssembly("PangoSharp")
    {
        Includes = new[] { "GLibSharp", "CairoSharp" }
    },
    new GAssembly("GdkSharp")
    {
        Includes = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp" }
    },
    new GAssembly("GtkSharp")
    {
        Includes = new[] { "GLibSharp", "GioSharp", "AtkSharp", "CairoSharp", "PangoSharp", "GdkSharp" },
        ExtraArgs = "--abi-cs-usings=Gtk,GLib"
    }
};

// TASKS

Task("Prepare")
    .Does(() =>
{
    MSBuild("Source/Tools/GapiCodegen/GapiCodegen.csproj", new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        Configuration = "Release",
    });

    foreach(var gassembly in glist)
        gassembly.Prepare();
});

Task("Clean")
    .Does(() =>
{
    foreach(var gassembly in glist)
        gassembly.Clean();
});

Task("Build")
    .IsDependentOn("Prepare")
    .Does(() =>
{
    foreach(var gassembly in glist)
    {
        MSBuild(gassembly.Csproj, new MSBuildSettings {
            Verbosity = Verbosity.Minimal,
            Configuration = "Release",
        });
    }
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build");

// EXECUTION

RunTarget(target);
