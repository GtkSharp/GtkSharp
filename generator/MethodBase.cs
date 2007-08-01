// GtkSharp.Generation.MethodBase.cs - function element base class.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004-2005 Novell, Inc.
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
	using System.Xml;

	public abstract class MethodBase  {

		XmlElement elem;
		protected ClassBase container_type;
		Parameters parms;
		bool is_static = false;
		string mods = String.Empty;

		protected MethodBase (XmlElement elem, ClassBase container_type) 
		{
			this.elem = elem;
			this.container_type = container_type;
			parms = new Parameters (elem ["parameters"]);
			IsStatic = elem.GetAttribute ("shared") == "true";
			if (elem.HasAttribute ("new_flag"))
				mods = "new ";
		}

		MethodBody body;
		public MethodBody Body {
			get {
				if (body == null)
					body = new MethodBody (parms);
				return body;
			}
		}

		public string CName {
			get {
				return elem.GetAttribute ("cname");
			}
		}

		public bool IsStatic {
			get {
				return is_static;
			}
			set {
				is_static = value;
				parms.Static = value;
			}
		}

		public string LibraryName {
			get {
				if (elem.HasAttribute ("library"))
					return elem.GetAttribute ("library");
				return container_type.LibraryName;
			}
		}

		public string Modifiers {
			get {
				return mods;
			}
			set {
				mods = value;
			}
		}

		public Parameters Parameters {
			get {
				return parms;
			}
		}

		protected string Safety {
			get {
				return Body.ThrowsException && !(container_type is InterfaceGen) ? "unsafe " : "";
			}
		}

		Signature sig;
		public Signature Signature {
			get {
				if (sig == null)
					sig = new Signature (parms);
				return sig;
			}
		}

		public virtual bool Validate ()
		{
			if (!parms.Validate ()) {
				Console.Write("in " + CName + " ");
				Statistics.ThrottledCount++;
				return false;
			}

			return true;
		}
	}
}

