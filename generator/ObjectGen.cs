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
		private ArrayList strings = new ArrayList();
		private bool ctors_initted = false;
		private Hashtable clash_map;

		public ObjectGen (XmlElement ns, XmlElement elem) : base (ns, elem) 
		{
			foreach (XmlNode node in elem.ChildNodes) {

				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
				case "callback":
					Statistics.IgnoreCount++;
					break;

				case "constructor":
					ctors.Add (new Ctor (LibraryName, member, this));
					break;
					
				case "static-string":
					strings.Add (node);
					break;

				default:
					if (!IsNodeNameHandled (node.Name))
						Console.WriteLine ("Unexpected node " + node.Name + " in " + CName);
					break;
				}
			}
		}

		public void Generate ()
		{
			StreamWriter sw = CreateWriter ();

			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();

			sw.WriteLine("\t\t/// <summary> " + Name + " Class</summary>");
			sw.WriteLine("\t\t/// <remarks>");
			sw.WriteLine("\t\t/// </remarks>");
			sw.Write ("\tpublic class " + Name);
			string cs_parent = SymbolTable.GetCSType(Elem.GetAttribute("parent"));
			if (cs_parent != "")
				sw.Write (" : " + cs_parent);
			if (interfaces != null) {
				foreach (string iface in interfaces) {
					if (Parent != null && Parent.Implements (iface))
						continue;
					sw.Write (", " + SymbolTable.GetCSType (iface));
				}
			}
			sw.WriteLine (" {");
			sw.WriteLine ();

			GenCtors (sw);
			GenProperties (sw);
			
			bool has_sigs = (sigs != null);
			if (!has_sigs) {
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.GetClassGen (iface);
					if (igen.Signals != null) {
						has_sigs = true;
						break;
					}
				}
			}

			if (has_sigs && Elem.HasAttribute("parent"))
			{
				sw.WriteLine("\t\tprivate Hashtable Signals = new Hashtable();");
				GenSignals (sw, null, true);
			}

			GenMethods (sw, null, null, true);
			
			if (interfaces != null) {
				Hashtable all_methods = new Hashtable ();
				Hashtable collisions = new Hashtable ();
				foreach (string iface in interfaces) {
					ClassBase igen = SymbolTable.GetClassGen (iface);
					foreach (Method m in igen.Methods.Values) {
						if (all_methods.Contains (m.Name))
							collisions[m.Name] = true;
						else
							all_methods[m.Name] = true;
					}
				}
					
				foreach (string iface in interfaces) {
					if (Parent != null && Parent.Implements (iface))
						continue;
					ClassBase igen = SymbolTable.GetClassGen (iface);
					igen.GenMethods (sw, collisions, this, false);
					igen.GenSignals (sw, this, false);
				}
			}

			foreach (XmlElement str in strings) {
				sw.Write ("\t\tpublic static string " + str.GetAttribute ("name"));
				sw.WriteLine (" {\n\t\t\t get { return \"" + str.GetAttribute ("value") + "\"; }\n\t\t}");
			}
			
			AppendCustom(sw);

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

			if (SymbolTable.GetCSType(parent) == null)
				return false;

			return true;
		}

		public ArrayList Ctors { get { return ctors; } }

		private void InitializeCtors ()
		{
			if (ctors_initted)
				return;
			
			clash_map = new Hashtable();

			if (ctors != null) {
				bool has_preferred = false;
				foreach (Ctor ctor in ctors) {
					if (ctor.Validate ()) {
						ctor.InitClashMap (clash_map);
						if (ctor.Preferred)
							has_preferred = true;
					}
					else
						Console.WriteLine(" in Object " + Name);
				}
				
				if (!has_preferred && ctors.Count > 0)
					((Ctor) ctors[0]).Preferred = true;

				foreach (Ctor ctor in ctors) 
					if (ctor.Validate ()) {
						ctor.Initialize (clash_map);
					}
			}

			ctors_initted = true;
		}

		private void GenCtors (StreamWriter sw)
		{
			if (!Elem.HasAttribute("parent"))
				return;
			ObjectGen klass = this;
			while (klass != null) {
				klass.InitializeCtors ();
				klass = (ObjectGen) klass.Parent;
			}
			
			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
			sw.WriteLine();

			if (ctors != null) {
				foreach (Ctor ctor in ctors) {
					if (ctor.Validate ())
						ctor.Generate (sw);
				}
			}

			if (!clash_map.ContainsKey("")) {
				sw.WriteLine("\t\tprotected " + Name + "() : base(){}");
				sw.WriteLine();
			}

		}
	}
}

