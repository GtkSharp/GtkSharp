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

	public class ObjectGen : GenBase, IGeneratable  {

		private ArrayList ctors = new ArrayList();
		private Hashtable props = new Hashtable();
		private Hashtable sigs = new Hashtable();
		private Hashtable methods = new Hashtable();

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

				case "method":
					methods.Add (member.GetAttribute ("name"), new Method (ns, member));
					break;

				case "property":
					props.Add (member.GetAttribute ("name"), new Property (member));
					break;

				case "signal":
					sigs.Add (member.GetAttribute ("name"), new Signal (ns, member));
					break;

				default:
					Console.WriteLine ("Unexpected node " + node.Name + " in " + CName);
					break;
				}
			}
		}

		public string MarshalType {
			get {
				return "IntPtr";
			}
		}

		public string CallByName (string var_name)
		{
			return var_name + ".Handle";
		}

		public string FromNative(string var)
		{
			return "(" + QualifiedName + ") GLib.Object.GetObject(" + var + ")";
		}

		private ObjectGen Parent {
			get {
				string parent = Elem.GetAttribute("parent");
				return SymbolTable.GetObjectGen(parent);
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
			sw.WriteLine (" {");
			sw.WriteLine ();

			GenCtors (sw);
			GenProperties (sw);
			GenSignals (sw);
			GenMethods (sw);
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

		private void GenProperties (StreamWriter sw)
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

		private void GenSignals (StreamWriter sw)
		{		
			if (sigs == null)
				return;

			sw.WriteLine("\t\tprivate Hashtable Signals = new Hashtable();");
			
			foreach (Signal sig in sigs.Values) {
				if (sig.Validate ())
					sig.Generate (sw);
				else
					Console.WriteLine(" in Object " + Name);
			}
		}

		private void GenMethods (StreamWriter sw)
		{		
			if (methods == null)
				return;

			foreach (Method method in methods.Values) {
				string mname = method.Name;
				if ((mname.StartsWith("Set") || mname.StartsWith("Get")) &&
				    (props != null) && props.ContainsKey(mname.Substring(3))) {
				    	continue;
				} else if ((sigs != null) && sigs.ContainsKey(mname)) {
					method.Name = "Emit" + mname;
				}

				if (method.Validate ())
					method.Generate (sw);
				else
					Console.WriteLine(" in Object " + Name);
			}
		}
	}
}

