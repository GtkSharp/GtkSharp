// GtkSharp.Generation.InterfaceGen.cs - The Interface Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public class InterfaceGen : GenBase, IGeneratable  {

		public InterfaceGen (string ns, XmlElement elem) : base (ns, elem) {}

		public String MarshalType {
			get
			{
				return "";
			}
		}

		public String CallByName (String var_name)
		{
			return "";
		}

		public String FromNative(String var)
		{
			return "";
		}

		public void Generate ()
		{
			StreamWriter sw = CreateWriter ();

			sw.WriteLine ("\tusing System;");
			sw.WriteLine ();

			sw.WriteLine ("\tpublic interface " + Name + " {");
			sw.WriteLine ();

			foreach (XmlNode node in Elem.ChildNodes) {
				if (node.Name != "member") {
					continue;
				}
				//FIXME: Generate the methods.
				XmlElement member = (XmlElement) node;
			}

			sw.WriteLine ("\t}");
			CloseWriter (sw);
			Statistics.IFaceCount++;
		}
		
	}
}

