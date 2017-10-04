// GtkSharp.Generation.ClassBase.cs - Common code between object
// and interface wrappers
//
// Authors: Rachel Hestilow <hestilow@ximian.com>
//          Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2002 Rachel Hestilow
// Copyright (c) 2001-2003 Mike Kestner 
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	public abstract class ClassBase : GenBase {
		private IDictionary<string, Property> props = new Dictionary<string, Property> ();
		private IDictionary<string, ObjectField> fields = new Dictionary<string, ObjectField> ();
		private IDictionary<string, Method> methods = new Dictionary<string, Method> ();
		private IDictionary<string, Constant> constants = new Dictionary<string, Constant>();
		protected IList<string> interfaces = new List<string>();
		protected IList<string> managed_interfaces = new List<string>();
		protected IList<Ctor> ctors = new List<Ctor>();

		protected List<StructABIField> abi_fields = new List<StructABIField> ();
		protected bool abi_fields_valid; // false if the instance structure contains a bitfield or fields of unknown types

		private bool ctors_initted = false;
		private Dictionary<string, Ctor> clash_map;
		private bool deprecated = false;
		private bool isabstract = false;

		public IDictionary<string, Method> Methods {
			get {
				return methods;
			}
		}

		public IDictionary<string, Property> Properties {
			get {
				return props;
			}
		}

		public ClassBase Parent {
			get {
				string parent = Elem.GetAttribute("parent");

				if (parent == "")
					return null;
				else
					return SymbolTable.Table.GetClassGen(parent);
			}
		}

		protected ClassBase (XmlElement ns, XmlElement elem) : base (ns, elem) {

			deprecated = elem.GetAttributeAsBoolean ("deprecated");
			isabstract = elem.GetAttributeAsBoolean ("abstract");
			abi_fields_valid = true;
			string parent_type = Elem.GetAttribute("parent");

			int num_abi_fields = 0;
			foreach (XmlNode node in elem.ChildNodes) {
				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;
				StructABIField abi_field = null;

				// Make sure ABI fields are taken into account, even when hidden.
				if (node.Name == "field") {
					num_abi_fields += 1;
					// Skip instance parent struct if present, taking into account
					// bindinator broken behaviour concerning parent field (ie.
					// marking it as pointer, somehow risky but good enough for now.)
					if (num_abi_fields != 1 ||
							parent_type == "" ||
							(member.GetAttribute("type").Replace("*", "") != parent_type
							 )) {
						abi_field = new StructABIField (member, this, "abi_info");
						abi_fields.Add (abi_field);
					}
				} else if (node.Name == "union") {
					abi_field = new UnionABIField (member, this, "abi_info");
					abi_fields.Add (abi_field);
				}

				if (member.GetAttributeAsBoolean ("hidden"))
					continue;
				
				string name;
				switch (node.Name) {
				case "method":
					name = member.GetAttribute("name");
					while (methods.ContainsKey(name))
						name += "mangled";
					methods.Add (name, new Method (member, this));
					break;

				case "property":
					name = member.GetAttribute("name");
					while (props.ContainsKey(name))
						name += "mangled";
					props.Add (name, new Property (member, this));
					break;

				case "field":
					// FIXME Generate callbacks.
					if (member.GetAttributeAsBoolean ("is_callback"))
						continue;

					name = member.GetAttribute("name");
					while (fields.ContainsKey (name))
						name += "mangled";

					var field = new ObjectField (member, this);
					field.abi_field = abi_field;
					fields.Add (name, field);
					break;

				case "implements":
					ParseImplements (member);
					break;

				case "constructor":
					ctors.Add (new Ctor (member, this));
					break;

				case "constant":
					name = member.GetAttribute ("name");
					constants.Add (name, new Constant (member));
					break;

				default:
					break;
				}
			}
		}

		public virtual bool CanGenerateABIStruct(LogWriter log) {
			return (abi_fields_valid);
		}

		bool CheckABIStructParent(LogWriter log, out string cs_parent_struct) {
			cs_parent_struct = null;

			if (!CanGenerateABIStruct(log))
				return false;

			var parent = SymbolTable.Table[Elem.GetAttribute("parent")];
			string cs_parent = SymbolTable.Table.GetCSType(Elem.GetAttribute("parent"));
			var parent_can_generate = true;

			cs_parent_struct = null;
			if (parent != null) {
				// FIXME Add that information to ManualGen and use it.
				if (parent.CName == "GInitiallyUnowned" || parent.CName == "GObject") {
					cs_parent_struct = "GLib.Object";
				} else {
					parent_can_generate = false;
					var _parent = parent as ClassBase;

					if (_parent != null) {
						string tmp;
						parent_can_generate = _parent.CheckABIStructParent(log, out tmp);
					}

					if (parent_can_generate)
						cs_parent_struct = cs_parent;
				}

				if (!parent_can_generate) {
					log.Warn("Can't generate ABI structrure as the parent structure '" +
							parent.CName + "' can't be generated.");
					return false;
				}
			} else {
				cs_parent_struct = "";
			}
			return parent_can_generate;
		}

		protected void GenerateStructureABI (GenerationInfo gen_info) {
			GenerateStructureABI(gen_info, null, "abi_info", CName);
		}

		protected void GenerateStructureABI (GenerationInfo gen_info, List<StructABIField> _fields,
				string info_name, string structname)
		{
			string cs_parent_struct = null;

			if (_fields == null)
				_fields = abi_fields;

			LogWriter log = new LogWriter (QualifiedName);
			if (!CheckABIStructParent (log, out cs_parent_struct))
				return;

			StreamWriter sw = gen_info.Writer;

			var _new = "";
			if (cs_parent_struct != "")
				_new = "new ";

			sw.WriteLine ();
			sw.WriteLine ("\t\t// Internal representation of the wrapped structure ABI.");
			sw.WriteLine ("\t\tstatic GLib.AbiStruct _" + info_name + " = null;");
			sw.WriteLine ("\t\tstatic public " + _new + "GLib.AbiStruct " + info_name + " {");
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\tif (_" + info_name + " == null)");

			// Generate Tests
			var using_parent_fields = false;
			if (_fields.Count > 0) {
				sw.WriteLine ("\t\t\t\t\t_" + info_name + " = new GLib.AbiStruct (new List<GLib.AbiField>{ ");

				if (gen_info.CAbiWriter != null) {
					gen_info.CAbiWriter.WriteLine("\tg_print(\"\\\"sizeof({0})\\\": \\\"%\" G_GUINT64_FORMAT \"\\\"\\n\", (guint64) sizeof({0}));", structname);
					gen_info.AbiWriter.WriteLine("\t\t\tConsole.WriteLine(\"\\\"sizeof({0})\\\": \\\"\" + {1}.{2}." + info_name + ".Size + \"\\\"\");", structname, NS, Name);
				}
			} else {
				if (cs_parent_struct != "") {
					sw.WriteLine ("\t\t\t\t\t_" + info_name + " = new GLib.AbiStruct ({0}.{1}.Fields);", cs_parent_struct, info_name);
					using_parent_fields = true;
				} else {
					sw.WriteLine ("\t\t\t\t\t_" + info_name + " = new GLib.AbiStruct (new List<GLib.AbiField>{ ");
					using_parent_fields = false;
				}
			}

			StructABIField prev = null;
			StructABIField next = null;

			StringWriter field_alignment_structures_writer = new StringWriter();
			for(int i=0; i < _fields.Count; i++) {
				var field = _fields[i];
				next = _fields.Count > i +1 ? _fields[i + 1] : null;

				prev = field.Generate(gen_info, "\t\t\t\t\t", prev, next, cs_parent_struct,
						field_alignment_structures_writer);
				var union = field as UnionABIField;
				if (union == null && gen_info.CAbiWriter != null && !field.IsBitfield) {
					gen_info.AbiWriter.WriteLine("\t\t\tConsole.WriteLine(\"\\\"{0}.{3}\\\": \\\"\" + {1}.{2}." + info_name + ".GetFieldOffset(\"{3}\") + \"\\\"\");", structname, NS, Name, field.CName);
					gen_info.CAbiWriter.WriteLine("\tg_print(\"\\\"{0}.{1}\\\": \\\"%\" G_GUINT64_FORMAT \"\\\"\\n\", (guint64) G_STRUCT_OFFSET({0}, {1}));", structname, field.CName);
				}

			}

			if (_fields.Count > 0 && gen_info.CAbiWriter != null) {
				gen_info.AbiWriter.Flush();
				gen_info.CAbiWriter.Flush();
			}

			if (!using_parent_fields)
				sw.WriteLine ("\t\t\t\t\t});");
			sw.WriteLine ();
			sw.WriteLine ("\t\t\t\treturn _" + info_name + ";");
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();

			sw.WriteLine (field_alignment_structures_writer.ToString());
			sw.WriteLine ("\t\t// End of the ABI representation.");
			sw.WriteLine ();
		}

		public override bool Validate ()
		{
			LogWriter log = new LogWriter (QualifiedName);

			foreach (string iface in interfaces) {
				InterfaceGen igen = SymbolTable.Table[iface] as InterfaceGen;
				if (igen == null) {
					log.Warn ("implements unknown GInterface " + iface);
					return false;
				}
				if (!igen.ValidateForSubclass ()) {
					log.Warn ("implements invalid GInterface " + iface);
					return false;
				}
			}

			foreach (StructABIField abi_field in abi_fields) {
				if (!abi_field.Validate(log))
					abi_fields_valid = false;
			}
			if (abi_fields_valid)
				foreach (StructABIField abi_field in abi_fields) {
					abi_field.SetGetOffseName();
			}

			ArrayList invalids = new ArrayList ();

			foreach (Property prop in props.Values) {
				if (!prop.Validate (log))
					invalids.Add (prop);
			}
			foreach (Property prop in invalids)
				props.Remove (prop.Name);
			invalids.Clear ();

			foreach (ObjectField field in fields.Values) {
				if (!field.Validate (log))
					invalids.Add (field);
			}
			foreach (ObjectField field in invalids)
				fields.Remove (field.Name);
			invalids.Clear ();

			foreach (Method method in methods.Values) {
				if (!method.Validate (log))
					invalids.Add (method);
			}
			foreach (Method method in invalids)
				methods.Remove (method.Name);
			invalids.Clear ();

			foreach (Constant con in constants.Values) {
				if (!con.Validate (log))
					invalids.Add (con);
			}
			foreach (Constant con in invalids)
				constants.Remove (con.Name);
			invalids.Clear ();

			foreach (Ctor ctor in ctors) {
				if (!ctor.Validate (log))
					invalids.Add (ctor);
			}
			foreach (Ctor ctor in invalids)
				ctors.Remove (ctor);
			invalids.Clear ();

			return true;
		}

		public bool IsDeprecated {
			get {
				return deprecated;
			}
		}

		public bool IsAbstract {
			get {
				return isabstract;
			}
		}

		public abstract string AssignToName { get; }

		public abstract string CallByName ();

		public override string DefaultValue {
			get {
				return "null";
			}
		}

		protected virtual bool IsNodeNameHandled (string name)
		{
			switch (name) {
			case "method":
			case "property":
			case "field":
			case "signal":
			case "implements":
			case "constructor":
			case "disabledefaultconstructor":
			case "constant":
				return true;
				
			default:
				return false;
			}
		}

		public void GenProperties (GenerationInfo gen_info, ClassBase implementor)
		{		
			if (props.Count == 0)
				return;

			foreach (Property prop in props.Values)
				prop.Generate (gen_info, "\t\t", implementor);
		}

		protected void GenFields (GenerationInfo gen_info)
		{
			foreach (ObjectField field in fields.Values) {
				field.Generate (gen_info, "\t\t");
			}
		}

		protected void GenConstants (GenerationInfo gen_info)
		{
			foreach (Constant con in constants.Values)
				con.Generate (gen_info, "\t\t");
		}

		private void ParseImplements (XmlElement member)
		{
			foreach (XmlNode node in member.ChildNodes) {
				if (node.Name != "interface")
					continue;
				XmlElement element = (XmlElement) node;
				if (element.GetAttributeAsBoolean ("hidden"))
					continue;
				if (element.HasAttribute ("cname"))
					interfaces.Add (element.GetAttribute ("cname"));
				else if (element.HasAttribute ("name"))
					managed_interfaces.Add (element.GetAttribute ("name"));
			}
		}
		
		protected bool IgnoreMethod (Method method, ClassBase implementor)
		{	
			if (implementor != null && implementor.QualifiedName != this.QualifiedName && method.IsStatic)
				return true;

			string mname = method.Name;
			return ((method.IsSetter || (method.IsGetter && mname.StartsWith("Get"))) &&
				((props != null) && props.ContainsKey(mname.Substring(3)) ||
				 (fields != null) && fields.ContainsKey(mname.Substring(3))));
		}

		public void GenMethods (GenerationInfo gen_info, IDictionary<string, bool> collisions, ClassBase implementor)
		{
			if (methods == null)
				return;

			foreach (Method method in methods.Values) {
				if (IgnoreMethod (method, implementor))
					continue;

				string oname = null, oprotection = null;
				if (collisions != null && collisions.ContainsKey (method.Name)) {
					oname = method.Name;
					oprotection = method.Protection;
					method.Name = QualifiedName + "." + method.Name;
					method.Protection = "";
				}
				method.Generate (gen_info, implementor);
				if (oname != null) {
					method.Name = oname;
					method.Protection = oprotection;
				}
			}
		}

		public Method GetMethod (string name)
		{
			Method m = null;
			methods.TryGetValue (name, out m);
			return m;
		}

		public Property GetProperty (string name)
		{
			Property prop = null;
			props.TryGetValue (name, out prop);
			return prop;
		}

		public Method GetMethodRecursively (string name)
		{
			return GetMethodRecursively (name, false);
		}
		
		public virtual Method GetMethodRecursively (string name, bool check_self)
		{
			Method p = null;
			if (check_self)
				p = GetMethod (name);
			if (p == null && Parent != null) 
				p = Parent.GetMethodRecursively (name, true);
			
			if (check_self && p == null) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.Table.GetClassGen (iface);
					if (igen == null)
						continue;
					p = igen.GetMethodRecursively (name, true);
					if (p != null)
						break;
				}
			}

			return p;
		}

		public virtual Property GetPropertyRecursively (string name)
		{
			ClassBase klass = this;
			Property p = null;
			while (klass != null && p == null) {
				p = (Property) klass.GetProperty (name);
				klass = klass.Parent;
			}
			if (p == null) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.Table.GetClassGen (iface);
					if (igen == null)
						continue;
					p = igen.GetPropertyRecursively (name);
					if (p != null)
						break;
				}
			}
			return p;
		}

		public bool Implements (string iface)
		{
			if (interfaces.Contains (iface))
				return true;
			else if (Parent != null)
				return Parent.Implements (iface);
			else
				return false;
		}

		public IList<Ctor> Ctors { get { return ctors; } }

		bool HasStaticCtor (string name) 
		{
			if (Parent != null && Parent.HasStaticCtor (name))
				return true;

			foreach (Ctor ctor in Ctors)
				if (ctor.StaticName == name)
					return true;

			return false;
		}

		private void InitializeCtors ()
		{
			if (ctors_initted)
				return;

			if (Parent != null)
				Parent.InitializeCtors ();

			var valid_ctors = new List<Ctor>();
			clash_map = new Dictionary<string, Ctor>();

			foreach (Ctor ctor in ctors) {
				if (clash_map.ContainsKey (ctor.Signature.Types)) {
					Ctor clash = clash_map [ctor.Signature.Types];
					Ctor alter = ctor.Preferred ? clash : ctor;
					alter.IsStatic = true;
					if (Parent != null && Parent.HasStaticCtor (alter.StaticName))
						alter.Modifiers = "new ";
				} else
					clash_map [ctor.Signature.Types] = ctor;

				valid_ctors.Add (ctor);
			}

			ctors = valid_ctors;
			ctors_initted = true;
		}

		protected virtual void GenCtors (GenerationInfo gen_info)
		{
			InitializeCtors ();
			foreach (Ctor ctor in ctors)
				ctor.Generate (gen_info);
		}

		public virtual void Finish (StreamWriter sw, string indent)
		{
		}

		public virtual void Prepare (StreamWriter sw, string indent)
		{
		}
	}
}
