// GtkSharp.Generation.StructGen.cs - The Structure Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class StructGen : StructBase, IGeneratable  {
		
		public StructGen (XmlElement ns, XmlElement elem) : base (ns, elem) {}
		
		public new string MarshalType {
			get
			{
				return QualifiedName;
			}
		}
		
		public override String CallByName (String var_name)
		{
			return var_name;
		}
		
		public override String FromNative(String var)
		{
			return var;
		}

		public void Generate ()
		{
			StreamWriter sw = CreateWriter ();
			
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Collections;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();
			
			sw.WriteLine("\t\t/// <summary> " + Name + " Struct </summary>");
			sw.WriteLine("\t\t/// <remarks>");
			sw.WriteLine("\t\t/// </remarks>");

			sw.WriteLine ("\t[StructLayout(LayoutKind.Sequential)]");
			sw.WriteLine ("\tpublic class " + Name + " {");
			sw.WriteLine ();
				
			foreach (XmlNode node in Elem.ChildNodes) {
				if (!(node is XmlElement)) continue;
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
					Statistics.IgnoreCount++;
					break;
					
				case "method":
					Statistics.IgnoreCount++;
					break;
					
				default:
					Console.WriteLine ("Unexpected node");
					break;
				}		
			}
			
			AppendCustom(sw);
			
			sw.WriteLine ("\t}");
			CloseWriter (sw);
			Statistics.StructCount++;
		}		
	}
}

