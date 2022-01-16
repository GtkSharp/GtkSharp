using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

class GLibrary
{

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetDllDirectory(string lpPathName);

	private static Dictionary<Library, IntPtr> _libraries;
	private static HashSet<Library> _librariesNotFound;
	private static Dictionary<string, IntPtr> _customlibraries;
	private static Dictionary<Library, string[]> _libraryDefinitions;

	static GLibrary()
	{
		_customlibraries = new Dictionary<string, IntPtr>();
		_librariesNotFound = new HashSet<Library>();
		_libraries = new Dictionary<Library, IntPtr>();
		_libraryDefinitions = new Dictionary<Library, string[]>();
		_libraryDefinitions[Library.GLib] = new[] {"libglib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib", "glib-2.dll"};
		_libraryDefinitions[Library.GObject] = new[] {"libgobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib", "gobject-2.dll"};
		_libraryDefinitions[Library.Cairo] = new[] {"libcairo-2.dll", "libcairo.so.2", "libcairo.2.dylib", "cairo.dll"};
		_libraryDefinitions[Library.Gio] = new[] {"libgio-2.0-0.dll", "libgio-2.0.so.0", "libgio-2.0.0.dylib", "gio-2.dll"};
		_libraryDefinitions[Library.Atk] = new[] {"libatk-1.0-0.dll", "libatk-1.0.so.0", "libatk-1.0.0.dylib", "atk-1.dll"};
		_libraryDefinitions[Library.Pango] = new[] {"libpango-1.0-0.dll", "libpango-1.0.so.0", "libpango-1.0.0.dylib", "pango-1.dll"};
		_libraryDefinitions[Library.Gdk] = new[] {"libgdk-3-0.dll", "libgdk-3.so.0", "libgdk-3.0.dylib", "gdk-3.dll"};
		_libraryDefinitions[Library.GdkPixbuf] = new[] {"libgdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib", "gdk_pixbuf-2.dll"};
		_libraryDefinitions[Library.Gtk] = new[] {"libgtk-3-0.dll", "libgtk-3.so.0", "libgtk-3.0.dylib", "gtk-3.dll"};
		_libraryDefinitions[Library.PangoCairo] = new[] {"libpangocairo-1.0-0.dll", "libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib", "pangocairo-1.dll"};
		_libraryDefinitions[Library.GtkSource] = new[] {"libgtksourceview-4-0.dll", "libgtksourceview-4.so.0", "libgtksourceview-4.0.dylib", "gtksourceview-4.dll"};
        _libraryDefinitions[Library.Webkit] = new[] { "libwebkit2gtk-4.0.dll", "libwebkit2gtk-4.0.so.37", "libwebkit2gtk-4.0.dylib", "libwebkit2gtk-4.0.0.dll" };
	}

	public static IntPtr Load(Library library)
	{
		if (_libraries.TryGetValue(library, out var ret))
			return ret;

		if (TryGet(library, out ret)) return ret;

		var err = library + ": " + string.Join(", ", _libraryDefinitions[library]);

		throw new DllNotFoundException(err);

	}

	public static bool IsSupported(Library library) => TryGet(library, out var __);

	static bool TryGet(Library library, out IntPtr ret)
	{
		ret = IntPtr.Zero;

		if (_libraries.TryGetValue(library, out ret)) {
			return true;
		}

		if (_librariesNotFound.Contains(library)) {
			return false;
		}

		if (FuncLoader.IsWindows) {
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][0]);

			if (ret == IntPtr.Zero) {
				SetDllDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gtk", "3.24.24"));
				ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][0]);
			}
		} else if (FuncLoader.IsOSX) {
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][2]);

			if (ret == IntPtr.Zero) {
				ret = FuncLoader.LoadLibrary("/usr/local/lib/" + _libraryDefinitions[library][2]);
			}
		} else
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][1]);

		if (ret == IntPtr.Zero) {
			for (var i = 0; i < _libraryDefinitions[library].Length; i++) {
				ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][i]);

				if (ret != IntPtr.Zero)
					break;
			}
		}

		if (ret != IntPtr.Zero) {
			_libraries[library] = ret;
		} else {
			_librariesNotFound.Add(library);
		}

		return ret != IntPtr.Zero;
	}

}