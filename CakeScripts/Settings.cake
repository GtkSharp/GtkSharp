
class Settings
{
    public static ICakeContext Cake { get; set; }
    public static string Version { get; set; }
    public static string BuildTarget { get; set; }
    public static string Assembly { get; set; }
    public static List<GAssembly> AssemblyList { get; set; }

    // Not a wrapper assembly of its own: GObject types are bound inside
    // GLibSharp, so every converter run needs its gir for type resolution.
    const string GObjectGir = "Source/Gir/GObject-2.0.gir";

    public static void Init()
    {
        // This list is the build order, and the source of the --include flags
        // for both codegen and gir conversion. Deps are listed transitively
        // because both of those uses need the full closure, not just the direct
        // edges.
        AssemblyList = new List<GAssembly>()
        {
            // Hand-written: no .metadata, so nothing is generated here. The gir
            // is still listed so dependents can --include it.
            new GAssembly("GLibSharp")
            {
                Gir = new[] { "Source/Gir/GLib-2.0.gir", GObjectGir },
            },
            new GAssembly("GioSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp" },
                Gir = new[] { "Source/Gir/Gio-2.0.gir" },
            },
            // Also hand-written in full. Cairo has no real introspection -- its
            // gir is a stub of foreign="1" records -- so it is deliberately not
            // vendored and nothing is generated.
            new GAssembly("CairoSharp"),
            new GAssembly("GrapheneSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp" },
                Gir = new[] { "Source/Gir/Graphene-1.0.gir" },
            },
            new GAssembly("PangoSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "CairoSharp" },
                Gir = new[] { "Source/Gir/Pango-1.0.gir", "Source/Gir/PangoCairo-1.0.gir" },
                // Both gir namespaces land in the Pango C# namespace, so group
                // pango_cairo_* under Pango's "cairo" class the way gapi2xml.pl
                // did. That class is renamed CairoHelper by the metadata and is
                // public API the samples call.
                ExtraGirArgs = "--group-prefix=pango",
            },
            // GdkSharp binds two namespaces, Gdk and GdkPixbuf, as it always has.
            new GAssembly("GdkSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GrapheneSharp" },
                Gir = new[] { "Source/Gir/Gdk-4.0.gir", "Source/Gir/GdkPixbuf-2.0.gir" },
            },
            new GAssembly("GskSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GrapheneSharp", "GdkSharp" },
                Gir = new[] { "Source/Gir/Gsk-4.0.gir" },
            },
            new GAssembly("GtkSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GrapheneSharp", "GdkSharp", "GskSharp" },
                Gir = new[] { "Source/Gir/Gtk-4.0.gir" },
                ExtraArgs = "--abi-cs-usings=Gtk,GLib"
            },
            new GAssembly("AdwaitaSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GrapheneSharp", "GdkSharp", "GskSharp", "GtkSharp", "JavaScriptCoreSharp" },
                Gir = new[] { "Source/Gir/Adw-1.gir" },
            },
            new GAssembly("GtkSourceSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GrapheneSharp", "GdkSharp", "GskSharp", "GtkSharp", "JavaScriptCoreSharp" },
                Gir = new[] { "Source/Gir/GtkSource-5.gir" },
            },
            // JavaScriptCore ships as its own shared library, and WebKit's
            // javascript results are JSCValues, so this has to be bound before
            // WebkitGtkSharp can express them as anything but a bare pointer.
            new GAssembly("JavaScriptCoreSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp" },
                Gir = new[] { "Source/Gir/JavaScriptCore-6.0.gir" },
            },
            new GAssembly("WebkitGtkSharp")
            {
                StrictMetadata = true,
                Deps = new[] { "GLibSharp", "GioSharp", "CairoSharp", "PangoSharp", "GrapheneSharp", "GdkSharp", "GskSharp", "GtkSharp", "JavaScriptCoreSharp" },
                Gir = new[] { "Source/Gir/WebKit-6.0.gir" },
                ExtraArgs = "--abi-cs-usings=WebKit,Gtk,GLib,Gdk,Pango,Cairo"
            }
        };
    }
}
