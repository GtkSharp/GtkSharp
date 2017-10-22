// GtkSharp.Generation.ManagedCallString.cs - The ManagedCallString Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2003 Mike Kestner
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
	using System.Collections.Generic;
	using System.IO;

	public class ManagedCallString {
		
		IDictionary<Parameter, bool> parms = new Dictionary<Parameter, bool> ();
		IList<Parameter> dispose_params = new List<Parameter> ();
		string error_param = null;
		string user_data_param = null;
		string destroy_param = null;

		public ManagedCallString (Parameters parms)
		{
			for (int i = 0; i < parms.Count; i ++) {
				Parameter p = parms [i];
				if (p.IsLength && i > 0 && parms [i-1].IsString) 
					continue;
				else if (p.Scope == "notified") {
					user_data_param = parms[i+1].Name;
					destroy_param = parms[i+2].Name;
					i += 2;
				} else if ((p.IsCount || p.IsUserData) && parms.IsHidden (p)) {
					user_data_param = p.Name;
					continue;
				} else if (p is ErrorParameter) {
					error_param = p.Name;
					continue;
				}

				bool special = false;
				if (p.PassAs != String.Empty && (p.Name != p.FromNative (p.Name)))
					special = true;
				else if (p.Generatable is CallbackGen)
					special = true;

				this.parms.Add (p, special);

				if (p.IsOwnable) {
					dispose_params.Add (p);
				}
			}
		}

		public bool HasOutParam {
			get {
				foreach (Parameter p in parms.Keys) {
					if (p.PassAs == "out")
						return true;
				}
				return false;
			}
		}

		public bool HasDisposeParam {
			get { return dispose_params.Count > 0; }
		}

		public string Unconditional (string indent) {
			string ret = "";
			if (error_param != null)
				ret = indent + error_param + " = IntPtr.Zero;\n";

			foreach (Parameter p in dispose_params) {
				ret += indent + p.CSType + " my" + p.Name + " = null;\n";
			}
			return ret;
		}

		public string Setup (string indent)
		{
			string ret = "";

			foreach (Parameter p in parms.Keys) {
				if (parms [p] == false) {
					continue;
				}

				IGeneratable igen = p.Generatable;

				if (igen is CallbackGen) {
					if (user_data_param == null)
						ret += indent + String.Format ("{0} {1}_invoker = new {0} ({1});\n", (igen as CallbackGen).InvokerName, p.Name);
					else if (destroy_param == null)
						ret += indent + String.Format ("{0} {1}_invoker = new {0} ({1}, {2});\n", (igen as CallbackGen).InvokerName, p.Name, user_data_param);
					else
						ret += indent + String.Format ("{0} {1}_invoker = new {0} ({1}, {2}, {3});\n", (igen as CallbackGen).InvokerName, p.Name, user_data_param, destroy_param);
				} else {
					ret += indent + igen.QualifiedName + " my" + p.Name;
					if (p.PassAs == "ref")
						ret += " = " + p.FromNative (p.Name);
					ret += ";\n";
				}
			}

			foreach (Parameter p in dispose_params) {
				ret += indent + "my" + p.Name + " = " + p.FromNative (p.Name) + ";\n";
			}

			return ret;
		}

		public override string ToString ()
		{
			if (parms.Count < 1)
				return "";

			string[] result = new string [parms.Count];

			int i = 0;
			foreach (Parameter p in parms.Keys) {
				result [i] = p.PassAs == "" ? "" : p.PassAs + " ";
				if (p.Generatable is CallbackGen) {
					result [i] += p.Name + "_invoker.Handler";
				} else {
					if (parms [p] || dispose_params.Contains(p)) {
						// Parameter was declared and marshalled earlier
						result [i] +=  "my" + p.Name;
					} else {
						result [i] +=  p.FromNative (p.Name);
					}
				}
				i++;
			}

			return String.Join (", ", result);
		}

		public string Finish (string indent)
		{
			string ret = "";

			foreach (Parameter p in parms.Keys) {
				if (parms [p] == false) {
					continue;
				}

				IGeneratable igen = p.Generatable;

				if (igen is CallbackGen)
					continue;
				else if (igen is StructBase || igen is ByRefGen)
					ret += indent + String.Format ("if ({0} != IntPtr.Zero) System.Runtime.InteropServices.Marshal.StructureToPtr (my{0}, {0}, false);\n", p.Name);
				else if (igen is IManualMarshaler)
					ret += String.Format ("{0}{1} = {2};", indent, p.Name, (igen as IManualMarshaler).AllocNative ("my" + p.Name));
				else
					ret += indent + p.Name + " = " + igen.CallByName ("my" + p.Name) + ";\n";
			}

			return ret;
		}

		public string DisposeParams (string indent)
		{
			string ret = "";

			foreach (Parameter p in dispose_params) {
				string name = "my" + p.Name;
				string disp_name = "disposable_" + p.Name;

				ret += indent + "var " + disp_name + " = " + name + " as IDisposable;\n";
				ret += indent + "if (" + disp_name + " != null)\n";
				ret += indent + "\t" + disp_name + ".Dispose ();\n";
			}

			return ret;
		}
	}
}

