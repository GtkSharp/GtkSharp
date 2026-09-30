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
		// Index 0 is the Windows fast path and must match the bundle GtkSharp.targets
		// installs, which is gvsbuild's. gvsbuild builds with MSVC and emits no "lib"
		// prefix, so the prefixed spellings follow as later candidates for MSYS2 and
		// hand-installed runtimes -- the fallback loop below tries every entry.
		_libraryDefinitions[Library.GLib] = new[] {"glib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib", "libglib-2.0-0.dll"};
		_libraryDefinitions[Library.GObject] = new[] {"gobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib", "libgobject-2.0-0.dll"};
		_libraryDefinitions[Library.Cairo] = new[] {"cairo-2.dll", "libcairo.so.2", "libcairo.2.dylib", "libcairo-2.dll"};
		_libraryDefinitions[Library.Gio] = new[] {"gio-2.0-0.dll", "libgio-2.0.so.0", "libgio-2.0.0.dylib", "libgio-2.0-0.dll"};
		_libraryDefinitions[Library.Pango] = new[] {"pango-1.0-0.dll", "libpango-1.0.so.0", "libpango-1.0.0.dylib", "libpango-1.0-0.dll"};
		_libraryDefinitions[Library.PangoCairo] = new[] {"pangocairo-1.0-0.dll", "libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib", "libpangocairo-1.0-0.dll"};
		_libraryDefinitions[Library.Graphene] = new[] {"graphene-1.0-0.dll", "libgraphene-1.0.so.0", "libgraphene-1.0.0.dylib", "libgraphene-1.0-0.dll"};
		_libraryDefinitions[Library.GdkPixbuf] = new[] {"gdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib", "libgdk_pixbuf-2.0-0.dll"};
		// Gtk 4 ships ONE library: gdk_*, gsk_* and gtk_* all resolve out of it.
		// Gdk-4.0.gir and Gsk-4.0.gir both declare shared-library="libgtk-4.so.1",
		// and there is no libgdk-4 or libgsk-4 to find.
		_libraryDefinitions[Library.Gdk] = new[] {"gtk-4-1.dll", "libgtk-4.so.1", "libgtk-4.1.dylib", "libgtk-4-1.dll"};
		_libraryDefinitions[Library.Gsk] = new[] {"gtk-4-1.dll", "libgtk-4.so.1", "libgtk-4.1.dylib", "libgtk-4-1.dll"};
		_libraryDefinitions[Library.Gtk] = new[] {"gtk-4-1.dll", "libgtk-4.so.1", "libgtk-4.1.dylib", "libgtk-4-1.dll"};
		_libraryDefinitions[Library.GtkSource] = new[] {"gtksourceview-5-0.dll", "libgtksourceview-5.so.0", "libgtksourceview-5.0.dylib", "libgtksourceview-5-0.dll"};
		_libraryDefinitions[Library.Adwaita] = new[] {"adwaita-1-0.dll", "libadwaita-1.so.0", "libadwaita-1.0.dylib", "libadwaita-1-0.dll"};
		// WebKitGTK 6.0 has no Windows build -- neither gvsbuild nor MSYS2 ships one --
		// so callers must gate on GLibrary.IsSupported(Library.Webkit).
		_libraryDefinitions[Library.Webkit] = new[] {"webkitgtk-6.0-4.dll", "libwebkitgtk-6.0.so.4", "libwebkitgtk-6.0.dylib"};
		// JavaScriptCore ships as its own shared library. The jsc_* symbols were
		// being looked up in the WebKit handle, which cannot find them: a module
		// handle only resolves its own exports, so every one was a null delegate.
		_libraryDefinitions[Library.JavaScriptCore] = new[] {"javascriptcoregtk-6.0-1.dll", "libjavascriptcoregtk-6.0.so.1", "libjavascriptcoregtk-6.0.dylib"};
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
				// The gvsbuild bundle is not flat: everything lives under bin/,
				// unlike the Gtk 3 zip this replaced. Must stay in step with
				// GtkDir in GtkSharp.targets.
				SetDllDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gtk", "4.22.4", "bin"));
				ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][0]);
			}
		} else if (FuncLoader.IsOSX) {
			ret = FuncLoader.LoadLibrary(_libraryDefinitions[library][2]);

			if (ret == IntPtr.Zero) {
				ret = FuncLoader.LoadLibrary("/usr/local/lib/" + _libraryDefinitions[library][2]);
				if (ret == IntPtr.Zero) {
					ret = FuncLoader.LoadLibrary("/opt/homebrew/lib/" + _libraryDefinitions[library][2]);
				}
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