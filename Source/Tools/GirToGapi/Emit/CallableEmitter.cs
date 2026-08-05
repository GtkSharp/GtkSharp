// CallableEmitter.cs - methods, constructors, signals, virtual methods.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.

namespace GtkSharp.GirConversion.Emit {

	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using GtkSharp.GirConversion.Gir;
	using GtkSharp.GirConversion.Rules;

	/// <summary>
	/// Emits everything that has a return type and a parameter list. Shared by
	/// methods, functions, constructors, signals, virtual methods and callbacks
	/// because gapi gives them all the same body shape.
	/// </summary>
	public class CallableEmitter {

		readonly CTypeMapper types;

		public CallableEmitter (CTypeMapper types)
		{
			this.types = types;
		}

		/// <summary>&lt;method&gt; from a GIR method / function.</summary>
		public XElement Method (XElement gir, bool shared)
		{
			var el = new XElement ("method",
				new XAttribute ("name", NameMangler.StudlyCaps ((string) gir.Attribute ("name"))),
				new XAttribute ("cname", (string) gir.Attribute (Ns.CIdentifier)));

			if (shared)
				el.Add (new XAttribute ("shared", "true"));

			AddDeprecated (el, gir);
			AddBody (el, gir, skipInstance: !shared);

			return el;
		}

		/// <summary>&lt;constructor&gt;.</summary>
		/// <remarks>
		/// gapi2xml.pl emitted no name attribute here and let GapiCodegen derive
		/// one, but that derivation assumes the cname contains "new"
		/// ([Ctor.cs:67](../../GapiCodegen/Ctor.cs)) and throws on anything else.
		/// Graphene's constructors are `graphene_point_alloc` and friends, which
		/// crash it. GIR knows the name, so pass it and the derivation is never
		/// reached. For the ordinary `*_new_with_label` case this produces
		/// exactly the string the old derivation did.
		/// </remarks>
		public XElement Constructor (XElement gir)
		{
			var el = new XElement ("constructor",
				new XAttribute ("cname", (string) gir.Attribute (Ns.CIdentifier)),
				new XAttribute ("name", NameMangler.StudlyCaps ((string) gir.Attribute ("name"))));

			AddDeprecated (el, gir);
			AddBody (el, gir, skipInstance: true, includeEmptyParameters: false);

			// A constructor's return-type is the type itself; gapi does not
			// record it, matching gapi2xml.pl.
			var ret = el.Element ("return-type");
			if (ret != null)
				ret.Remove ();

			return el;
		}

		/// <summary>
		/// &lt;signal&gt;. <paramref name="fieldName"/> is the class-struct field
		/// holding the class closure; ObjectBase.cs keys signal_vms on it, so it
		/// must match the corresponding &lt;method signal_vm=&gt; exactly.
		/// </summary>
		public XElement Signal (XElement gir, string fieldName)
		{
			var el = new XElement ("signal",
				new XAttribute ("name", NameMangler.StudlyCaps ((string) gir.Attribute ("name"))),
				new XAttribute ("cname", (string) gir.Attribute ("name")));

			var when = (string) gir.Attribute ("when");
			if (!string.IsNullOrEmpty (when))
				el.Add (new XAttribute ("when", when.ToUpperInvariant ()));

			if (fieldName != null)
				el.Add (new XAttribute ("field_name", fieldName));

			AddDeprecated (el, gir);
			// Signals always carry a <parameters> element, even when empty.
			AddBody (el, gir, skipInstance: true, includeEmptyParameters: true);

			return el;
		}

		/// <summary>&lt;virtual_method&gt; for a real vfunc slot.</summary>
		public XElement VirtualMethod (XElement gir, string cname)
		{
			var el = new XElement ("virtual_method",
				new XAttribute ("name", NameMangler.StudlyCaps (cname)),
				new XAttribute ("cname", cname));

			AddDeprecated (el, gir);
			AddBody (el, gir, skipInstance: true);

			return el;
		}

		/// <summary>
		/// A padding slot: an unused pointer in the class struct that exists only
		/// to keep the ABI stable. It still needs a virtual_method, because every
		/// &lt;method vm=&gt; in the class struct is resolved against one.
		/// </summary>
		public static XElement PaddingSlot (string cname)
		{
			return new XElement ("virtual_method",
				new XAttribute ("name", NameMangler.StudlyCaps (cname)),
				new XAttribute ("cname", cname),
				new XAttribute ("shared", "true"),
				new XAttribute ("padding", "true"),
				new XElement ("return-type", new XAttribute ("type", "void")));
		}

		/// <summary>&lt;callback&gt; at namespace level.</summary>
		public XElement Callback (XElement gir, string name, string cname)
		{
			var el = new XElement ("callback",
				new XAttribute ("name", name),
				new XAttribute ("cname", cname));

			AddBody (el, gir, skipInstance: false);

			return el;
		}

		// ------------------------------------------------------------------

