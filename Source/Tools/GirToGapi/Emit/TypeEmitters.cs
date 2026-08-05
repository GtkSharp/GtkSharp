// TypeEmitters.cs - records, unions, enumerations, bitfields, aliases, callbacks.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion.Emit {

	using System.Linq;
	using System.Xml.Linq;
	using GtkSharp.GirConversion.Gir;
	using GtkSharp.GirConversion.Rules;

	public class TypeEmitters {

		readonly GirDocument doc;
		readonly TypeRegistry registry;
		readonly CTypeMapper types;
		readonly CallableEmitter callables;
		readonly ConversionLog log;

		public TypeEmitters (GirDocument doc, TypeRegistry registry, CTypeMapper types,
		                     CallableEmitter callables, ConversionLog log)
		{
			this.doc = doc;
			this.registry = registry;
			this.types = types;
			this.callables = callables;
			this.log = log;
		}

		/// <summary>
		/// &lt;record&gt; / &lt;union&gt; become either &lt;boxed&gt; or
		/// &lt;struct&gt;: boxed when the type is GType-registered with copy/free
		/// semantics, a plain struct otherwise.
		/// </summary>
		public XElement Record (XElement gir)
		{
			var girName = (string) gir.Attribute ("name");
			var cname = (string) gir.Attribute (Ns.CType) ?? doc.IdentifierPrefix + girName;

			var isBoxed = gir.Attribute (Ns.GlibTypeName) != null
				&& gir.Attribute (Ns.GlibGetType) != null;

			var fields = gir.Elements (Ns.Core + "field").ToList ();

			// A boxed type is opaque by default and metadata turns that off for the
			// handful that should expose their layout -- GtkSharp.metadata does
			// exactly that for GtkBorder and GtkRequisition. Opacity decides class
			// versus struct in the generated code, and the hand-written partials
			// are written against the class form, so deriving it from whether GIR
			// happens to list fields flips types that have always been classes.
			// Gtk 3's PangoItem is opaque and still carries its four fields, so the
			// two are independent.
			//
			// For a plain struct there is no such convention: it is opaque only
			// when the layout is genuinely unusable. "disguised" is GIR's older
			// word for that, "opaque" the newer one.
			var opaque = isBoxed
				|| (string) gir.Attribute ("disguised") == "1"
				|| (string) gir.Attribute ("opaque") == "1"
				|| fields.Count == 0;

			var el = new XElement (isBoxed ? "boxed" : "struct",
				new XAttribute ("name", girName),
				new XAttribute ("cname", cname));

			if (opaque)
				el.Add (new XAttribute ("opaque", "true"));

			if ((string) gir.Attribute ("deprecated") == "1")
				el.Add (new XAttribute ("deprecated", "1"));

			// Fields are emitted either way: metadata that clears opaque needs them
			// present to have anything to expose.
			foreach (var f in fields) {
				var field = Field (f);
				if (field != null)
					el.Add (field);
			}

			foreach (var c in gir.Elements (Ns.Core + "constructor"))
				el.Add (callables.Constructor (c));

			foreach (var m in gir.Elements (Ns.Core + "method"))
				el.Add (callables.Method (m, shared: false));

			foreach (var f in gir.Elements (Ns.Core + "function"))
				el.Add (callables.Method (f, shared: true));

			var getType = (string) gir.Attribute (Ns.GlibGetType);
			if (!string.IsNullOrEmpty (getType) && getType != "intern"
			    && !el.Elements ("method").Any (m => (string) m.Attribute ("cname") == getType)) {
				el.Add (new XElement ("method",
					new XAttribute ("name", "GetType"),
					new XAttribute ("cname", getType),
					new XAttribute ("shared", "true"),
					new XElement ("return-type", new XAttribute ("type", "GType"))));
			}

			return el;
		}

		public XElement Field (XElement gir)
		{
			var cname = (string) gir.Attribute ("name");
			if (cname == null)
				return null;

			var t = types.Resolve (gir);
			if (t == null)
				return null;

			var el = new XElement ("field",
				new XAttribute ("name", NameMangler.StudlyCaps (cname)),
				new XAttribute ("cname", cname),
				new XAttribute ("type", t.Type));

			if (t.FixedSize.HasValue)
				el.Add (new XAttribute ("array_len", t.FixedSize.Value));

			var bits = (string) gir.Attribute ("bits");
			if (bits != null)
				el.Add (new XAttribute ("bits", bits));

			// A function-pointer field has no managed representation, but it still
			// occupies a slot. ClassBase counts the ABI field first and only then
			// skips the managed one on is_callback, so marking it keeps the struct
			// layout right while suppressing a member declaration whose type would
			// come out empty -- "private  _load;".
			if (IsCallbackTyped (gir))
				el.Add (new XAttribute ("is_callback", "1"));

			var isPrivate = (string) gir.Attribute ("private") == "1";
			el.Add (new XAttribute ("access", isPrivate ? "private" : "public"));

			if ((string) gir.Attribute ("writable") == "1")
				el.Add (new XAttribute ("writeable", "true"));

			return el;
		}

		/// <summary>Is this field a function pointer?</summary>
		bool IsCallbackTyped (XElement gir)
		{
			if (gir.Element (Ns.Core + "callback") != null)
				return true;

			var type = gir.Element (Ns.Core + "type");
			if (type == null)
				return false;

			var info = registry.Resolve ((string) type.Attribute ("name"), doc.Name);
			return info != null && info.Kind == GirKind.Callback;
		}

		/// <summary>&lt;enumeration&gt; and &lt;bitfield&gt; both become &lt;enum&gt;.</summary>
		public XElement Enumeration (XElement gir, bool isFlags)
		{
			var girName = (string) gir.Attribute ("name");
			var cname = (string) gir.Attribute (Ns.CType) ?? doc.IdentifierPrefix + girName;

			var el = new XElement ("enum",
				new XAttribute ("name", girName),
				new XAttribute ("cname", cname));

			var gtype = (string) gir.Attribute (Ns.GlibGetType);
			if (!string.IsNullOrEmpty (gtype) && gtype != "intern")
				el.Add (new XAttribute ("gtype", gtype));

			el.Add (new XAttribute ("type", isFlags ? "flags" : "enum"));

			if ((string) gir.Attribute ("deprecated") == "1")
				el.Add (new XAttribute ("deprecated", "1"));

			foreach (var m in gir.Elements (Ns.Core + "member")) {
				var memberCName = (string) m.Attribute (Ns.CIdentifier)
					?? (string) m.Attribute ("name");

				el.Add (new XElement ("member",
					new XAttribute ("cname", memberCName),
					new XAttribute ("name", NameMangler.EnumMemberName ((string) m.Attribute ("name"))),
					new XAttribute ("value", (string) m.Attribute ("value"))));
			}

			// Error domains carry an *_error_quark function and enums a
			// *_get_type one. gapi's <enum> holds nothing but <member>, so the
			// quark is dropped (GLibSharp binds error domains by hand) and
			// get_type is already recorded in the gtype attribute above.
			foreach (var f in gir.Elements (Ns.Core + "function")) {
				var fname = (string) f.Attribute (Ns.CIdentifier);
				if (fname != gtype)
					log.Skipped ("enum function", fname, "gapi <enum> accepts only <member>");
			}

			return el;
		}

		public XElement Alias (XElement gir)
		{
			var girName = (string) gir.Attribute ("name");
			var cname = (string) gir.Attribute (Ns.CType) ?? doc.IdentifierPrefix + girName;
			var t = types.Resolve (gir);

			return new XElement ("alias",
				new XAttribute ("name", girName),
				new XAttribute ("cname", cname),
				new XAttribute ("type", t == null ? "gpointer" : t.Type));
		}

		public XElement Callback (XElement gir)
		{
			var girName = (string) gir.Attribute ("name");
			var cname = (string) gir.Attribute (Ns.CType) ?? doc.IdentifierPrefix + girName;

			return callables.Callback (gir, girName, cname);
		}
	}
}
