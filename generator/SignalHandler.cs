// GtkSharp.Generation.SignalHandler.cs - The SignalHandler marshaling Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002-2003 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class SignalHandler {
		
		public static string GetName(XmlElement sig, string ns, bool generate)
		{
			XmlElement ret_elem = sig["return-type"];
			if (ret_elem == null) {
				Console.Write("Missing return-type ");
				return "";
			}
			
			string retval = ret_elem.GetAttribute("type");
			if (retval == "") {
				Console.Write("Invalid return-type ");
				return "";
			}
			
			SymbolTable table = SymbolTable.Table;

			string s_ret = table.GetCSType(retval);
			string p_ret = table.GetMarshalReturnType(retval);
			if ((s_ret == "") || (p_ret == "")) {
				Console.Write("Funky type: " + retval);
				return "";
			}
			
			string key = retval;
			string pinv = "";
			string name = table.GetName(retval);
			int pcnt = 0;
			
			ArrayList parms = new ArrayList();
			
			XmlElement params_elem = sig["parameters"];
			if (params_elem == null) {
				Console.Write("Missing parameters ");
				return "";
			}
				
			foreach (XmlNode parm in params_elem.ChildNodes) {
				if (!(parm is XmlElement) || parm.Name != "parameter") continue;

				XmlElement elem = (XmlElement) parm;
				string type = elem.GetAttribute("type");
				string ptype = table.GetMarshalType(type);
				if (ptype == "") {
					Console.Write("Funky type: " + type);
					return "";
				}
			
				if (pcnt > 0) {
					pinv += ", ";
				}

				pinv += (ptype + " arg" + pcnt);
				parms.Add(type);
				if (table.IsObject(type) || table.IsInterface(type)) {
					name += "Object";
					key += "Object";
				} else {
					name += table.GetName(type);
					key += type;
				}
				pcnt++;
			}		 

			String sname = name + "Signal";
			String dname = name + "Delegate";
			String cbname = name + "Callback";

			if (!generate)
				return ns + "." + sname;

			char sep = Path.DirectorySeparatorChar;
			String dir = ".." + sep + ns.ToLower() + sep + "generated";

			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}

			String filename = dir + sep + ns + "Sharp." + sname + ".cs";

			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			sw.WriteLine("namespace " + ns + "Sharp {");
			sw.WriteLine();
			sw.WriteLine("\tusing System;");
			sw.WriteLine("\tusing System.Runtime.InteropServices;");
			sw.WriteLine("\tusing GtkSharp;");
			sw.WriteLine();
			sw.Write("\tinternal delegate " + p_ret + " ");
			sw.WriteLine(dname + "(" + pinv + ", int key);");
			sw.WriteLine();
			sw.WriteLine("\tinternal class " + sname + " : SignalCallback {");
			sw.WriteLine();
			sw.WriteLine("\t\tprivate static " + dname + " _Delegate;");
			sw.WriteLine();
			sw.WriteLine("\t\tprivate IntPtr _raw;");
			sw.WriteLine("\t\tprivate uint _HandlerID;");
			sw.WriteLine();
			sw.Write("\t\tprivate static " + p_ret + " ");
			sw.WriteLine(cbname + "(" + pinv + ", int key)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (!_Instances.Contains(key))");
			sw.WriteLine("\t\t\t\tthrow new Exception(\"Unexpected signal key \" + key);");
			sw.WriteLine();
			sw.WriteLine("\t\t\t" + sname + " inst = (" + sname + ") _Instances[key];");
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
					// sw.WriteLine("\t\t\tConsole.WriteLine (\"" + sname + " arg{0}: \" + arg{0});", idx);
					string ctype = (string) parms[idx];
					ClassBase wrapper = table.GetClassGen (ctype);
					if ((wrapper != null && !(wrapper is StructBase)) || table.IsManuallyWrapped (ctype)) {
						sw.WriteLine("\t\t\tif (arg{0} == IntPtr.Zero)", idx);
						sw.WriteLine("\t\t\t\targs.Args[{0}] = null;", idx - 1);
						sw.WriteLine("\t\t\telse {");
						if (wrapper != null && wrapper is ObjectGen)
							sw.WriteLine("\t\t\t\targs.Args[" + (idx-1) + "] = GLib.Object.GetObject(arg" + idx + ", true);");
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
			sw.Write("IntPtr obj, String name, " + dname + " cb, int key, IntPtr p,");
			sw.WriteLine(" int flags);");
			sw.WriteLine();
			sw.Write("\t\tpublic " + sname + "(GLib.Object obj, IntPtr raw, ");
			sw.WriteLine("String name, Delegate eh, Type argstype) : base(obj, eh, argstype)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (_Delegate == null) {");
			sw.WriteLine("\t\t\t\t_Delegate = new " + dname + "(" + cbname + ");");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t\t_raw = raw;");
			sw.Write("\t\t\t_HandlerID = g_signal_connect_data(raw, name, ");
			sw.WriteLine("_Delegate, _key, new IntPtr(0), 0);");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			sw.WriteLine("\t\t[DllImport(\"libgobject-2.0-0.dll\")]");
			sw.WriteLine("\t\tstatic extern void g_signal_handler_disconnect (IntPtr instance, uint handler);");
			sw.WriteLine();
			sw.WriteLine("\t\tprotected override void Dispose (bool disposing)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\t_Instances.Remove(_key);");
			sw.WriteLine("\t\t\tif(_Instances.Count == 0)");
			sw.WriteLine("\t\t\t\t_Delegate = null;");
			sw.WriteLine();
			sw.WriteLine("\t\t\tg_signal_handler_disconnect (_raw, _HandlerID);");
			sw.WriteLine("\t\t\tbase.Dispose (disposing);");
			sw.WriteLine("\t\t}");
			sw.WriteLine("\t}");
			sw.WriteLine("}");
			sw.Close();

			return ns + "Sharp." + sname;
		}
	}
}

