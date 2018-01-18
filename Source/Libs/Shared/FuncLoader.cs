using System;
using System.Runtime.InteropServices;

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
            throw new EntryPointNotFoundException(function);

        return ret;
    }
}
