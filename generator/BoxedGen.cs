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
		
		public BoxedGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
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
			StreamWriter sw = CreateWriter ();
				
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			
			sw.WriteLine("\t\t/// <summary> " + Name + " Boxed Struct</summary>");
			sw.WriteLine("\t\t/// <remarks>");
			sw.WriteLine("\t\t/// </remarks>");

			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic class " + Name + " : GLib.Boxed {");
			sw.WriteLine ();
				
			sw.WriteLine("\t\tpublic " + Name + "(IntPtr raw) : base(raw) {}");
			sw.WriteLine();
				
			Hashtable clash_map = new Hashtable();
				
			foreach (XmlNode node in Elem.ChildNodes) {
				
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
			
			AppendCustom(sw);
			sw.WriteLine ("\t}");
			CloseWriter (sw);
			Statistics.BoxedCount++;
		}		
	}
}

