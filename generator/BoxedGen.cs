// GtkSharp.Generation.BoxedGen.cs - The Boxed Type Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class BoxedGen : StructBase, IGeneratable  {
		
		public BoxedGen (String ns, XmlElement elem) : base (ns, elem) {}
		
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
			return "(" + QualifiedName + ") GLib.Boxed.FromNative(" + var + ")";
		}

		public void Generate ()
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
			
			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic class " + Name + " : GLib.Boxed {");
			sw.WriteLine ();
				
			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
			sw.WriteLine();
				
			Hashtable clash_map = new Hashtable();
				
			foreach (XmlNode node in elem.ChildNodes) {
				
				XmlElement member = (XmlElement) node;

				switch (node.Name) {
				case "field":
					Statistics.IgnoreCount++;
					// GenField(member, sw);
					break;
					
				case "callback":
					Statistics.IgnoreCount++;
					break;
					
				case "constructor":
					if (!GenCtor(member, sw, clash_map)) {
						Console.WriteLine(" in boxed " + CName);
					}
					break;
					
				case "method":
					if (!GenMethod(member, sw)) {
						Console.WriteLine(" in boxed " + CName);
					}
					break;
					
				default:
					Console.WriteLine ("Unexpected node");
					break;
				}		
			}
			
			sw.WriteLine ("\t}");
			sw.WriteLine ();
			sw.WriteLine ("}");
			
			sw.Flush();
			sw.Close();
			Statistics.BoxedCount++;
		}		
	}
}

