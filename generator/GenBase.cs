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
		
		private string ns;
		private XmlElement elem;

		protected GenBase (string ns, XmlElement elem)
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

		public string Name {
			get {
				return elem.GetAttribute ("name");
			}
		}

		public string Namespace {
			get {
				return ns;
			}
		}

		public string QualifiedName {
			get {
				return ns + "." + Name;
			}
		}

		protected StreamWriter CreateWriter () 
		{
			char sep = Path.DirectorySeparatorChar;
			string dir = ".." + sep + ns.ToLower() + sep + "generated";
			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}
			String filename = dir + sep + Name + ".cs";
			Console.WriteLine ("creating " + filename);
			
			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			sw.WriteLine ("namespace " + ns + " {");
			sw.WriteLine ();

			return sw;
		}
				
		protected void CloseWriter (StreamWriter sw)
		{
			sw.WriteLine ();
			sw.WriteLine ("}");
			sw.Flush();
			sw.Close();
		}
		
		public static void AppendCustom (string ns, string name, StreamWriter sw)
		{
			char sep = Path.DirectorySeparatorChar;
			string custom = ".." + sep + ns.ToLower() + sep + name + ".custom";
			if (File.Exists(custom)) {
				FileStream custstream = new FileStream(custom, FileMode.Open, FileAccess.Read);
				StreamReader sr = new StreamReader(custstream);
				sw.WriteLine (sr.ReadToEnd ());
				sr.Close ();
			}
		}
	}
}

