// GtkSharp.Generation.Signal.cs - The Signal Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Signal {

		private string marsh;
		private string name;
		private XmlElement elem;
		private Parameters parms;
		private ClassBase container_type;

		public Signal (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			this.name = elem.GetAttribute ("name");
			if (elem["parameters"] != null)
				parms = new Parameters (elem["parameters"]);
			this.container_type = container_type;
		}

		public string Name {
			get {
				return name; 
			}
			set {
				name = value;
			}
		}

		public bool Validate ()
		{
			marsh = SignalHandler.GetName(elem, container_type.NS, container_type.DoGenerate);
			if ((Name == "") || (marsh == "")) {
				Console.Write ("bad signal " + Name);
				Statistics.ThrottledCount++;
				return false;
			}
			
			if (parms != null && !parms.Validate ())
				return false;

			return true;
		}

 		public void GenerateDecl (StreamWriter sw)
 		{
			string argsname;
			string handler = GetHandlerName (out argsname);
			if (handler != "EventHandler")
				handler = container_type.NS + "Sharp." + handler;

			GenComments (sw);
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null))
				sw.Write("new ");

 			sw.WriteLine ("\t\tevent " + handler + " " + Name + ";");
		}

		private void GenComments (StreamWriter sw)
		{
			sw.WriteLine();
			sw.WriteLine("\t\t/// <summary> " + Name + " Event </summary>");
			sw.WriteLine("\t\t/// <remarks>");
			// FIXME: Generate some signal docs
			sw.WriteLine("\t\t/// </remarks>");
		}

		private string GetHandlerName (out string argsname)
		{
			if (marsh.EndsWith (".voidObjectSignal")) {
				argsname = "EventArgs";
				return "EventHandler";
			}

			argsname = Name + "Args";
			return Name + "Handler";
		}

		private string GenHandler (out string argsname)
		{
			string handler = GetHandlerName (out argsname);
			if (handler == "EventHandler" || !container_type.DoGenerate)
				return handler;

			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + container_type.NS.ToLower() + sep + "generated";

			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			string filename = dir + sep + container_type.NS + "Sharp." + handler + ".cs";

			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			sw.WriteLine ("namespace " + container_type.NS + "Sharp {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");

			sw.WriteLine ();
			sw.WriteLine ("\t/// <summary> " + handler + " Delegate </summary>");
			sw.WriteLine ("\t/// <remarks>");
			sw.WriteLine ("\t///\tDelegate signature for " + Name + " Event handlers");
			sw.WriteLine ("\t/// </remarks>");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic delegate void " + handler + "(object o, " + argsname + " args);");
			sw.WriteLine ();
			sw.WriteLine ("\t/// <summary> " + argsname + " Class </summary>");
			sw.WriteLine ("\t/// <remarks>");
			sw.WriteLine ("\t///\tArguments for " + Name + " Event handlers");
			sw.WriteLine ("\t/// </remarks>");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic class " + argsname + " : GtkSharp.SignalArgs {");
			if (parms != null) {
				for (int i = 1; i < parms.Count; i++) {
					sw.WriteLine ("\t\tpublic " + parms[i].CSType + " " + parms[i].StudlyName + "{");
					sw.WriteLine ("\t\t\tget {");
					sw.WriteLine ("\t\t\t\treturn (" + parms[i].CSType + ") Args[" + (i - 1) + "];");
					sw.WriteLine ("\t\t\t}");
					sw.WriteLine ("\t\t}");
					sw.WriteLine ();
				}
			}
			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
			sw.Close ();
			argsname = container_type.NS + "Sharp." + argsname;
			return container_type.NS + "Sharp." + handler;
		}

		public void Generate (StreamWriter sw, ClassBase implementor, bool gen_docs)
		{
			string cname = "\"" + elem.GetAttribute("cname") + "\"";
			string qual_marsh = marsh;

			if (gen_docs)
				GenComments (sw);

			string argsname;
			string handler = GenHandler (out argsname);

			sw.WriteLine("\t\t[GLib.Signal("+ cname + ")]");
			sw.Write("\t\tpublic ");
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null) || (implementor != null && implementor.GetSignalRecursively (Name) != null))
				sw.Write("new ");
			sw.WriteLine("event " + handler + " " + Name + " {");
			sw.WriteLine("\t\t\tadd {");
			sw.WriteLine("\t\t\t\tif (EventList[" + cname + "] == null)");
			sw.Write("\t\t\t\t\tSignals[" + cname + "] = new " + qual_marsh);
			sw.Write("(this, Handle, " + cname + ", value, System.Type.GetType(\"" + argsname);
			if (argsname != "EventArgs")
				sw.Write("," + container_type.NS.ToLower() + "-sharp");
			sw.WriteLine("\"));");
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
		}
	}
}

