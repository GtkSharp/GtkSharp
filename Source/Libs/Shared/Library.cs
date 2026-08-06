
// The native libraries symbols are looked up in. These name logical symbol
// sources, not one file each: under Gtk 4 the Gdk and Gsk entries both resolve
// to libgtk-4, because GDK and GSK are compiled into it rather than shipped as
// separate shared objects. Keeping them as distinct enum values means generated
// gdk_*/gsk_* code needs no redirection and reads honestly about where the
// symbol comes from. See GLibrary for the per-platform file names.
enum Library
{
    GLib,
    GObject,
    Cairo,
    Gio,
    Pango,
    PangoCairo,
    Graphene,
    GdkPixbuf,
    Gdk,
    Gsk,
    Gtk,
    GtkSource,
    Webkit,
    JavaScriptCore,
    Adwaita
}
