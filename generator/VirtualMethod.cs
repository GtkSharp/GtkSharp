// GtkSharp.Generation.VirtualMethod.cs - The VirtualMethod Generatable.
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2003-2004 Novell, Inc.
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
	using System.Collections;
	using System.IO;
	using System.Xml;

	// FIXME: handle static VMs
	public class VirtualMethod  {
		
		XmlElement elem;
		ReturnValue retval;
		Parameters parms;
		ImportSignature isig;

		public VirtualMethod (XmlElement elem, ClassBase container_type) 
		{
			this.elem = elem;
			retval = new ReturnValue (elem ["return-type"]);
			parms = new Parameters (elem["parameters"]);
			isig = new ImportSignature (parms, container_type.NS);
		}

		public string CName {
			get {
				return elem.GetAttribute("cname");
			}
		}

		public string Declaration {
			get {
				VMSignature vmsig = new VMSignature (parms);
				return retval.CSType + " " + Name + " (" + vmsig + ");";
			}
		}

		public string MarshalReturnType {
			get {
				return SymbolTable.Table.GetToNativeReturnType (elem["return-type"].GetAttribute("type"));
			}
		}

		public string Name {
			get {
				return elem.GetAttribute("name");
			}
		}

		public string NativeDelegate {
			get {
				return "delegate " + MarshalReturnType + " " + Name + "Delegate (" + isig + ");";
			}
		}

		public void GenerateCallback (StreamWriter sw)
		{
			ManagedCallString call = new ManagedCallString (parms);
			string type = parms [0].CSType;
			string name = parms [0].Name;
			string call_string = "__obj." + Name + " (" + call + ")";
			sw.WriteLine ("\t\tstatic " + MarshalReturnType + " " + Name + "Callback (" + isig + ")");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\t" + type + " __obj = GLib.Object.GetObject (" + name + ", false) as " + type + ";");
			sw.Write (call.Setup ("\t\t\t"));
			if (retval.CSType == "void")
				sw.WriteLine ("\t\t\t" + call_string + ";");
			else {
				sw.WriteLine ("\t\t\treturn " + SymbolTable.Table.ToNativeReturn (retval.CType, call_string) + ";");
			}
			sw.Write (call.Finish ("\t\t\t"));
			sw.WriteLine ("\t\t}");
		}

		public bool Validate ()
		{
			if (!parms.Validate () || !retval.Validate ()) {
				Console.Write ("in virtual method " + Name + " ");
				return false;
			}

			for (int i = 0; i < parms.Count; i++) {
				Parameter p = parms [i];
				if (p.Generatable is CallbackGen) {
					Console.Write("Callback: " + p.CSType + " in virtual method " + Name + " ");
					Statistics.ThrottledCount++;
					return false;
				}
			}

			return true;
		}
	}
}