		void AddBody (XElement el, XElement gir, bool skipInstance,
		              bool includeEmptyParameters = false)
		{
			el.Add (ReturnType (gir.Element (Ns.Core + "return-value")));

			var girParams = gir.Element (Ns.Core + "parameters");
			var list = girParams == null
				? new List<XElement> ()
				: girParams.Elements (Ns.Core + "parameter").ToList ();

			if (list.Count == 0 && !includeEmptyParameters) {
				if (girParams != null && girParams.Attribute ("throws") == null)
					return;
				if (girParams == null)
					return;
			}

			var parameters = new XElement ("parameters");

			// GIR sets throws on methods but not on callbacks, even when the
			// callback takes a trailing GError**. gapi keys the whole GError
			// treatment off this attribute -- Parameters.IsHidden only hides a
			// GError** when Throws is set -- so without it the parameter stays
			// visible and collides with the "error" local codegen declares for it.
			// Infer it from the parameter list rather than trusting the attribute.
			var throws = (string) gir.Attribute ("throws") == "1"
				|| list.Any (p => {
					var t = types.Resolve (p);
					return t != null && t.Type == "GError**";
				});

			if (throws)
				parameters.Add (new XAttribute ("throws", "1"));

			foreach (var p in list)
				parameters.Add (Parameter (p));

			el.Add (parameters);
		}

		XElement ReturnType (XElement girReturn)
		{
			if (girReturn == null)
				return new XElement ("return-type", new XAttribute ("type", "void"));

			var t = types.Resolve (girReturn);
			var el = new XElement ("return-type",
				new XAttribute ("type", t == null ? "void" : t.Type));

			var transfer = (string) girReturn.Attribute ("transfer-ownership");

			// "full" hands the whole thing over; "container" hands over only the
			// container, leaving the elements borrowed.
			if (transfer == "full" || transfer == "container")
				el.Add (new XAttribute ("owned", "true"));

			if (transfer == "full" && t != null && IsNullTerminatedStringArray (t))
				el.Add (new XAttribute ("elements_owned", "true"));

			if (t != null && IsNullTerminatedStringArray (t))
				el.Add (new XAttribute ("null_term_array", "true"));

			// element_type is deliberately not emitted. GapiCodegen only knows how
			// to use it on GList/GSList/GPtrArray returns (ReturnValue.cs:147-155)
			// and throws on anything else, so gapi2xml.pl left it to the .metadata
			// files -- there are twelve hand-added occurrences across the whole
			// tree. Emitting it here would turn every string array into a crash.
			return el;
		}

		XElement Parameter (XElement girParam)
		{
			var t = types.Resolve (girParam);

			if (t != null && t.IsEllipsis)
				return new XElement ("parameter", new XAttribute ("ellipsis", "true"));

			var el = new XElement ("parameter",
				new XAttribute ("type", t == null ? "gpointer" : t.Type),
				new XAttribute ("name", (string) girParam.Attribute ("name") ?? "arg"));

			// direction="out" with caller-allocates="1" on an array is a buffer the
			// caller supplies for the callee to fill -- g_input_stream_read's
			// `void *buffer` -- not a C# out parameter. Marking it out makes
			// codegen emit a method that never assigns it. gapi2xml.pl left these
			// bare too. A caller-allocated *struct* is still a genuine out
			// parameter, so the exemption is limited to arrays.
			var direction = (string) girParam.Attribute ("direction");
			var callerAllocatedBuffer = (string) girParam.Attribute ("caller-allocates") == "1"
				&& t != null && t.IsArray;

			if (direction == "out" && !callerAllocatedBuffer)
				el.Add (new XAttribute ("pass_as", "out"));
			else if (direction == "inout")
				el.Add (new XAttribute ("pass_as", "ref"));

			if ((string) girParam.Attribute ("transfer-ownership") == "full")
				el.Add (new XAttribute ("owned", "true"));

			// Scope is emitted verbatim. The XSD says the value should be "notify",
			// but the generator actually tests for GIR's own "notified"
			// (ManagedCallString.cs:42, Parameters.cs:250) -- the schema is what is
			// out of step, and it is corrected rather than obeyed here.
			//
			// A scope beyond "call" only means something alongside the indices of
			// the user_data and destroy-notify parameters it belongs to. Without
			// them MethodBody.cs guesses that they sit at i+1 and i+2, and where
			// several callbacks share one user_data -- g_bus_own_name takes three
			// against a single closure, and GIR annotates only the last -- that
			// guess lands on the next callback and generates code that does not
			// compile. Better to leave the parameter a plain delegate than to
			// claim a lifetime the binding cannot honour.
			var scope = (string) girParam.Attribute ("scope");
			var closure = (string) girParam.Attribute ("closure");
			var destroy = (string) girParam.Attribute ("destroy");

			if (scope == "call") {
				el.Add (new XAttribute ("scope", scope));
			} else if (!string.IsNullOrEmpty (scope) && closure != null) {
				el.Add (new XAttribute ("scope", scope));
				el.Add (new XAttribute ("closure", closure));

				if (destroy != null)
					el.Add (new XAttribute ("destroy", destroy));
			}

			// As with return types, only the null-terminated string-array shape is
			// marshallable without a metadata-supplied length parameter. A bare
			// array="true" with no count parameter makes Parameter.cs throw.
			if (t != null && IsNullTerminatedStringArray (t))
				el.Add (new XAttribute ("null_term_array", "true"));

			return el;
		}

		/// <summary>
		/// The one array shape GapiCodegen can marshal unaided:
		/// GLib.Marshaller.NullTermPtrToStringArray. Everything else needs a count
		/// parameter that only .metadata can point at.
		/// </summary>
		static bool IsNullTerminatedStringArray (GirTypeRef t)
		{
			if (!t.IsArray || !t.NullTerminated || t.Type == null)
				return false;

			return t.Type == "gchar**" || t.Type == "char**"
				|| t.Type == "const-gchar**" || t.Type == "const-char**";
		}

		static void AddDeprecated (XElement el, XElement gir)
		{
			if ((string) gir.Attribute ("deprecated") == "1")
				el.Add (new XAttribute ("deprecated", "1"));
		}
	}
}
