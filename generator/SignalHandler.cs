// GtkSharp.Generation.SignalHandler.cs - The SignalHandler marshaling Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2002-2003 Mike Kestner
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

	public class SignalHandler {
		
		XmlElement sig;
		string ns;
		ReturnValue retval;
		Parameters parms = null;

		public SignalHandler (XmlElement sig, string ns)
		{
			this.sig = sig;
			this.ns = ns;
			retval = new ReturnValue (sig["return-type"]);
			XmlElement params_elem = sig["parameters"] as XmlElement;
			if (params_elem != null)
				parms = new Parameters (params_elem, ns);
		}

		public bool Validate ()
		{
			if (!retval.Validate ()) {
				Console.Write(" in signal handler " + Name);
				return false;
			}
			
			if (parms == null || !parms.Validate ()) {
				Console.Write("Missing parameters ");
				return false;
			}

			return true;
		}

		private string ISig {
			get {
				string result = "";
				for (int i = 0; i < parms.Count; i++) {
					if (i > 0)
						result += ", ";

					result += (parms[i].MarshalType + " arg" + i);
				}
				return result;
			}
		}
				
		private string BaseName {
			get {
				string result = SymbolTable.Table.GetName (retval.CType);
				for (int i = 0; i < parms.Count; i++) {
					if (parms[i].Generatable is ObjectGen || parms[i].Generatable is InterfaceGen) {
						result += "Object";
					} else {
						result += SymbolTable.Table.GetName(parms[i].CType);
					}
				}		 
				return result;
			}
		}

		public string CallbackName {
			get {
				return BaseName + "Callback";
			}
		}

		public string DelegateName {
			get {
				return BaseName + "Delegate";
			}
		}

		public string Name {
			get {
				return BaseName + "Signal";
			}
		}

		public void Generate (string implementor_ns, GenerationInfo gen_info)
		{
			SymbolTable table = SymbolTable.Table;

			StreamWriter sw = gen_info.OpenStream (implementor_ns + "Sharp." + Name);
			
			sw.WriteLine("namespace " + implementor_ns + "Sharp {");
			sw.WriteLine();
			sw.WriteLine("\tusing System;");
			sw.WriteLine("\tusing System.Runtime.InteropServices;");
			sw.WriteLine();
			sw.Write("\tinternal delegate " + retval.MarshalType + " ");
			sw.WriteLine(DelegateName + "(" + ISig + ", int key);");
			sw.WriteLine();
			sw.WriteLine("\tinternal class " + Name + " : GLib.SignalCallback {");
			sw.WriteLine();
			sw.WriteLine("\t\tprivate static " + DelegateName + " _Delegate;");
			sw.WriteLine();
			sw.Write("\t\tprivate static " + retval.MarshalType + " ");
			sw.WriteLine(CallbackName + "(" + ISig + ", int key)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (!_Instances.Contains(key))");
			sw.WriteLine("\t\t\t\tthrow new Exception(\"Unexpected signal key \" + key);");
			sw.WriteLine();
			sw.WriteLine("\t\t\t" + Name + " inst = (" + Name + ") _Instances[key];");
			if ((retval.CSType == "void") && (parms.Count == 1)) {
				sw.WriteLine("\t\t\tEventHandler h = (EventHandler) inst._handler;");
				sw.WriteLine("\t\t\th (inst._obj, new EventArgs ());");
				sw.WriteLine("\t\t}");
				sw.WriteLine();
			} else {
				sw.WriteLine("\t\t\tGLib.SignalArgs args = (GLib.SignalArgs) Activator.CreateInstance (inst._argstype);");
				if (parms.Count > 1) {
					sw.WriteLine("\t\t\targs.Args = new object[" + (parms.Count-1) + "];");
				}
				for (int idx=1; idx < parms.Count; idx++) {
					string ctype = parms[idx].CType;
					ClassBase wrapper = table.GetClassGen (ctype);
					if ((wrapper != null && !(wrapper is StructBase)) || table.IsManuallyWrapped (ctype)) {
						sw.WriteLine("\t\t\tif (arg{0} == IntPtr.Zero)", idx);
						sw.WriteLine("\t\t\t\targs.Args[{0}] = null;", idx - 1);
						sw.WriteLine("\t\t\telse {");
						if ((wrapper != null) && wrapper is ObjectGen)
							sw.WriteLine("\t\t\t\targs.Args[" + (idx-1) + "] = GLib.Object.GetObject(arg" + idx + ");");
						else
							sw.WriteLine("\t\t\t\targs.Args[" + (idx-1) + "] = " + table.FromNative (ctype, "arg" + idx)  + ";");
						sw.WriteLine("\t\t\t}");
					} else {
						sw.WriteLine("\t\t\targs.Args[" + (idx-1) + "] = " + table.FromNative (ctype, "arg" + idx)  + ";");
					}
				}
				sw.WriteLine("\t\t\tobject[] argv = new object[2];");
				sw.WriteLine("\t\t\targv[0] = inst._obj;");
				sw.WriteLine("\t\t\targv[1] = args;");
				sw.WriteLine("\t\t\tinst._handler.DynamicInvoke(argv);");
				if (retval.CSType != "void") {
					sw.WriteLine ("\t\t\tif (args.RetVal == null)");
					if (retval.CSType == "bool")
						sw.WriteLine ("\t\t\t\treturn false;");
					else
						sw.WriteLine ("\t\t\t\tthrow new Exception(\"args.RetVal unset in callback\");");

					sw.WriteLine("\t\t\treturn (" + retval.MarshalType + ") " + table.ToNativeReturn (retval.CType, "((" + retval.CSType + ")args.RetVal)") + ";");
				}
				sw.WriteLine("\t\t}");
				sw.WriteLine();
			}
			sw.Write("\t\tpublic " + Name + "(GLib.Object obj, ");
			sw.WriteLine("string name, Delegate eh, Type argstype, int connect_flags) : base(obj, eh, argstype)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (_Delegate == null) {");
			sw.WriteLine("\t\t\t\t_Delegate = new " + DelegateName + "(" + CallbackName + ");");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\tConnect (name, _Delegate, connect_flags);");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			sw.WriteLine("\t\tprotected override void Dispose (bool disposing)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\t_Instances.Remove(_key);");
			sw.WriteLine("\t\t\tif(_Instances.Count == 0)");
			sw.WriteLine("\t\t\t\t_Delegate = null;");
			sw.WriteLine();
			sw.WriteLine("\t\t\tDisconnect ();");
			sw.WriteLine("\t\t\tbase.Dispose (disposing);");
			sw.WriteLine("\t\t}");
			sw.WriteLine("\t}");
			sw.WriteLine("}");
			sw.Close();
		}
	}
}

