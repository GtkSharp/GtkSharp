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

		private ArrayList strings = new ArrayList();
		private static Hashtable objs = new Hashtable();

		public ObjectGen (XmlElement ns, XmlElement elem) : base (ns, elem) 
		{
			objs.Add (CName, QualifiedName + "," + NS.ToLower() + "-sharp");
 
			foreach (XmlNode node in elem.ChildNodes) {

				if (!(node is XmlElement)) continue;
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
				case "callback":
					Statistics.IgnoreCount++;
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
				Console.WriteLine ("Object " + QualifiedName + " Unknown parent " + parent);
				return false;
			}

			if (ctors != null)
				foreach (Ctor ctor in ctors)
					if (!ctor.Validate()) {
						Console.WriteLine ("in Object " + QualifiedName);
						return false;
					}

			if (props != null)
				foreach (Property prop in props.Values)
					if (!prop.Validate()) {
						Console.WriteLine ("in Object " + QualifiedName);
						return false;
					}

			if (sigs != null)
				foreach (Signal sig in sigs.Values)
					if (!sig.Validate()) {
						Console.WriteLine ("in Object " + QualifiedName);
						return false;
					}

			if (methods != null)
				foreach (Method method in methods.Values)
					if (!method.Validate()) {
						Console.WriteLine ("in Object " + QualifiedName);
						return false;
					}

			if (SymbolTable.GetCSType(parent) == null)
				return false;

			return true;
		}
		
		protected override void GenCtors (StreamWriter sw)
		{
			if (!Elem.HasAttribute("parent"))
				return;

			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
			sw.WriteLine();

			base.GenCtors (sw);
		}

		public static void GenerateMapper ()
		{
			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + "glib" + sep + "generated";
			if (!Directory.Exists(dir)) {
        			Console.WriteLine ("creating " + dir);
        			Directory.CreateDirectory(dir);
			}
			String filename = dir + sep + "ObjectManager.cs";

			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);

			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();

			sw.WriteLine ("namespace GtkSharp {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic class ObjectManager {");
			sw.WriteLine ();
			sw.WriteLine ("\t\tprivate static Hashtable types = new Hashtable ();");
			sw.WriteLine ();
			sw.WriteLine ("\t\tstatic ObjectManager ()");
			sw.WriteLine ("\t\t{");

			foreach (string key in objs.Keys) {
				sw.WriteLine ("\t\t\t\ttypes.Add(\"" + key + "\", \"" + objs[key] + "\");");
			}
				
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\t[DllImport(\"gtksharpglue\")]");
			sw.WriteLine ("\t\tstatic extern string gtksharp_get_type_name (IntPtr raw);");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static GLib.Object CreateObject (IntPtr raw)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tif (raw == IntPtr.Zero)");
			sw.WriteLine ("\t\t\t\treturn null;");
			sw.WriteLine ();
			sw.WriteLine ("\t\t\tstring typename = gtksharp_get_type_name (raw);");
			sw.WriteLine ("\t\t\tif (!types.ContainsKey(typename))");
			sw.WriteLine ("\t\t\t\treturn null;");
			sw.WriteLine ();
			sw.WriteLine ("\t\t\tType t = Type.GetType ((string)types[typename]);");
			sw.WriteLine ("\t\t\treturn (GLib.Object) Activator.CreateInstance (t, new object[] {raw});");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static void RegisterType (string native_name, string managed_name, string assembly)");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\ttypes.Add(native_name, managed_name + \",\" + assembly);");
			sw.WriteLine ("\t\t}");
			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
			sw.Flush ();
			sw.Close ();
		}
	}
}

