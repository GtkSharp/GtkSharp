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
	using System.Xml;

	public class Parameter {

		private XmlElement elem;

		public Parameter (XmlElement e)
		{
			elem = e;
		}

		string call_name;
		public string CallName {
			get {
				if (call_name == null)
					return Name;
				else
					return call_name;
			}
			set {
				call_name = value;
			}
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
				return elem.GetAttributeAsBoolean ("array") || elem.GetAttributeAsBoolean ("null_term_array");
			}
		}

		public bool IsEllipsis {
			get {
				return elem.GetAttributeAsBoolean ("ellipsis");
			}
		}

		internal bool IsOptional {
			get {
				return elem.GetAttributeAsBoolean ("allow-none");
			}
		}

		bool is_count;
		bool is_count_set;
		public bool IsCount {
			get {
				if (is_count_set)
					return is_count;
				
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
			set {
				is_count_set = true;
				is_count = value;
			}
		}

		public bool IsDestroyNotify {
			get {
				return CType == "GDestroyNotify";
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

		public virtual string MarshalType {
			get {
				string type = SymbolTable.Table.GetMarshalType( elem.GetAttribute("type"));
				if (type == "void" || Generatable is IManualMarshaler)
					type = "IntPtr";
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

		public bool IsOwnable {
			get {
				return this.Generatable is OwnableGen;
			}
		}

		public bool Owned {
			get {
				return elem.GetAttribute ("owned") == "true";
			}
		}

		public virtual string NativeSignature {
			get {
				string sig = MarshalType + " " + Name;

				// A caller-allocated out parameter is a plain pointer to memory
				// this side owns, so the import takes it by value; only the
				// managed-facing signature keeps the "out".
				if (PassAs != String.Empty && !IsCallerAllocatedOut)
					sig = PassAs + " " + sig;

				return sig;
			}
		}

		public string PropertyName {
			get {
				return elem.GetAttribute("property_name");
			}
		}

		string pass_as;

		public string PassAs {
			get {
				if (pass_as != null)
					return pass_as;

				if (elem.HasAttribute ("pass_as"))
					return elem.GetAttribute ("pass_as");

				if (IsArray || CSType.EndsWith ("IntPtr"))
					return "";

				if (CType.EndsWith ("*") && (Generatable is SimpleGen || Generatable is EnumGen))
					return "out";

				return "";
			}
			set {
				pass_as = value;
			}
		}

		string scope;
		public string Scope {
			get {
				if (scope == null)
					scope = elem.GetAttribute ("scope");
				return scope;
			}
			set {
				scope = value;
			}
		}

		int closure = -1;
		public int Closure {
			get {
				if(closure == -1 && elem.HasAttribute ("closure"))
					closure = int.Parse(elem.GetAttribute ("closure"));
				return closure;
			}
			set {
				closure = value;
			}
		}

		int destroynotify = -1;
		public int DestroyNotify {
			get {
				if (destroynotify == -1 && elem.HasAttribute ("destroy"))
					destroynotify = int.Parse (elem.GetAttribute ("destroy"));
				return destroynotify;
			}
			set {
				destroynotify = value;
			}
		}

		public bool IsHidden {
			get {
				return elem.GetAttributeAsBoolean ("hidden");
			}
		}

		/// <summary>
		/// An out parameter whose storage the *caller* provides, wrapped by a
		/// reference type.
		/// </summary>
		/// <remarks>
		/// C writes the result into memory the caller owns -- graphene_rect_union's
		/// `res`, gsk_render_node_get_bounds' `bounds`. Passing "out IntPtr" gives
		/// the callee an 8-byte slot to write a 16-byte struct into, which is
		/// stack corruption, and the pointer read back afterwards is garbage.
		///
		/// Value types are unaffected, because a managed struct already provides
		/// real storage: Gdk.Rectangle out parameters work, and GdkRectangle is a
		/// struct only because GdkSharp-symbols.xml overrides it. That is why this
		/// is keyed off the api.xml attribute rather than off the type.
		/// </remarks>
		public bool IsCallerAllocatedOut {
			get {
				// StructBase covers <boxed>; OpaqueGen covers <boxed opaque="true">,
				// which is what most of these are. Both emit abi_info, which is
				// where the size comes from. Primitive out parameters -- ulong,
				// IntPtr -- are excluded by the IntPtr marshal-type test, and are
				// correct as ordinary out parameters.
				return PassAs == "out"
					&& elem.GetAttributeAsBoolean ("caller_allocates")
					&& (Generatable is StructBase || Generatable is OpaqueGen)
					&& MarshalType == "IntPtr";
			}
		}

		public virtual string[] Prepare {
			get {
				IGeneratable gen = Generatable;

				if (IsCallerAllocatedOut) {
					// abi_info.Size is the type's own computed C size, which is
					// exactly what the callee is going to write. The buffer is
					// g_malloc'd and released by the type's own free function,
					// which pairs correctly because GLib's slice allocator has
					// been an alias for g_malloc since 2.76.
					return new string [] {
						"IntPtr native_" + CallName + " = GLib.Marshaller.Malloc ((ulong) " +
							CSType + ".abi_info.Size);"
					};
				}

				if (gen is IManualMarshaler) {
					string result = "IntPtr native_" + CallName;
					if (PassAs != "out")
						result += " = " + (gen as IManualMarshaler).AllocNative (CallName);
					return new string [] { result + ";" };
				} else if (PassAs == "out" && CSType != MarshalType) {
					return new string [] { gen.MarshalType + " native_" + CallName + ";" };
				} else if (PassAs == "ref" && CSType != MarshalType) {
					return new string [] { gen.MarshalType + " native_" + CallName + " = (" + gen.MarshalType + ") " + CallName + ";" };
				} else if (gen is OpaqueGen && Owned) {
					return new string [] { CallName + ".Owned = false;" };
				}

				return new string [0];
			}
		}

		public virtual string CallString {
			get {
				string call_parm;

				IGeneratable gen = Generatable;
				if (gen is CallbackGen)
					return SymbolTable.Table.CallByName (CType, CallName + "_wrapper");
				else if (IsCallerAllocatedOut)
					// By value: the pointer *is* the storage, so it is not an out.
					return "native_" + CallName;
				else if (PassAs != String.Empty) {
					call_parm = PassAs + " ";
					if (CSType != MarshalType)
						call_parm += "native_";
					call_parm += CallName;
				} else if (gen is IManualMarshaler)
					call_parm = "native_" + CallName;
				else if (gen is ObjectBase)
					call_parm = (gen as ObjectBase).CallByName (CallName, Owned);
				else
					call_parm = gen.CallByName (CallName);
			
				return call_parm;
			}
		}

		public virtual string[] Finish {
			get {
				IGeneratable gen = Generatable;

				if (IsCallerAllocatedOut) {
					// Owned: the buffer was allocated here, so the wrapper frees
					// it through the type's own free function.
					string wrap = gen is IOwnable
						? (gen as IOwnable).FromNative ("native_" + CallName, true)
						: gen.FromNative ("native_" + CallName);
					return new string [] { CallName + " = " + wrap + ";" };
				}

				if (gen is IManualMarshaler) {
					string[] result = new string [PassAs == "ref" ? 2 : 1];
					int i = 0;
					if (PassAs != String.Empty)
						result [i++] = CallName + " = " + Generatable.FromNative ("native_" + CallName) + ";";
					if (PassAs != "out")
						result [i] = (gen as IManualMarshaler).ReleaseNative ("native_" + CallName) + ";";
					return result;
				} else if (PassAs != String.Empty && MarshalType != CSType)
					if (gen is IOwnable)
						return new string [] { CallName + " = " + (gen as IOwnable).FromNative ("native_" + CallName, Owned) + ";" };
					else
						return new string [] { CallName + " = " + gen.FromNative ("native_" + CallName) + ";" };
				return new string [0];
			}
		}

		public string FromNative (string var)
		{
			if (Generatable == null)
				return String.Empty;
			else if (Generatable is IOwnable)
				return ((IOwnable)Generatable).FromNative (var, Owned);
			else
				return Generatable.FromNative (var);
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

	public class ErrorParameter : Parameter {

		public ErrorParameter (XmlElement elem) : base (elem)
		{
			PassAs = "out";
		}

		public override string CallString {
			get {
				return "out error";
			}
		}
	}

	public class StructParameter : Parameter {

		public StructParameter (XmlElement elem) : base (elem) {}

		public override string MarshalType {
			get {
				return "IntPtr";
			}
		}

		public override string[] Prepare {
			get {
				if (PassAs == "out")
					return new string [] { "IntPtr native_" + CallName + " = Marshal.AllocHGlobal (" + GenerationInfo.GetSizeOfExpression(Generatable.QualifiedName) + ");"};
				else
					return new string [] { "IntPtr native_" + CallName + " = " + (Generatable as IManualMarshaler).AllocNative (CallName) + ";"};
			}
		}

		public override string CallString {
			get {
				return "native_" + CallName;
			}
		}

		public override string[] Finish {
			get {
				string[] result = new string [PassAs == string.Empty ? 1 : 2];
				int i = 0;
				if (PassAs != string.Empty) {
					result [i++] = CallName + " = " + FromNative ("native_" + CallName) + ";";
				}
				result [i++] = (Generatable as IManualMarshaler).ReleaseNative ("native_" + CallName) + ";";
				return result;
			}
		}

		public override string NativeSignature {
			get {
				return "IntPtr " + CallName;
			}
		}
	}
}
