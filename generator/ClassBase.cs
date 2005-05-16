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
	using System.IO;
	using System.Xml;

	public abstract class ClassBase : GenBase {
		protected Hashtable props = new Hashtable();
		protected Hashtable fields = new Hashtable();
		protected Hashtable sigs = new Hashtable();
		protected Hashtable methods = new Hashtable();
		protected ArrayList interfaces = new ArrayList();
		protected ArrayList managed_interfaces = new ArrayList();
		protected ArrayList ctors = new ArrayList();

		private bool ctors_initted = false;
		private Hashtable clash_map;
		private bool deprecated = false;
		private bool isabstract = false;

		public Hashtable Methods {
			get {
				return methods;
			}
		}	

		public Hashtable Signals {
			get {
				return sigs;
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
					
			if (elem.HasAttribute ("deprecated"))
				deprecated = elem.GetAttribute ("deprecated") == "1";
			if (elem.HasAttribute ("abstract"))
				isabstract = elem.GetAttribute ("abstract") == "1";

			foreach (XmlNode node in elem.ChildNodes) {
				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;
				if (member.HasAttribute ("hidden"))
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
					name = member.GetAttribute("name");
					while (fields.ContainsKey (name))
						name += "mangled";
					fields.Add (name, new ObjectField (member, this));
					break;

				case "signal":
					name = member.GetAttribute("name");
					while (sigs.ContainsKey(name))
						name += "mangled";
					sigs.Add (name, new Signal (member, this));
					break;

				case "implements":
					ParseImplements (member);
					break;

				case "constructor":
					ctors.Add (new Ctor (member, this));
					break;

				default:
					break;
				}
			}
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

		protected bool IsNodeNameHandled (string name)
		{
			switch (name) {
			case "method":
			case "property":
			case "field":
			case "signal":
			case "implements":
			case "constructor":
			case "disabledefaultconstructor":
				return true;
				
			default:
				return false;
			}
		}

		public override string MarshalType {
			get {
				return "IntPtr";
			}
		}

		public override string CallByName (string name)
		{
			return name + " == null ? IntPtr.Zero : " + name + ".Handle";
		}

		public virtual string CallByName ()
		{
			return "Handle";
		}

		public virtual string AssignToName {
			get { return "Raw"; }
		}

		public override string FromNative(string var)
		{
			return "GLib.Object.GetObject(" + var + ") as " + QualifiedName;
		}
		
		protected void GenProperties (GenerationInfo gen_info)
		{		
			if (props.Count == 0)
				return;

			foreach (Property prop in props.Values) {
				if (prop.Validate ())
					prop.Generate (gen_info, "\t\t");
				else
					Console.WriteLine("in Object " + QualifiedName);
			}
		}

		public void GenSignals (GenerationInfo gen_info, ClassBase implementor)
		{		
			if (sigs == null)
				return;

			foreach (Signal sig in sigs.Values) {
				if (sig.Validate ())
					sig.Generate (gen_info, implementor);
				else
					Console.WriteLine("in Object " + QualifiedName);
			}
		}

		protected void GenFields (GenerationInfo gen_info)
		{
			foreach (ObjectField field in fields.Values) {
				if (field.Validate ())
					field.Generate (gen_info, "\t\t");
				else
					Console.WriteLine("in Object " + QualifiedName);
			}
		}

		private void ParseImplements (XmlElement member)
		{
			foreach (XmlNode node in member.ChildNodes) {
				if (node.Name != "interface")
					continue;
				XmlElement element = (XmlElement) node;
				if (element.HasAttribute ("cname"))
					interfaces.Add (element.GetAttribute ("cname"));
				else if (element.HasAttribute ("name"))
					managed_interfaces.Add (element.GetAttribute ("name"));
			}
		}
		
		protected bool IgnoreMethod (Method method)
		{	
			string mname = method.Name;
			return ((method.IsSetter || (method.IsGetter && mname.StartsWith("Get"))) &&
				((props != null) && props.ContainsKey(mname.Substring(3)) ||
				 (fields != null) && fields.ContainsKey(mname.Substring(3))));
		}

		public void GenMethods (GenerationInfo gen_info, Hashtable collisions, ClassBase implementor)
		{		
			if (methods == null)
				return;

			foreach (Method method in methods.Values) {
				if (IgnoreMethod (method))
				    	continue;

				if (method.Validate ())
				{
					string oname = null, oprotection = null;
					if (collisions != null && collisions.Contains (method.Name))
					{
						oname = method.Name;
						oprotection = method.Protection;
						method.Name = Name + "." + method.Name;
						method.Protection = "";
					}
					method.Generate (gen_info, implementor);
					if (oname != null)
					{
						method.Name = oname;
						method.Protection = oprotection;
					}
				}
				else
					Console.WriteLine("in Object " + QualifiedName);
			}
		}

		public Method GetMethod (string name)
		{
			return (Method) methods[name];
		}

		public Property GetProperty (string name)
		{
			return (Property) props[name];
		}

		public Signal GetSignal (string name)
		{
			return (Signal) sigs[name];
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

			return p;
		}

		public Signal GetSignalRecursively (string name)
		{
			return GetSignalRecursively (name, false);
		}
		
		public virtual Signal GetSignalRecursively (string name, bool check_self)
		{
			Signal p = null;
			if (check_self)
				p = GetSignal (name);
			if (p == null && Parent != null) 
				p = Parent.GetSignalRecursively (name, true);
			
			if (check_self && p == null) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.Table.GetClassGen (iface);
					p = igen.GetSignalRecursively (name, true);
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

		public ArrayList Ctors { get { return ctors; } }

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

			ArrayList valid_ctors = new ArrayList();
			clash_map = new Hashtable();

			foreach (Ctor ctor in ctors) {
				if (ctor.Validate ()) {
					if (clash_map.Contains (ctor.Signature.Types)) {
						Ctor clash = clash_map [ctor.Signature.Types] as Ctor;
						Ctor alter = ctor.Preferred ? clash : ctor;
						alter.IsStatic = true;
						if (Parent != null && Parent.HasStaticCtor (alter.StaticName))
							alter.Modifiers = "new ";
					} else
						clash_map [ctor.Signature.Types] = ctor;

					valid_ctors.Add (ctor);
				} else
					Console.WriteLine("in Type " + QualifiedName);
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

	}
}
