// GtkSharp.Generation.ObjectGen.cs - The Object Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class ObjectGen : ClassBase, IGeneratable  {

		private ArrayList ctors = new ArrayList();

		public ObjectGen (string ns, XmlElement elem) : base (ns, elem) 
		{
			foreach (XmlNode node in elem.ChildNodes) {

				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
				case "callback":
					Statistics.IgnoreCount++;
					break;

				case "constructor":
					ctors.Add (new Ctor (ns, member));
					break;
					
				default:
					if (!IsNodeNameHandled (node.Name))
						Console.WriteLine ("Unexpected node " + node.Name + " in " + CName);
					break;
				}
			}
		}

		private ObjectGen Parent {
			get {
				string parent = Elem.GetAttribute("parent");
				return (ObjectGen) SymbolTable.GetClassGen(parent);
			}
		}

		public void Generate ()
		{
			StreamWriter sw = CreateWriter ();

			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();

			sw.Write ("\tpublic class " + Name);
			string cs_parent = SymbolTable.GetCSType(Elem.GetAttribute("parent"));
			if (cs_parent != "")
				sw.Write (" : " + cs_parent);
			if (interfaces != null) {
				foreach (string iface in interfaces) {
					sw.Write (", " + SymbolTable.GetCSType (iface));
				}
			}
			sw.WriteLine (" {");
			sw.WriteLine ();

			GenCtors (sw);
			GenProperties (sw);
			GenSignals (sw);
			GenMethods (sw);
			
			if (interfaces != null) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.GetClassGen (iface);
					igen.GenMethods (sw);
				}
			}

			AppendCustom(Namespace, Name, sw);

			sw.WriteLine ("\t}");

			CloseWriter (sw);
			Statistics.ObjectCount++;
		}

		private bool Validate ()
		{
			string parent = Elem.GetAttribute("parent");
			string cs_parent = SymbolTable.GetCSType(parent);
			if (cs_parent == "") {
				Console.WriteLine ("Object " + Name + " Unknown parent " + parent);
				return false;
			}

			if (ctors != null)
				foreach (Ctor ctor in ctors)
					if (!ctor.Validate())
						return false;

			if (props != null)
				foreach (Property prop in props.Values)
					if (!prop.Validate())
						return false;

			if (sigs != null)
				foreach (Signal sig in sigs.Values)
					if (!sig.Validate())
						return false;

			if (methods != null)
				foreach (Method method in methods.Values)
					if (!method.Validate())
						return false;

			if (interfaces != null) {
				foreach (string iface in interfaces) {
					if (SymbolTable.GetCSType(parent) == null)
						return false;
				}
			}

			return true;
		}

		private void GenCtors (StreamWriter sw)
		{
			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
			sw.WriteLine();

			Hashtable clash_map = new Hashtable();

			if (ctors != null)
				foreach (Ctor ctor in ctors) {
					if (ctor.Validate ())
						ctor.Generate (sw, clash_map);
					else
						Console.WriteLine(" in Object " + Name);
				}

			if (!clash_map.ContainsKey("")) {
				sw.WriteLine("\t\tprotected " + Name + "() : base(){}");
				sw.WriteLine();
			}

		}
	}
}

