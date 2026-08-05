// ApiWriter.cs - assembles the gapi api.xml document.
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

	public class ApiWriter {

		/// <summary>
		/// Must match GapiCodegen's Parser.cs:curr_parser_version. The generator
		/// warns when it reads a file claiming to be newer than itself.
		/// </summary>
		public const int ParserVersion = 3;

		readonly GirDocument doc;
		readonly TypeRegistry registry;
		readonly ConversionLog log;
		readonly string groupPrefix;

		/// <param name="groupPrefix">
		/// Overrides the C prefix that namespace-level functions are grouped by.
		/// Needed where several gir namespaces are merged into one gapi namespace:
		/// pango_cairo_create_layout has to group as Pango's "cairo" class, the
		/// way gapi2xml.pl saw it when Pango and PangoCairo were one namespace,
		/// not as PangoCairo's "create" class. The resulting CairoHelper is public
		/// API the samples use.
		/// </param>
		public ApiWriter (GirDocument doc, TypeRegistry registry, ConversionLog log,
		                  string groupPrefix = null)
		{
			this.doc = doc;
			this.registry = registry;
			this.log = log;
			this.groupPrefix = groupPrefix;
		}

		/// <summary>
		/// Wraps one or more converted namespaces in the api.xml root element.
		/// More than one is normal: GdkSharp binds Gdk and GdkPixbuf together, so
		/// its api.xml carries a namespace for each.
		/// </summary>
		public static XDocument Document (IEnumerable<XElement> namespaces)
		{
			return new XDocument (
				new XElement ("api",
					new XAttribute ("parser_version", ParserVersion),
					namespaces));
		}

		public XElement Write ()
		{
			var types = new CTypeMapper (registry, doc.Name);
			var callables = new CallableEmitter (types);
			var typeEmitters = new TypeEmitters (doc, registry, types, callables, log);
			var objects = new ObjectEmitter (doc, registry, types, callables,
			                                 registry.GTypeStructsFor (doc), log);

			var ns = new XElement ("namespace", new XAttribute ("name", doc.Name));

			// The library string is pasted verbatim into GLibrary.Load(...) by
			// GapiCodegen, so it has to end up as a C# expression. The raw
			// shared-library name is emitted here and normalised to a Library
			// enum member by each assembly's .metadata, which is how
			// GtkSourceSharp already worked.
			if (!string.IsNullOrEmpty (doc.SharedLibrary))
				ns.Add (new XAttribute ("library", doc.SharedLibrary.Split (',') [0]));

			// Emitted in a fixed order so regenerating produces reviewable diffs.
			foreach (var el in doc.Namespace.Elements (Ns.Core + "alias"))
				ns.Add (typeEmitters.Alias (el));

			foreach (var el in doc.Namespace.Elements (Ns.Core + "enumeration"))
				ns.Add (typeEmitters.Enumeration (el, isFlags: false));

			foreach (var el in doc.Namespace.Elements (Ns.Core + "bitfield"))
				ns.Add (typeEmitters.Enumeration (el, isFlags: true));

			foreach (var el in doc.Namespace.Elements (Ns.Core + "callback"))
				ns.Add (typeEmitters.Callback (el));

			foreach (var el in doc.Namespace.Elements (Ns.Core + "interface"))
				ns.Add (objects.Emit (el, isInterface: true));

			foreach (var el in doc.Namespace.Elements (Ns.Core + "class"))
				ns.Add (objects.Emit (el, isInterface: false));

			// gtype-struct records are emitted as <class_struct> inside their
			// owning type, never as standalone structs.
			var gtypeStructs = registry.GTypeStructsFor (doc);
			var emittedAsClassStruct = new HashSet<XElement> (gtypeStructs.Values);

			foreach (var el in doc.Namespace.Elements (Ns.Core + "record")) {
				if (emittedAsClassStruct.Contains (el))
					continue;
				ns.Add (typeEmitters.Record (el));
			}

			foreach (var el in doc.Namespace.Elements (Ns.Core + "union"))
				ns.Add (typeEmitters.Record (el));

			foreach (var cls in GroupGlobalFunctions (callables))
				ns.Add (cls);

			var constants = doc.Namespace.Elements (Ns.Core + "constant").Count ();
			if (constants > 0)
				log.Skipped ("constant", doc.Name, constants + " namespace-level constants; gapi has no namespace-level slot for them");

			return ns;
		}

		/// <summary>
		/// Namespace-level functions become &lt;class&gt; elements, the way
		/// gapi2xml.pl's addStaticFuncElems grouped them: functions sharing a
		/// &lt;prefix&gt;_&lt;word&gt;_ stem are collected into a class named
		/// after that word, and everything else lands in Global.
		/// </summary>
		/// <remarks>
		/// One deliberate deviation: gapi2xml.pl silently DROPPED functions whose
		/// cname had no third token (gtk_init had no home, only gtk_init_check
		/// did). Those go to Global here instead of vanishing. Metadata that
		/// selects these by class cname is re-triaged in Phase 4 regardless,
		/// since GTK 4's global function set barely overlaps GTK 3's.
		/// </remarks>
		IEnumerable<XElement> GroupGlobalFunctions (CallableEmitter callables)
		{
			// Verb-like stems are never class names; gapi2xml.pl:861 skipped them.
			var notAClassName = new HashSet<string> {
				"set", "get", "scan", "find", "add", "remove", "free",
				"register", "execute", "show", "parse", "paint", "string",
			};

			var prefixes = string.IsNullOrEmpty (groupPrefix)
				? doc.SymbolPrefixes
				: new[] { groupPrefix };

			var functions = doc.Namespace.Elements (Ns.Core + "function")
				.Where (f => f.Attribute (Ns.CIdentifier) != null)
				.OrderBy (f => (string) f.Attribute (Ns.CIdentifier), System.StringComparer.Ordinal)
				.ToList ();

			var stemOf = new Dictionary<XElement, string> ();
			var prefixOf = new Dictionary<XElement, string> ();
			var stemCounts = new Dictionary<string, int> ();

			foreach (var f in functions) {
				var cname = (string) f.Attribute (Ns.CIdentifier);

				foreach (var prefix in prefixes) {
					var m = Regex.Match (cname,
						"^" + Regex.Escape (prefix + "_") + @"([a-zA-Z]+)_\w+$");

					if (!m.Success)
						continue;

					if (notAClassName.Contains (m.Groups [1].Value))
						break;

					var stem = m.Groups [1].Value;
					stemOf [f] = stem;
					prefixOf [f] = prefix;
					stemCounts [stem] = stemCounts.TryGetValue (stem, out var n) ? n + 1 : 1;
					break;
				}
			}

			var groups = new Dictionary<string, XElement> ();
			var global = new XElement ("class",
				new XAttribute ("name", "Global"),
				new XAttribute ("cname", doc.IdentifierPrefix + "Global"));

			foreach (var f in functions) {
				string stem;
				var grouped = stemOf.TryGetValue (f, out stem) && stemCounts [stem] > 1;

				var method = callables.Method (f, shared: true);

				if (!grouped) {
					global.Add (method);
					continue;
				}

				XElement cls;
				if (!groups.TryGetValue (stem, out cls)) {
					cls = new XElement ("class",
						new XAttribute ("name", NameMangler.StudlyCaps (stem)),
						new XAttribute ("cname",
							NameMangler.StudlyCaps (prefixOf [f] + "_" + stem + "_")));
					groups [stem] = cls;
				}

				// Inside a group the class stem is redundant: GIR names
				// gtk_accel_groups_activate "accel_groups_activate", and gapi
				// wants GroupsActivate.
				var girName = (string) f.Attribute ("name");
				if (girName.StartsWith (stem + "_"))
					method.SetAttributeValue ("name",
						NameMangler.StudlyCaps (girName.Substring (stem.Length + 1)));

				cls.Add (method);
			}

			foreach (var cls in groups.Values.OrderBy (c => (string) c.Attribute ("name"), System.StringComparer.Ordinal))
				yield return cls;

			if (global.HasElements)
				yield return global;
		}
	}

	/// <summary>Collects what the conversion could not represent.</summary>
	public class ConversionLog {

		readonly List<string> skipped = new List<string> ();

		public void Skipped (string kind, string name, string reason)
		{
			skipped.Add (string.Format ("  skipped {0} '{1}': {2}", kind, name, reason));
		}

		public int Count { get { return skipped.Count; } }

		public void Report ()
		{
			if (skipped.Count == 0)
				return;

			// Never let a coverage gap pass silently: an api.xml that is quietly
			// missing a type looks identical to one where the type was removed
			// upstream.
			System.Console.WriteLine ("gir-to-gapi: {0} item(s) not represented:", skipped.Count);
			foreach (var line in skipped)
				System.Console.WriteLine (line);
		}
	}
}
