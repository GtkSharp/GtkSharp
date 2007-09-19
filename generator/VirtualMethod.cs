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

		public VirtualMethod (XmlElement elem, ClassBase container_type) 
		{
			this.elem = elem;
			retval = new ReturnValue (elem ["return-type"]);
			parms = new Parameters (elem["parameters"]);
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

		bool IsGetter {
			get {
				return (Name.StartsWith ("Get") || Name.StartsWith ("Has")) && ((!retval.IsVoid && parms.Count == 1) || (retval.IsVoid && parms.Count == 2 && parms [1].PassAs == "out"));
			}
		}
	
		bool IsSetter {
			get {
				if (!Name.StartsWith ("Set") || !retval.IsVoid)
					return false;

				if (parms.Count == 2 || (parms.Count == 4 && parms [1].Scope == "notified"))
					return true;
				else
					return false;
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

		public void GenerateCallback (StreamWriter sw)
		{
			if (!Validate ())
				return;

			ManagedCallString call = new ManagedCallString (parms);
			string type = parms [0].CSType;
			string name = parms [0].Name;
			string call_string = "__obj." + Name + " (" + call + ")";
			if (IsGetter)
				call_string = "__obj." + (Name.StartsWith ("Get") ? Name.Substring (3) : Name);
			else if (IsSetter)
				call_string = "__obj." + Name.Substring (3) + " = " + call;

			sw.WriteLine ("\t\t[GLib.CDeclCallback]");
			sw.WriteLine ("\t\tdelegate " + MarshalReturnType + " " + Name + "Delegate (" + parms.ImportSignature + ");");
			sw.WriteLine ();
			sw.WriteLine ("\t\tstatic " + MarshalReturnType + " " + Name + "Callback (" + parms.ImportSignature + ")");
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\t" + type + " __obj = GLib.Object.GetObject (" + name + ", false) as " + type + ";");
			sw.Write (call.Setup ("\t\t\t"));
			if (retval.IsVoid) { 
				if (IsGetter) {
					Parameter p = parms [1];
					string out_name = p.Name;
					if (p.MarshalType != p.CSType)
						out_name = "my" + out_name;
					sw.WriteLine ("\t\t\t" + out_name + " = " + call_string + ";");
				} else
					sw.WriteLine ("\t\t\t" + call_string + ";");
			} else
				sw.WriteLine ("\t\t\t" + retval.ToNativeType + " result = " + retval.ToNative (call_string) + ";");
			sw.Write (call.Finish ("\t\t\t"));
			if (!retval.IsVoid)
				sw.WriteLine ("\t\t\treturn result;");
			sw.WriteLine ("\t\t}");
		}

		enum ValidState {
			Unvalidated,
			Invalid,
			Valid
		}

		ValidState vstate = ValidState.Unvalidated;

		public bool IsValid {
			get { 
				if (vstate == ValidState.Unvalidated)
					return Validate ();
				else
					return vstate == ValidState.Valid; 
			}
		}

		bool Validate ()
		{
			if (!parms.Validate () || !retval.Validate ()) {
				Console.Write ("in virtual method " + Name + " ");
				vstate = ValidState.Invalid;
				return false;
			}

			vstate = ValidState.Valid;
			return true;
		}
	}
}

