using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

enum Library
{
    GLib,
    GObject,
    Cairo,
    Gio,
    Atk,
    Pango,
    Gdk,
    GdkPixbuf,
    Gtk
}

class GLibrary
{
    private static Dictionary<Library, IntPtr> _libraries;
    private static List<(Library Library, string WindowsLib, string LinuxLib, string OSXLib)> _libdict;
    private static IntPtr _temp = FuncLoader.LoadLibrary("libgtk-3.so.0");

    static GLibrary()
    {
        _libraries = new Dictionary<Library, IntPtr>();
        _libdict = new List<(Library, string, string, string)>();
        _libdict.Add((Library.GLib, "libglib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib"));
        _libdict.Add((Library.GObject, "libgobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib"));
        _libdict.Add((Library.Cairo, "libcairo-2.dll", "libcairo.so.2", "libcairo.2.dylib"));
        _libdict.Add((Library.Gio, "libgio-2.0-0.dll", "libgio-2.0.so.0", "libgio-2.0.0.dylib"));
        _libdict.Add((Library.Atk, "libatk-1.0-0.dll", "libatk-1.0.so.0", "libatk-1.0.0.dylib"));
        _libdict.Add((Library.Pango, "libpango-1.0-0.dll", "libpango-1.0.so.0", "libpango-1.0.0.dylib"));
        _libdict.Add((Library.Gdk, "libgdk-3-0.dll", "libgdk-3.so.0", "libgdk-quartz-3.0.0.dylib"));
        _libdict.Add((Library.GdkPixbuf, "libgdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib"));
        _libdict.Add((Library.Gtk, "libgtk-3-0.dll", "libgtk-3.so.0", "libgtk-quartz-3.0.0.dylib"));
    }

    public static IntPtr Load(string libname)
    {
        var index = _libdict.FindIndex((e) => (e.WindowsLib == libname || e.LinuxLib == libname || e.OSXLib == libname));

        if (index != -1)
            return Load(_libdict[index].Library);

        return _temp;
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

            // Console.WriteLine(s);
            _libraries[library] = ret = FuncLoader.LoadLibrary(s);
        }

        if (ret == IntPtr.Zero)
        {
            Console.WriteLine("Error: " + library);
            throw new Exception();
        }

        return ret;
    }
}

class FuncLoader
{
    private class Windows
    {
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(string lpszLib);
    }

    private class Linux
    {
        [DllImport("libdl.so.2")]
        public static extern IntPtr dlopen(string path, int flags);

        [DllImport("libdl.so.2")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
    }

    private class OSX
    {
        [DllImport("/usr/lib/libSystem.dylib")]
        public static extern IntPtr dlopen(string path, int flags);

        [DllImport("/usr/lib/libSystem.dylib")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);
    }

    private static bool IsWindows, IsOSX;

    static FuncLoader()
    {
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    }

    public static IntPtr LoadLibrary(string libname)
    {
        if (IsWindows)
            return Windows.LoadLibraryW(libname);

        if (IsOSX)
            return OSX.dlopen(libname, 1);

        return Linux.dlopen(libname, 1);
    }

    public static IntPtr GetProcAddress(IntPtr library, string function)
    {
        var ret = IntPtr.Zero;

        if (IsWindows)
            ret = Windows.GetProcAddress(library, function);
        else if (IsOSX)
            ret = OSX.dlsym(library, function);
        else
            ret = Linux.dlsym(library, function);

        if (ret == IntPtr.Zero)
        {
            Console.WriteLine("Error: " + function);
            throw new Exception();
        }

        return ret;
    }
}