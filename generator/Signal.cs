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
		private ClassBase container_type;

		public Signal (XmlElement elem, ClassBase container_type)
		{
			this.elem = elem;
			this.name = elem.GetAttribute ("name");
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
			marsh = SignalHandler.GetName(elem);
			if ((Name == "") || (marsh == "")) {
				Console.Write ("bad signal " + Name);
				Statistics.ThrottledCount++;
				return false;
			}
			
			return true;
		}

		public void GenerateDecl (StreamWriter sw)
		{
			GenComments (sw);
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null))
				sw.Write("new ");

			sw.WriteLine ("\t\tevent EventHandler " + Name + ";");
		}

		public void GenComments (StreamWriter sw)
		{
			sw.WriteLine();
			sw.WriteLine("\t\t/// <summary> " + Name + " Event </summary>");
			sw.WriteLine("\t\t/// <remarks>");
			// FIXME: Generate some signal docs
			sw.WriteLine("\t\t/// </remarks>");
		}
		
		public void Generate (StreamWriter sw, ClassBase implementor, bool gen_docs)
		{
			string cname = "\"" + elem.GetAttribute("cname") + "\"";
			marsh = "GtkSharp." + marsh;

			if (gen_docs)
				GenComments (sw);
			sw.Write("\t\tpublic ");
			if (elem.HasAttribute("new_flag") || (container_type != null && container_type.GetSignalRecursively (Name) != null) || (implementor != null && implementor.GetSignalRecursively (Name) != null))
				sw.Write("new ");
			sw.WriteLine("event EventHandler " + Name + " {");
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
		}
	}
}

