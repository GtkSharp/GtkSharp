// GtkSharp.Generation.GenBase.cs - The Generatable base class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2002 Mike Kestner
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


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

