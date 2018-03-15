
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
            new GAssembly("GLibSharp")
            {
                NativeDeps = new[] { "libglib-2.0-0.dll", "libgobject-2.0-0.dll", "libgthread-2.0-0.dll" }
            },
            new GAssembly("GioSharp")
            {
                Deps = new[] { "GLibSharp" },
                NativeDeps = new[] { "libgio-2.0-0.dll" }
            },
            new GAssembly("AtkSharp")
            {
                Deps = new[] { "GLibSharp" },
                NativeDeps = new[] { "libatk-1.0-0.dll" },
                ExtraArgs = "--abi-cs-usings=Atk,GLib"
            },
            new GAssembly("CairoSharp")
            {
                NativeDeps = new[] { "libcairo-2.dll" }
            },
            new GAssembly("PangoSharp")
            {
                Deps = new[] { "GLibSharp", "CairoSharp" },
                NativeDeps = new[] { "libpango-1.0-0.dll" }
            },
            new GAssembly("GdkSharp")
            {
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp" },
                NativeDeps = new[] { "libgdk-3-0.dll", "libgdk_pixbuf-2.0-0.dll" }
            },
            new GAssembly("GtkSharp")
            {
                Deps = new[] { "GLibSharp", "GioSharp", "AtkSharp", "CairoSharp", "PangoSharp", "GdkSharp" },
                NativeDeps = new[] { "libgtk-3-0.dll" },
                ExtraArgs = "--abi-cs-usings=Gtk,GLib"
            }
        };
    }
}