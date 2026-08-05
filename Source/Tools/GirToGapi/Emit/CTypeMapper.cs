// CTypeMapper.cs - GIR type reference -> gapi type string.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion.Emit {

	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using GtkSharp.GirConversion.Gir;

	/// <summary>
	/// Produces the type strings that go into gapi's <c>type=</c> attributes.
	/// </summary>
	/// <remarks>
	/// These are C type strings, not GIR names — GapiCodegen's SymbolTable.cs is
	/// keyed on exactly these spellings, so "const-gchar*" resolves and
	/// "const char *" does not. Emit the c:type verbatim (after the same
	/// normalisation gapi2xml.pl applied) wherever GIR provides one, and
	/// synthesise from &lt;type name=&gt; only where it does not.
	/// </remarks>
	public class CTypeMapper {

		readonly TypeRegistry registry;
		readonly string currentNamespace;

		public CTypeMapper (TypeRegistry registry, string currentNamespace)
		{
			this.registry = registry;
			this.currentNamespace = currentNamespace;
		}

		/// <summary>
		/// GIR primitive names to their C spelling, for references that carry no
		/// c:type. gsize/gssize/glong must land on exactly these strings or the
		/// native-sized integer marshalling handled by LPGen/LPUGen regresses.
		/// </summary>
		static readonly Dictionary<string, string> GirNameToCType = new Dictionary<string, string> {
			{ "none", "void" },
			{ "gboolean", "gboolean" },
			{ "gchar", "gchar" },
			{ "guchar", "guchar" },
			{ "gshort", "gshort" },
			{ "gushort", "gushort" },
			{ "gint", "gint" },
			{ "guint", "guint" },
			{ "glong", "glong" },        // LPGen  - native sized
			{ "gulong", "gulong" },      // LPUGen - native sized
			{ "gint8", "gint8" },
			{ "guint8", "guint8" },
			{ "gint16", "gint16" },
			{ "guint16", "guint16" },
			{ "gint32", "gint32" },
			{ "guint32", "guint32" },
			{ "gint64", "gint64" },
			{ "guint64", "guint64" },
			{ "gsize", "gsize" },        // LPUGen
			{ "gssize", "gssize" },      // LPGen
			{ "gintptr", "gintptr" },
			{ "guintptr", "guintptr" },
			{ "gfloat", "gfloat" },
			{ "gdouble", "gdouble" },
			{ "goffset", "goffset" },
			{ "gunichar", "gunichar" },
			{ "gunichar2", "gunichar2" },
			{ "gpointer", "gpointer" },
			{ "gconstpointer", "gconstpointer" },
			{ "GType", "GType" },
			{ "va_list", "va_list" },
			{ "utf8", "const-gchar*" },
			{ "filename", "const-gchar*" },
		};

		/// <summary>
		/// gapi2xml.pl's parameter type clean-up (addParamsElem), applied in the
		/// same order. The order matters: the "const X* const*" collapse has to
		/// run before "const " becomes "const-", or the result is a spelling
		/// SymbolTable has never heard of.
		/// </summary>
		public static string Normalize (string ctype)
		{
			if (string.IsNullOrEmpty (ctype))
				return ctype;

			var t = ctype.Trim ();

			t = Regex.Replace (t, @"\s+(\*+)", "$1 ");                       // "gchar *x" -> "gchar* x"
			t = Regex.Replace (t, @"(const\s+)?(\w+)\*\s+const\*", "const $2*");
			t = Regex.Replace (t, @"(\*+)\s*const\s+", "$1 ");
			t = Regex.Replace (t, @"(\w+)\s+const\s*\*", "const $1*");
			t = Regex.Replace (t, @"const\s+", "const-");
			t = Regex.Replace (t, @"unsigned\s+", "unsigned-");
			t = Regex.Replace (t, @"\bvolatile\s+", "");
			t = Regex.Replace (t, @"\s+", " ").Trim ();

			return t;
		}

		/// <summary>
		/// Resolve the type of an element that owns a &lt;type&gt; or
		/// &lt;array&gt; child (parameter, return-value, field, ...).
		/// </summary>
		public GirTypeRef Resolve (XElement owner)
		{
			var array = owner.Element (Ns.Core + "array");
			if (array != null)
				return ResolveArray (array);

			var varargs = owner.Element (Ns.Core + "varargs");
			if (varargs != null)
				return new GirTypeRef { IsEllipsis = true };

			var type = owner.Element (Ns.Core + "type");
			if (type != null)
				return ResolveType (type);

			// A <callback> in field position, or an untyped element.
			return null;
		}

		GirTypeRef ResolveType (XElement type)
		{
			var ctype = (string) type.Attribute (Ns.CType);
			var girName = (string) type.Attribute ("name");

			if (!string.IsNullOrEmpty (ctype))
				return new GirTypeRef { Type = Normalize (ctype), GirName = girName };

			return new GirTypeRef { Type = SynthesiseCType (girName), GirName = girName };
		}

		GirTypeRef ResolveArray (XElement array)
		{
			var result = new GirTypeRef { IsArray = true };

			var fixedSize = (string) array.Attribute ("fixed-size");
			if (fixedSize != null)
				result.FixedSize = int.Parse (fixedSize);

			// GIR's default for zero-terminated is 1 when the attribute is absent.
			// gapi2xml.pl almost never detected null termination, so a faithful
			// conversion marks far more arrays null-terminated than the GTK 3
			// api.xml does. That is a correctness improvement, not a regression.
			var zeroTerminated = (string) array.Attribute ("zero-terminated");
			result.NullTerminated = fixedSize == null && zeroTerminated != "0";

			var lengthIndex = (string) array.Attribute ("length");
			if (lengthIndex != null)
				result.LengthParamIndex = int.Parse (lengthIndex);

			var ctype = (string) array.Attribute (Ns.CType);
			if (!string.IsNullOrEmpty (ctype)) {
				result.Type = Normalize (ctype);
			} else {
				var inner = array.Element (Ns.Core + "type");
				var innerType = inner != null
					? ResolveType (inner).Type
					: "gpointer";
				result.Type = innerType + "*";
			}

			var innerEl = array.Element (Ns.Core + "type");
			if (innerEl != null)
				result.ElementType = ResolveType (innerEl).Type;

			return result;
		}

		/// <summary>Best-effort C type for a GIR name with no c:type attached.</summary>
		public string SynthesiseCType (string girName)
		{
			if (string.IsNullOrEmpty (girName))
				return "gpointer";

			string primitive;
			if (GirNameToCType.TryGetValue (girName, out primitive))
				return primitive;

			var info = registry.Resolve (girName, currentNamespace);
			if (info == null)
				return "gpointer";

			switch (info.Kind) {
			case GirKind.Class:
			case GirKind.Interface:
				return info.CType + "*";
			case GirKind.Record:
			case GirKind.Union:
				return info.CType + "*";
			default:
				return info.CType;
			}
		}
	}

	/// <summary>A resolved type reference, plus the array facts gapi records.</summary>
	public class GirTypeRef {
		public string Type;
		public string GirName;
		public string ElementType;
		public bool IsArray;
		public bool IsEllipsis;
		public bool NullTerminated;
		public int? FixedSize;
		public int? LengthParamIndex;
	}
}
