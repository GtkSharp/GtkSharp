// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
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
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class Parameter {

		private XmlElement elem;

		public Parameter (XmlElement e)
		{
			elem = e;
		}

		public string CType {
			get {
				string type = elem.GetAttribute("type");
				if (type == "void*")
					type = "gpointer";
				return type;
			}
		}

		public string CSType {
			get {
				string cstype = SymbolTable.Table.GetCSType( elem.GetAttribute("type"));
				if (cstype == "void")
					cstype = "System.IntPtr";
				if (IsArray) {
					if (IsParams)
						cstype = "params " + cstype;
					cstype += "[]";
					cstype = cstype.Replace ("ref ", "");
				}
				return cstype;
			}
		}

		public IGeneratable Generatable {
			get {
				return SymbolTable.Table[CType];
			}
		}

		public bool IsArray {
			get {
				return elem.HasAttribute("array");
			}
		}

		public bool IsEllipsis {
			get {
				return elem.HasAttribute("ellipsis");
			}
		}

		public bool IsCount {
			get {
				
				if (Name.StartsWith("n_"))
					switch (CSType) {
					case "int":
					case "uint":
					case "long":
					case "ulong":
					case "short":
					case "ushort": 
						return true;
					default:
						return false;
					}
				else
					return false;
			}
		}

		public bool IsLength {
			get {
				
				if (Name.EndsWith("len") || Name.EndsWith("length"))
					switch (CSType) {
					case "int":
					case "uint":
					case "long":
					case "ulong":
					case "short":
					case "ushort": 
						return true;
					default:
						return false;
					}
				else
					return false;
			}
		}

		public bool IsParams {
			get {
				return elem.HasAttribute("params");
			}
		}

		public bool IsString {
			get {
				return (CSType == "string");
			}
		}

		public bool IsUserData {
			get {
				return CSType == "IntPtr" && (Name.EndsWith ("data") || Name.EndsWith ("data_or_owner"));
			}
		}

		public string MarshalType {
			get {
				string type = SymbolTable.Table.GetMarshalType( elem.GetAttribute("type"));
				if (type == "void")
					type = "System.IntPtr";
				if (IsArray) {
					type += "[]";
					type = type.Replace ("ref ", "");
				}
				return type;
			}
		}

		public string Name {
			get {
				return SymbolTable.Table.MangleName (elem.GetAttribute("name"));
			}
		}

		public bool NullOk {
			get {
				return elem.HasAttribute ("null_ok");
			}
		}

		public string PropertyName {
			get {
				return elem.GetAttribute("property_name");
			}
		}

		public string PassAs {
			get {
				if (elem.HasAttribute ("pass_as"))
					return elem.GetAttribute ("pass_as");

				if (IsArray || CSType.EndsWith ("IntPtr"))
					return "";

				if (CType.EndsWith ("*") && (Generatable is SimpleGen || Generatable is EnumGen))
					return "out";

				return "";
			}
		}

		public string CallByName (string call_parm_name)
		{
			string call_parm;
			if (Generatable is CallbackGen)
				call_parm = SymbolTable.Table.CallByName (CType, call_parm_name + "_wrapper");
			else
				call_parm = SymbolTable.Table.CallByName(CType, call_parm_name);
			
			if (NullOk && !CSType.EndsWith ("IntPtr") && !(Generatable is StructBase))
				call_parm = String.Format ("({0} != null) ? {1} : {2}", call_parm_name, call_parm, Generatable is CallbackGen ? "null" : "IntPtr.Zero");

			if (IsArray)
				call_parm = call_parm.Replace ("ref ", "");

			return call_parm;

		}

		public string StudlyName {
			get {
				string name = elem.GetAttribute("name");
				string[] segs = name.Split('_');
				string studly = "";
				foreach (string s in segs) {
					if (s.Trim () == "")
						continue;
					studly += (s.Substring(0,1).ToUpper() + s.Substring(1));
				}
				return studly;
				
			}
		}
	}

	public class Parameters : IEnumerable {
		
		ArrayList param_list = new ArrayList ();

		public Parameters (XmlElement elem) {
			
			if (elem == null)
				return;

			foreach (XmlNode node in elem.ChildNodes) {
				XmlElement parm = node as XmlElement;
				if (parm != null && parm.Name == "parameter")
					param_list.Add (new Parameter (parm));
			}
		}

		public int Count {
			get {
				return param_list.Count;
			}
		}

		public Parameter this [int idx] {
			get {
				return param_list [idx] as Parameter;
			}
		}

		bool hide_data;
		public bool HideData {
			get { return hide_data; }
			set { hide_data = value; }
		}

		bool is_static;
		public bool Static {
			get { return is_static; }
			set { is_static = value; }
		}

		bool cleared = false;
		void Clear ()
		{
			cleared = true;
			param_list.Clear ();
		}

		public IEnumerator GetEnumerator ()
		{
			return param_list.GetEnumerator ();
		}

		public bool Validate ()
		{
			if (cleared)
				return false;

			foreach (Parameter p in param_list) {
				
				if (p.IsEllipsis) {
					Console.Write("Ellipsis parameter ");
					Clear ();
					
					return false;
				}

				if ((p.CSType == "") || (p.Name == "") || 
				    (p.MarshalType == "") || (SymbolTable.Table.CallByName(p.CType, p.Name) == "")) {
					Console.Write("Name: " + p.Name + " Type: " + p.CType + " ");
					Clear ();
					return false;
				}
			}
			
			return true;
		}

		public bool IsAccessor {
			get {
				return Count == 1 && this [0].PassAs == "out";
			}
		}

		public string AccessorReturnType {
			get {
				if (Count > 0)
					return this [0].CSType;
				else
					return null;
			}
		}

		public string AccessorName {
			get {
				if (Count > 0)
					return this [0].Name;
				else
					return null;
			}
		}
	}
}

