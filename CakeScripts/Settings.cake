
class Settings
{
    public static ICakeContext Cake { get; set; }
    public static string Version { get; set; }
    public static string BuildTarget { get; set; }
    public static string Assembly { get; set; }
    public static List<GAssembly> AssemblyList { get; set; }
    
    public static void Init()
    {
        AssemblyList = new List<GAssembly>()
        {
            new GAssembly("GLibSharp"),
            new GAssembly("GioSharp")
            {
                Deps = new[] { "GLibSharp" },
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
            },
            new GAssembly("GtkSourceSharp")
            {
                Deps = new[] { "GLibSharp", "GtkSharp", "GioSharp", "CairoSharp", "PangoSharp", "GdkSharp" },
            }
        };
    }
}