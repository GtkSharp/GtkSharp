// GtkSharp.Generation.SignalHandler.cs - The SignalHandler marshaling Class.
//
// Author: Mike Kestner <mkestner@ximian.com>
//
// (c) 2002-2003 Mike Kestner
// (c) 2004 Novell, Inc.

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class SignalHandler {
		
		XmlElement sig;
		string ns;
		string retval = "";
		string s_ret = "";
		string p_ret = "";
		Parameters parms = null;

		public SignalHandler (XmlElement sig, string ns)
		{
			this.sig = sig;
			this.ns = ns;
			XmlElement params_elem = sig["parameters"] as XmlElement;
			if (params_elem != null)
				parms = new Parameters (params_elem, ns);
		}

		public bool Validate ()
		{
			XmlElement ret_elem = sig["return-type"];
			if (ret_elem == null) {
				Console.Write("Missing return-type ");
				return false;
			}
			
			retval = ret_elem.GetAttribute("type");
			if (retval == "") {
				Console.Write("Invalid return-type ");
				return false;
			}
			
			s_ret = SymbolTable.Table.GetCSType(retval);
			p_ret = SymbolTable.Table.GetMarshalReturnType(retval);
			if ((s_ret == "") || (p_ret == "")) {
				Console.Write("Funky type: " + retval);
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
				string result = SymbolTable.Table.GetName (retval);
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
			sw.WriteLine("\tusing GtkSharp;");
			sw.WriteLine();
			sw.Write("\tinternal delegate " + p_ret + " ");
			sw.WriteLine(DelegateName + "(" + ISig + ", int key);");
			sw.WriteLine();
			sw.WriteLine("\tinternal class " + Name + " : SignalCallback {");
			sw.WriteLine();
			sw.WriteLine("\t\tprivate static " + DelegateName + " _Delegate;");
			sw.WriteLine();
			sw.WriteLine("\t\tprivate IntPtr _raw;");
			sw.WriteLine("\t\tprivate uint _HandlerID;");
			sw.WriteLine();
			sw.Write("\t\tprivate static " + p_ret + " ");
			sw.WriteLine(CallbackName + "(" + ISig + ", int key)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (!_Instances.Contains(key))");
			sw.WriteLine("\t\t\t\tthrow new Exception(\"Unexpected signal key \" + key);");
			sw.WriteLine();
			sw.WriteLine("\t\t\t" + Name + " inst = (" + Name + ") _Instances[key];");
			if ((s_ret == "void") && (parms.Count == 1)) {
				sw.WriteLine("\t\t\tEventHandler h = (EventHandler) inst._handler;");
				sw.WriteLine("\t\t\th (inst._obj, new EventArgs ());");
				sw.WriteLine("\t\t}");
				sw.WriteLine();
			} else {
				sw.WriteLine("\t\t\tSignalArgs args = (SignalArgs) Activator.CreateInstance (inst._argstype);");
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
						if (wrapper != null && wrapper is ObjectGen)
							sw.WriteLine("\t\t\t\targs.Args[" + (idx-1) + "] = GLib.Object.GetObject(arg" + idx + ", false);");
						else
							sw.WriteLine("\t\t\t\targs.Args[" + (idx-1) + "] = " + table.FromNative (ctype, "arg" + idx)  + ";");
						if ((wrapper != null && (wrapper is OpaqueGen)) || table.IsManuallyWrapped (ctype)) {
							sw.WriteLine("\t\t\t\tif (args.Args[" + (idx-1) + "] == null)");
							sw.WriteLine("\t\t\t\t\targs.Args[{0}] = new {1}(arg{2});", idx-1, table.GetCSType (ctype), idx);
						}
						sw.WriteLine("\t\t\t}");
					} else {
						sw.WriteLine("\t\t\targs.Args[" + (idx-1) + "] = " + table.FromNative (ctype, "arg" + idx)  + ";");
					}
				}
				sw.WriteLine("\t\t\tobject[] argv = new object[2];");
				sw.WriteLine("\t\t\targv[0] = inst._obj;");
				sw.WriteLine("\t\t\targv[1] = args;");
				sw.WriteLine("\t\t\tinst._handler.DynamicInvoke(argv);");
				if (retval != "void") {
					sw.WriteLine ("\t\t\tif (args.RetVal == null)");
					if (s_ret == "bool")
						sw.WriteLine ("\t\t\t\treturn false;");
					else
						sw.WriteLine ("\t\t\t\tthrow new Exception(\"args.RetVal unset in callback\");");

					sw.WriteLine("\t\t\treturn (" + p_ret + ") " + table.ToNativeReturn (retval, "((" + s_ret + ")args.RetVal)") + ";");
				}
				sw.WriteLine("\t\t}");
				sw.WriteLine();
			}
			sw.WriteLine("\t\t[DllImport(\"libgobject-2.0-0.dll\")]");
			sw.Write("\t\tstatic extern uint g_signal_connect_data(");
			sw.Write("IntPtr obj, string name, " + DelegateName + " cb, int key, IntPtr p,");
			sw.WriteLine(" int flags);");
			sw.WriteLine();
			sw.Write("\t\tpublic " + Name + "(GLib.Object obj, IntPtr raw, ");
			sw.WriteLine("string name, Delegate eh, Type argstype, int connect_flags) : base(obj, eh, argstype)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (_Delegate == null) {");
			sw.WriteLine("\t\t\t\t_Delegate = new " + DelegateName + "(" + CallbackName + ");");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\t_raw = raw;");
			sw.Write("\t\t\t_HandlerID = g_signal_connect_data(raw, name, ");
			sw.WriteLine("_Delegate, _key, new IntPtr(0), connect_flags);");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			sw.WriteLine("\t\t[DllImport(\"libgobject-2.0-0.dll\")]");
			sw.WriteLine("\t\tstatic extern void g_signal_handler_disconnect (IntPtr instance, uint handler);");
			sw.WriteLine();
			sw.WriteLine("\t\t[DllImport(\"libgobject-2.0-0.dll\")]");
			sw.WriteLine("\t\tstatic extern bool g_signal_handler_is_connected (IntPtr instance, uint handler);");
			sw.WriteLine();
			sw.WriteLine("\t\tprotected override void Dispose (bool disposing)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\t_Instances.Remove(_key);");
			sw.WriteLine("\t\t\tif(_Instances.Count == 0)");
			sw.WriteLine("\t\t\t\t_Delegate = null;");
			sw.WriteLine();
			sw.WriteLine("\t\t\tif (g_signal_handler_is_connected (_raw, _HandlerID))");
			sw.WriteLine("\t\t\t\tg_signal_handler_disconnect (_raw, _HandlerID);");
			sw.WriteLine("\t\t\tbase.Dispose (disposing);");
			sw.WriteLine("\t\t}");
			sw.WriteLine("\t}");
			sw.WriteLine("}");
			sw.Close();
		}
	}
}

