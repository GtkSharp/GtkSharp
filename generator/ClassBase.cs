// GtkSharp.Generation.ClassBase.cs - Common code between object
// and interface wrappers
//
// Authors: Rachel Hestilow <hestilow@ximian.com>
//          Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Rachel Hestilow, 2001-2002 Mike Kestner 

namespace GtkSharp.Generation {
	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class ClassBase : GenBase {
		protected Hashtable props = new Hashtable();
		protected Hashtable sigs = new Hashtable();
		protected Hashtable methods = new Hashtable();
		protected ArrayList interfaces = null;

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
				return SymbolTable.GetClassGen(parent);
			}
		}

		protected ClassBase (XmlElement ns, XmlElement elem) : base (ns, elem) {
			foreach (XmlNode node in elem.ChildNodes) {
				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;
				if (member.HasAttribute ("hidden"))
					continue;

				switch (node.Name) {
				case "method":
					string name = member.GetAttribute("name");
					while (methods.ContainsKey(name))
						name += "mangled";
					methods.Add (name, new Method (LibraryName, member, this));
					break;

				case "property":
					props.Add (member.GetAttribute ("name"), new Property (member, this));
					break;

				case "signal":
					sigs.Add (member.GetAttribute ("name"), new Signal (member, this));
					break;

				case "implements":
					interfaces = ParseImplements (member);
					break;

				default:
					break;
				}
			}
		}

		protected bool IsNodeNameHandled (string name)
		{
			switch (name) {
			case "method":
			case "property":
			case "signal":
			case "implements":
				return true;
				
			default:
				return false;
			}
		}

		public virtual String MarshalType {
			get
			{
				return "IntPtr";
			}
		}

		public virtual String CallByName (String var_name)
		{
			return var_name + ".Handle";
		}

		public virtual String FromNative(String var)
		{
			return "(" + QualifiedName + ") GLib.Object.GetObject(" + var + ")";
		}

		protected void GenProperties (StreamWriter sw)
		{		
			if (props == null)
				return;

			foreach (Property prop in props.Values) {
				if (prop.Validate ())
					prop.Generate (sw);
				else
					Console.WriteLine(" in Object " + Name);
			}
		}

		public void GenSignals (StreamWriter sw, ClassBase implementor, bool gen_docs)
		{		
			if (sigs == null)
				return;

			foreach (Signal sig in sigs.Values) {
				if (sig.Validate ())
					sig.Generate (sw, implementor, gen_docs);
				else
					Console.WriteLine(" in Object " + Name);
			}
		}

		private ArrayList ParseImplements (XmlElement member)
		{
			ArrayList ifaces = new ArrayList ();
			
			foreach (XmlNode node in member.ChildNodes) {
				if (node.Name != "interface")
					continue;
				XmlElement element = (XmlElement) node;
				ifaces.Add (element.GetAttribute ("cname"));
			}

			return ifaces;
		}
		
		protected bool IgnoreMethod (Method method)
		{	
			string mname = method.Name;
			return ((mname.StartsWith("Set") || mname.StartsWith("Get")) &&
				     (props != null) && props.ContainsKey(mname.Substring(3)));
		}

		public void GenMethods (StreamWriter sw, Hashtable collisions, ClassBase implementor, bool gen_docs)
		{		
			if (methods == null)
				return;

			foreach (Method method in methods.Values) {
				if (IgnoreMethod (method))
				    	continue;

				if (method.Validate ())
				{
					String oname = null, oprotection = null;
					if (collisions != null && collisions.Contains (method.Name))
					{
						oname = method.Name;
						oprotection = method.Protection;
						method.Name = Name + "." + method.Name;
						method.Protection = "";
					}
					method.Generate (sw, implementor, gen_docs);
					if (oname != null)
					{
						method.Name = oname;
						method.Protection = oprotection;
					}
				}
				else
					Console.WriteLine(" in Object " + Name);
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
			
			if (check_self && p == null && interfaces != null) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.GetClassGen (iface);
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
			
			if (check_self && p == null && interfaces != null) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.GetClassGen (iface);
					p = igen.GetSignalRecursively (name, true);
					if (p != null)
						break;
				}
			}

			return p;
		}

		public bool Implements (string iface)
		{
			if (interfaces != null)
				return interfaces.Contains (iface);
			else
				return false;
		}
	}
}
