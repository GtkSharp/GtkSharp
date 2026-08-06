// ObjectEmitter.cs - GIR <class>/<interface> -> gapi <object>/<interface>.
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
	using GtkSharp.GirConversion.Rules;

	public class ObjectEmitter {

		readonly TypeRegistry registry;
		readonly CTypeMapper types;
		readonly CallableEmitter callables;
		readonly GirDocument doc;
		readonly Dictionary<string, XElement> gtypeStructs;
		readonly ConversionLog log;

		public ObjectEmitter (GirDocument doc, TypeRegistry registry, CTypeMapper types,
		                      CallableEmitter callables, Dictionary<string, XElement> gtypeStructs,
		                      ConversionLog log)
		{
			this.doc = doc;
			this.registry = registry;
			this.types = types;
			this.callables = callables;
			this.gtypeStructs = gtypeStructs;
			this.log = log;
		}

		public XElement Emit (XElement gir, bool isInterface)
		{
			var girName = (string) gir.Attribute ("name");
			var cname = (string) gir.Attribute (Ns.CType) ?? doc.IdentifierPrefix + girName;

			var el = new XElement (isInterface ? "interface" : "object",
				new XAttribute ("name", girName),
				new XAttribute ("cname", cname));

			if (!isInterface) {
				var parent = (string) gir.Attribute ("parent");
				if (!string.IsNullOrEmpty (parent)) {
					var info = registry.Resolve (parent, doc.Name);
					el.Add (new XAttribute ("parent", info != null ? info.CType : parent));
				}
			}

			// Signals first: the class-struct pass needs to know which slots are
			// class closures, and the signals need the field name back.
			var signals = gir.Elements (Ns.GlibSignal).ToList ();
			var signalByField = new Dictionary<string, XElement> ();
			foreach (var s in signals) {
				var slot = ((string) s.Attribute ("name")).Replace ('-', '_');
				if (!signalByField.ContainsKey (slot))
					signalByField [slot] = s;
			}

			var virtualMethods = new Dictionary<string, XElement> ();
			foreach (var vm in gir.Elements (Ns.Core + "virtual-method")) {
				var n = (string) vm.Attribute ("name");
				if (n != null && !virtualMethods.ContainsKey (n))
					virtualMethods [n] = vm;
			}

			// An interface with no glib:type-struct has a private interface
			// struct, so nothing outside the library can implement it -- only
			// consume it. Without this InterfaceGen still emits an adapter with
			// an "iface" field whose type it never resolved: "static  iface;".
			if (isInterface && !gtypeStructs.ContainsKey (girName))
				el.Add (new XAttribute ("consume_only", "true"));

			var emittedVirtualMethods = new List<XElement> ();
			var usedAsSignalSlot = new HashSet<string> ();

			var classStruct = EmitClassStruct (gir, girName, signalByField, virtualMethods,
			                                   emittedVirtualMethods, usedAsSignalSlot, isInterface);
			if (classStruct != null)
				el.Add (classStruct);

			// <implements>
			var implemented = gir.Elements (Ns.Core + "implements")
				.Select (i => registry.Resolve ((string) i.Attribute ("name"), doc.Name))
				.Where (i => i != null)
				.ToList ();

			if (implemented.Count > 0) {
				el.Add (new XElement ("implements",
					implemented.Select (i => new XElement ("interface", new XAttribute ("cname", i.CType)))));
			}

			// Instance fields (not the class struct's).
			foreach (var f in gir.Elements (Ns.Core + "field")) {
				var field = EmitField (f);
				if (field != null)
					el.Add (field);
			}

			foreach (var p in gir.Elements (Ns.Core + "property"))
				el.Add (EmitProperty (p));

			foreach (var s in signals) {
				var slot = ((string) s.Attribute ("name")).Replace ('-', '_');
				el.Add (callables.Signal (s, usedAsSignalSlot.Contains (slot) ? slot : null));
			}

			foreach (var vm in emittedVirtualMethods)
				el.Add (vm);

			foreach (var c in gir.Elements (Ns.Core + "constructor"))
				el.Add (callables.Constructor (c));

			foreach (var m in gir.Elements (Ns.Core + "method"))
				el.Add (callables.Method (m, shared: false));

			// GIR <function> on a type is a static method.
			foreach (var f in gir.Elements (Ns.Core + "function"))
				el.Add (callables.Method (f, shared: true));

			// GIR records the GType function as an attribute rather than a
			// <function>, but gapi expects it as a shared method on every
			// registered type.
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

		/// <summary>
		/// Walks the gtype-struct record in declaration order and emits one entry
		/// per ABI slot. Order is the ABI: every entry here becomes one pointer in
		/// GapiCodegen's class_abi table (MethodABIField), so a missed or merged
		/// slot silently shifts every vfunc after it.
		/// </summary>
		XElement EmitClassStruct (XElement gir, string girName,
		                          Dictionary<string, XElement> signalByField,
		                          Dictionary<string, XElement> virtualMethods,
		                          List<XElement> emittedVirtualMethods,
		                          HashSet<string> usedAsSignalSlot,
		                          bool isInterface)
		{
			XElement record;
			if (!gtypeStructs.TryGetValue (girName, out record))
				return null;

			var cs = new XElement ("class_struct",
				new XAttribute ("cname", (string) record.Attribute (Ns.CType)));

			var first = true;

			foreach (var f in record.Elements (Ns.Core + "field")) {
				var fieldName = (string) f.Attribute ("name");

				if (first) {
					// The parent class/iface struct. ObjectBase.cs skips the first
					// <field> as the parent, so it has to stay first and stay a field.
					var parentField = EmitField (f);
					if (parentField != null)
						cs.Add (parentField);
					first = false;
					continue;
				}

				var padding = PaddingSlotNames (f, fieldName);
				if (padding != null) {
					foreach (var slot in padding) {
						cs.Add (new XElement ("method", new XAttribute ("vm", slot)));
						emittedVirtualMethods.Add (CallableEmitter.PaddingSlot (slot));
					}
					continue;
				}

				var callback = f.Element (Ns.Core + "callback");
				if (callback == null) {
					var plain = EmitField (f);
					if (plain != null)
						cs.Add (plain);
					continue;
				}

				// A callback slot. It is a signal's class closure when its name
				// matches a signal on the same class; otherwise a plain vfunc.
				// Validated at 328/329 against GTK 3 with zero false negatives -
				// see Docs/gir-gapi-coverage.md section 3.
				if (signalByField.ContainsKey (fieldName)) {
					cs.Add (new XElement ("method", new XAttribute ("signal_vm", fieldName)));
					usedAsSignalSlot.Add (fieldName);
					continue;
				}

				cs.Add (new XElement ("method", new XAttribute ("vm", fieldName)));

				// Every <method vm="X"> is resolved against a <virtual_method
				// cname="X"> by ObjectBase.cs, so one must exist. Prefer GIR's
				// own <virtual-method> declaration; fall back to the callback
				// signature in the class struct itself.
				XElement source;
				if (!virtualMethods.TryGetValue (fieldName, out source))
					source = callback;

				emittedVirtualMethods.Add (callables.VirtualMethod (source, fieldName));
			}

			return cs;
		}

		/// <summary>
		/// Padding slot names for a class-struct field, or null if it is not padding.
		/// </summary>
		/// <remarks>
		/// gapi2xml.pl matched reserved_?N / padding_?N / recent_?N by name
		/// (gapi2xml.pl:573). GTK 4 uses two shapes: individual _gtk_reservedN
		/// pointers, which that rule still covers, and a single fixed-size array
		/// field standing for N pointers, which it does not. The array must be
		/// expanded, because each slot is one pointer of ABI.
		/// </remarks>
		static List<string> PaddingSlotNames (XElement field, string fieldName)
		{
			var array = field.Element (Ns.Core + "array");
			if (array != null) {
				var size = (string) array.Attribute ("fixed-size");
				if (size != null && IsPaddingName (fieldName)) {
					var n = int.Parse (size);
					return Enumerable.Range (1, n).Select (i => fieldName + "_" + i).ToList ();
				}
				return null;
			}

			if (Regex.IsMatch (fieldName, @"(reserved|padding|recent)_?[0-9]+$"))
				return new List<string> { fieldName };

			return null;
		}

		static bool IsPaddingName (string fieldName)
		{
			return Regex.IsMatch (fieldName, @"^_*(padding|reserved|dummy)$");
		}

		XElement EmitField (XElement gir)
		{
			var cname = (string) gir.Attribute ("name");
			if (cname == null)
				return null;

			var t = types.Resolve (gir);
			if (t == null) {
				// A callback-typed field outside a class struct; gapi has no
				// representation for it as a plain field.
				log.Skipped ("field", cname, "no resolvable type");
				return null;
			}

			var el = new XElement ("field",
				new XAttribute ("name", NameMangler.StudlyCaps (cname)),
				new XAttribute ("cname", cname),
				new XAttribute ("type", t.Type));

			if (t.FixedSize.HasValue)
				el.Add (new XAttribute ("array_len", t.FixedSize.Value));

			var bits = (string) gir.Attribute ("bits");
			if (bits != null)
				el.Add (new XAttribute ("bits", bits));

			if ((string) gir.Attribute ("private") == "1")
				el.Add (new XAttribute ("access", "private"));

			if ((string) gir.Attribute ("writable") == "1")
				el.Add (new XAttribute ("writeable", "true"));

			return el;
		}

		XElement EmitProperty (XElement gir)
		{
			var cname = (string) gir.Attribute ("name");
			var t = types.Resolve (gir);

			var el = new XElement ("property",
				new XAttribute ("name", NameMangler.StudlyCaps (cname)),
				new XAttribute ("cname", cname),
				new XAttribute ("type", t == null ? "gpointer" : t.Type));

			// GIR omits readable="1" because readable is the default.
			if ((string) gir.Attribute ("readable") != "0")
				el.Add (new XAttribute ("readable", "true"));
			if ((string) gir.Attribute ("writable") == "1")
				el.Add (new XAttribute ("writeable", "true"));
			if ((string) gir.Attribute ("construct") == "1")
				el.Add (new XAttribute ("construct", "true"));
			if ((string) gir.Attribute ("construct-only") == "1")
				el.Add (new XAttribute ("construct-only", "true"));
			if ((string) gir.Attribute ("deprecated") == "1")
				el.Add (new XAttribute ("deprecated", "true"));

			return el;
		}
	}
}
