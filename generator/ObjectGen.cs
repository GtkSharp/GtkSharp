// GtkSharp.Generation.ObjectGen.cs - The Object Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class ObjectGen : StructBase, IGeneratable  {
		
		public ObjectGen (String ns, XmlElement elem) : base (ns, elem) {}
		
		public String MarshalType {
			get
			{
				return "IntPtr";
			}
		}
		
		public String CallByName (String var_name)
		{
			return var_name + ".Handle";
		}
		
		public String FromNative(String var)
		{
			return "(" + QualifiedName + ") GLib.Object.GetObject(" + var + ")";
		}
		
		public void Generate (SymbolTable table)
		{
			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + ns.ToLower() + sep + "generated";
			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}
			String filename = dir + sep + Name + ".cs";
			
			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ();
				
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();

			String parent = elem.GetAttribute("parent");
			String cs_parent = table.GetCSType(parent);
			sw.Write ("\tpublic class " + Name);
			if (cs_parent == "") {
				sw.WriteLine (" {");
				Console.WriteLine ("Object " + Name + " Unknown parent " + parent);
			} else {
				sw.WriteLine (" : " + cs_parent + " {");
			}
			sw.WriteLine ();
				
			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
			sw.WriteLine();
				
			Hashtable clash_map = new Hashtable();
			Hashtable props = new Hashtable();
			Hashtable sigs = new Hashtable();
			ArrayList methods = new ArrayList();
			bool first_sig = true;
				
			foreach (XmlNode node in elem.ChildNodes) {
				
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					Statistics.IgnoreCount++;
					break;
					
				case "callback":
					Statistics.IgnoreCount++;
					break;
					
				case "constructor":
					if (!GenCtor(member, table, sw, clash_map)) {
						Console.WriteLine("in object " + CName);
					}
					break;
					
				case "method":
					methods.Add(member);
					break;
					
				case "property":
					String pname;
					if (!GenProperty(member, table, sw, out pname)) {
						Console.WriteLine("in object " + CName);
					}
					props.Add(pname, pname);
					break;
					
				case "signal":
					if (first_sig) {
						first_sig = false;
						sw.WriteLine("\t\tprivate Hashtable Signals = new Hashtable();");
					}
					String sname;
					if (!GenSignal(member, table, sw, out sname)) {
						Console.WriteLine("in object " + CName);
					}
					sigs.Add(sname, sname);
					break;
					
				default:
					Console.WriteLine ("Unexpected node");
					break;
				}
				
			}
			
			if (!clash_map.ContainsKey("")) {
				sw.WriteLine("\t\tprotected " + Name + "() : base(){}");
				sw.WriteLine();
			}
			
			foreach (XmlElement member in methods) {
				String mname = member.GetAttribute("name");
				if ((mname.StartsWith("Set") || mname.StartsWith("Get")) &&
				    props.ContainsKey(mname.Substring(3))) {
				    	continue;
				} else if (sigs.ContainsKey(mname)) {
					member.SetAttribute("name", "Emit" + mname);
				}
				
				if (!GenMethod(member, table, sw)) {
					Console.WriteLine("in object " + CName);
				}
			}

			string custom = ".." + sep + ns.ToLower() + sep + Name + ".custom";
			if (File.Exists(custom)) {
				FileStream custstream = new FileStream (custom, FileMode.Open, FileAccess.Read);
				StreamReader sr = new StreamReader (custstream);
				sw.WriteLine (sr.ReadToEnd ());
				sr.Close ();
			}
			
			sw.WriteLine ("\t}");
			sw.WriteLine ();
			sw.WriteLine ("}");
			
			sw.Flush();
			sw.Close();
			Statistics.ObjectCount++;
		}
		
		public bool GenProperty (XmlElement prop, SymbolTable table, StreamWriter sw, out String name)
		{
			String c_type = prop.GetAttribute("type");

			char[] ast = {'*'};
			c_type = c_type.TrimEnd(ast);
			string cs_type = table.GetCSType(c_type);
			
			XmlElement parent = (XmlElement) prop.ParentNode;
			name = prop.GetAttribute("name");
			if (name == parent.GetAttribute("name")) {
				name += "Prop";
			}

			string v_type = "";
			if (table.IsEnum(c_type)) {
				v_type = "int";
			} else if (table.IsInterface(c_type)) {
				// FIXME: Handle interface props properly.
				Console.Write("Interface property detected ");
				Statistics.ThrottledCount++;
				return true;
			}
			
			if (cs_type == "") {
				Console.Write("Property has unknown Type {0} ", c_type);
				Statistics.ThrottledCount++;
				return false;
			}
			
			if (prop.HasAttribute("construct-only") && !prop.HasAttribute("readable")) {
				return true;
			}
			
			sw.WriteLine("\t\tpublic " + cs_type + " " + name + " {");
			if (prop.HasAttribute("readable")) {
				sw.WriteLine("\t\t\tget {");
				sw.WriteLine("\t\t\t\tGLib.Value val;");
				sw.WriteLine("\t\t\t\tGetProperty(\"" + prop.GetAttribute("cname") + "\", out val);");
				sw.Write("\t\t\t\treturn (" + cs_type + ") ");
				if (v_type != "") {
					sw.Write("(" + v_type + ") ");
				}
				sw.WriteLine("val;");
				sw.WriteLine("\t\t\t}");
			}
			
			if (prop.HasAttribute("writeable") && !prop.HasAttribute("construct-only")) {
				sw.WriteLine("\t\t\tset {");
				sw.Write("\t\t\t\tSetProperty(\"" + prop.GetAttribute("cname") + "\", new GLib.Value(");
				if (v_type != "") {
					sw.Write("(" + v_type + ") ");
				}
				sw.WriteLine("value));");
				sw.WriteLine("\t\t\t}");
			}
			
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.PropCount++;
			return true;
		}

		public bool GenSignal (XmlElement sig, SymbolTable table, StreamWriter sw, out String name)
		{
			String cname = "\"" + sig.GetAttribute("cname") + "\"";
			name = sig.GetAttribute("name");

			String marsh = SignalHandler.GetName(sig, table);
			if (marsh == "") {
				Statistics.ThrottledCount++;
				return false;
			}
			
			marsh = "GtkSharp." + marsh;

			sw.WriteLine("\t\t/// <summary> " + name + " Event </summary>");
			sw.WriteLine("\t\t/// <remarks>");
			// FIXME: Generate some signal docs
			sw.WriteLine("\t\t/// </remarks>");
			sw.WriteLine();
			sw.WriteLine("\t\tpublic event EventHandler " + name + " {");
			sw.WriteLine("\t\t\tadd {");
			sw.WriteLine("\t\t\t\tif (EventList[" + cname + "] == null)");
			sw.Write("\t\t\t\t\tSignals[" + cname + "] = new " + marsh);
			sw.WriteLine("(this, Handle, " + cname + ", value);");
			sw.WriteLine("\t\t\t\tEventList.AddHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\tremove {");
			sw.WriteLine("\t\t\t\tEventList.RemoveHandler(" + cname + ", value);");
			sw.WriteLine("\t\t\t\tif (EventList[" + cname + "] == null)");
			sw.WriteLine("\t\t\t\t\tSignals.Remove(" + cname + ");");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			
			Statistics.SignalCount++;
			return true;
		}
	}
}

