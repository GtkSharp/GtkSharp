// TypeRegistry.cs - cross-namespace GIR type resolution.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion.Gir {

	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;

	public enum GirKind {
		Unknown,
		Class,
		Interface,
		Record,
		Union,
		Enumeration,
		Bitfield,
		Callback,
		Alias,
	}

	public class GirTypeInfo {
		public string QualifiedName;   // "Gtk.Widget"
		public string CType;           // "GtkWidget"
		public GirKind Kind;
		public XElement Element;
		public GirDocument Document;
	}

	/// <summary>
	/// Resolves GIR type references to C type names across the main .gir and
	/// every --include'd one.
	/// </summary>
	/// <remarks>
	/// gapi type strings are C type strings — SymbolTable.cs keys off exactly
	/// those — so every &lt;type name="…"&gt; must be resolvable to a c:type even
	/// when the reference itself omits one, which GIR commonly does inside
	/// &lt;array&gt;.
	/// </remarks>
	public class TypeRegistry {

		readonly Dictionary<string, GirTypeInfo> byQualifiedName =
			new Dictionary<string, GirTypeInfo> ();

		public void Add (GirDocument doc)
		{
			var nsName = doc.Name;

			foreach (var el in doc.Namespace.Elements ()) {
				var kind = KindOf (el.Name);
				if (kind == GirKind.Unknown)
					continue;

				var name = (string) el.Attribute ("name");
				if (name == null)
					continue;

				var ctype = (string) el.Attribute (Ns.CType)
					?? (string) el.Attribute (Ns.GlibTypeName)
					?? doc.IdentifierPrefix + name;

				byQualifiedName [nsName + "." + name] = new GirTypeInfo {
					QualifiedName = nsName + "." + name,
					CType = ctype,
					Kind = kind,
					Element = el,
					Document = doc,
				};
			}
		}

		static GirKind KindOf (XName name)
		{
			if (name.Namespace != Ns.Core)
				return GirKind.Unknown;

			switch (name.LocalName) {
			case "class": return GirKind.Class;
			case "interface": return GirKind.Interface;
			case "record": return GirKind.Record;
			case "union": return GirKind.Union;
			case "enumeration": return GirKind.Enumeration;
			case "bitfield": return GirKind.Bitfield;
			case "callback": return GirKind.Callback;
			case "alias": return GirKind.Alias;
			default: return GirKind.Unknown;
			}
		}

		/// <summary>
		/// Look a GIR type name up. Bare names resolve against
		/// <paramref name="currentNamespace"/> first, then as already-qualified.
		/// </summary>
		public GirTypeInfo Resolve (string girName, string currentNamespace)
		{
			if (string.IsNullOrEmpty (girName))
				return null;

			GirTypeInfo info;

			if (!girName.Contains ('.')) {
				if (byQualifiedName.TryGetValue (currentNamespace + "." + girName, out info))
					return info;
			}

			return byQualifiedName.TryGetValue (girName, out info) ? info : null;
		}

		public IEnumerable<GirTypeInfo> All {
			get { return byQualifiedName.Values; }
		}

		/// <summary>
		/// The gtype-struct records, indexed by the type they belong to, so an
		/// object emitter can find its own class struct.
		/// </summary>
		public Dictionary<string, XElement> GTypeStructsFor (GirDocument doc)
		{
			return doc.Namespace.Elements (Ns.Core + "record")
				.Where (r => r.Attribute (Ns.GlibIsGTypeStructFor) != null)
				.GroupBy (r => (string) r.Attribute (Ns.GlibIsGTypeStructFor))
				.ToDictionary (g => g.Key, g => g.First ());
		}
	}
}
