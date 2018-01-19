using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class GLibrary
{
    private static Dictionary<Library, IntPtr> _libraries;
    private static Dictionary<string, IntPtr> _customlibraries;
    private static List<(Library Library, string WindowsLib, string LinuxLib, string OSXLib)> _libdict;

    static GLibrary()
    {
        _customlibraries = new Dictionary<string, IntPtr>();
        _libraries = new Dictionary<Library, IntPtr>();
        _libdict = new List<(Library, string, string, string)>();
        _libdict.Add((Library.GLib, "libglib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib"));
        _libdict.Add((Library.GObject, "libgobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib"));
        _libdict.Add((Library.Cairo, "libcairo-2.dll", "libcairo.so.2", "libcairo.2.dylib"));
        _libdict.Add((Library.Gio, "libgio-2.0-0.dll", "libgio-2.0.so.0", "libgio-2.0.0.dylib"));
        _libdict.Add((Library.Atk, "libatk-1.0-0.dll", "libatk-1.0.so.0", "libatk-1.0.0.dylib"));
        _libdict.Add((Library.Pango, "libpango-1.0-0.dll", "libpango-1.0.so.0", "libpango-1.0.0.dylib"));
        _libdict.Add((Library.Gdk, "libgdk-3-0.dll", "libgdk-3.so.0", "libgdk-3.0.dylib"));
        _libdict.Add((Library.GdkPixbuf, "libgdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib"));
        _libdict.Add((Library.Gtk, "libgtk-3-0.dll", "libgtk-3.so.0", "libgtk-3.0.dylib"));
        _libdict.Add((Library.PangoCairo, "libpangocairo-1.0-0.dll", "libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib"));
    }

    public static IntPtr Load(string libname)
    {
        var index = _libdict.FindIndex((e) => (e.WindowsLib == libname || e.LinuxLib == libname || e.OSXLib == libname));

        if (index != -1)
            return Load(_libdict[index].Library);

        var ret = IntPtr.Zero;
        if (!_customlibraries.TryGetValue(libname, out ret))
            _customlibraries[libname] = ret = FuncLoader.LoadLibrary(libname);

        return ret;
    }

    public static IntPtr Load(Library library)
    {
        IntPtr ret = IntPtr.Zero;
        if (!_libraries.TryGetValue(library, out ret))
        {
            var i = _libdict.Find((e) => e.Library == library);
            var s = i.LinuxLib;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                s = i.WindowsLib;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                s = i.OSXLib;

            _libraries[library] = ret = FuncLoader.LoadLibrary(s);
        }

        if (ret == IntPtr.Zero)
            throw new DllNotFoundException(library.ToString());

        return ret;
    }
}