// GirDocument.cs - loading of GObject-Introspection repositories.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion.Gir {

	using System.Linq;
	using System.Xml.Linq;

	/// <summary>The three XML namespaces a .gir file uses.</summary>
	public static class Ns {
		public static readonly XNamespace Core = "http://www.gtk.org/introspection/core/1.0";
		public static readonly XNamespace C = "http://www.gtk.org/introspection/c/1.0";
		public static readonly XNamespace Glib = "http://www.gtk.org/introspection/glib/1.0";

		// c: and glib: attributes, spelled once so call sites stay readable.
		public static readonly XName CType = C + "type";
		public static readonly XName CIdentifier = C + "identifier";
		public static readonly XName CSymbolPrefixes = C + "symbol-prefixes";
		public static readonly XName CIdentifierPrefixes = C + "identifier-prefixes";
		public static readonly XName GlibTypeName = Glib + "type-name";
		public static readonly XName GlibGetType = Glib + "get-type";
		public static readonly XName GlibTypeStruct = Glib + "type-struct";
		public static readonly XName GlibIsGTypeStructFor = Glib + "is-gtype-struct-for";
		public static readonly XName GlibSignal = Glib + "signal";
		public static readonly XName GlibNick = Glib + "nick";
		public static readonly XName GlibFundamental = Glib + "fundamental";
		public static readonly XName GlibRefFunc = Glib + "ref-func";
		public static readonly XName GlibUnrefFunc = Glib + "unref-func";
	}

	/// <summary>A loaded .gir file, reduced to the one namespace it declares.</summary>
	public class GirDocument {

		public string Path { get; private set; }
		public XElement Repository { get; private set; }
		public XElement Namespace { get; private set; }

		/// <summary>Namespace name as GIR spells it, e.g. "Gtk", "GLib".</summary>
		public string Name {
			get { return Namespace.Attribute ("name").Value; }
		}

		/// <summary>
		/// The library the namespace's symbols resolve from. For GTK 4 both
		/// Gdk-4.0.gir and Gsk-4.0.gir report libgtk-4.so.1 here, because GDK
		/// and GSK are compiled into libgtk-4 rather than shipped separately.
		/// </summary>
		public string SharedLibrary {
			get {
				var attr = Namespace.Attribute ("shared-library");
				return attr == null ? null : attr.Value;
			}
		}

		/// <summary>
		/// C function prefixes, e.g. "gtk", or "gio" and "g" for Gio. Used to
		/// group namespace-level functions into gapi &lt;class&gt; elements.
		/// </summary>
		/// <remarks>
		/// More than one is common and the first is not always the one in use:
		/// Gio declares "gio,g" but its functions are all g_content_type_*,
		/// g_io_modules_*, and so on. Taking only the first left every Gio
		/// function ungrouped. Longest first, so that a hypothetical gio_foo_bar
		/// groups under "foo" rather than under "io".
		/// </remarks>
		public string[] SymbolPrefixes {
			get {
				var attr = Namespace.Attribute (Ns.CSymbolPrefixes);
				var values = attr != null
					? attr.Value.Split (',')
					: new[] { Name.ToLowerInvariant () };

				return values.Where (v => v.Length > 0)
					.OrderByDescending (v => v.Length)
					.ToArray ();
			}
		}

		/// <summary>C type prefix, e.g. "Gtk", "G".</summary>
		public string IdentifierPrefix {
			get {
				var attr = Namespace.Attribute (Ns.CIdentifierPrefixes);
				if (attr != null)
					return attr.Value.Split (',') [0];
				return Name;
			}
		}

		public static GirDocument Load (string path)
		{
			var doc = XDocument.Load (path);
			var repo = doc.Root;
			var ns = repo.Elements (Ns.Core + "namespace").FirstOrDefault ();

			if (ns == null)
				throw new GirException (path + ": no <namespace> element");

			return new GirDocument { Path = path, Repository = repo, Namespace = ns };
		}
	}

	public class GirException : System.Exception {
		public GirException (string message) : base (message) { }
	}
}
