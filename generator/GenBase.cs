// GtkSharp.Generation.GenBase.cs - The Generatable base class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001-2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.IO;
	using System.Xml;

	public abstract class GenBase {
		
		private XmlElement ns;
		private XmlElement elem;

		protected GenBase (XmlElement ns, XmlElement elem)
		{
			this.ns = ns;
			this.elem = elem;
		}

		public string CName {
			get {
				return elem.GetAttribute ("cname");
			}
		}

		public XmlElement Elem {
			get {
				return elem;
			}
		}

		public string LibraryName {
			get {
				return ns.GetAttribute ("library");
			}
		}

		public string Name {
			get {
				return elem.GetAttribute ("name");
			}
		}

		public string NS {
			get {
				return ns.GetAttribute ("name");
			}
		}

		public XmlElement NSElem {
			get {
				return ns;
			}
		}

		public string QualifiedName {
			get {
				return NS + "." + Name;
			}
		}

		public void AppendCustom (StreamWriter sw, string custom_dir)
		{
			char sep = Path.DirectorySeparatorChar;
			string custom = custom_dir + sep + Name + ".custom";
			if (File.Exists(custom)) {
				sw.WriteLine ("#region Customized extensions");
				sw.WriteLine ("#line 1 \"" + Name + ".custom\"");
				FileStream custstream = new FileStream(custom, FileMode.Open, FileAccess.Read);
				StreamReader sr = new StreamReader(custstream);
				sw.WriteLine (sr.ReadToEnd ());
				sw.WriteLine ("#endregion");
				sr.Close ();
			}
		}
	}
}

