// GtkSharp.Generation.SignalHandler.cs - The SignalHandler marshaling Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2002 Mike Kestner

namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class SignalHandler {
		
		private static Hashtable handlers = new Hashtable ();
		
		public static String GetName(XmlElement sig, SymbolTable table)
		{
			XmlElement ret_elem = sig["return-type"];
			if (ret_elem == null) {
				Console.Write("Missing return-type ");
				return "";
			}
			
			String retval = ret_elem.GetAttribute("type");
			if (retval == "") {
				Console.Write("Invalid return-type ");
				return "";
			}
			
			String s_ret = table.GetCSType(retval);
			String p_ret = table.GetMarshalType(retval);
			if ((s_ret == "") || (p_ret == "")) {
				Console.Write("Funky type: " + retval);
				return "";
			}
			
			String key = retval;
			String pinv = "";
			String name = table.GetName(retval);
			int pcnt = 0;
			
			ArrayList parms = new ArrayList();
			
			XmlElement params_elem = sig["parameters"];
			if (params_elem == null) {
				Console.Write("Missing parameters ");
				return "";
			}
				
			foreach (XmlNode parm in params_elem.ChildNodes) {
				if (parm.Name != "parameter") continue;

				XmlElement elem = (XmlElement) parm;
				String type = elem.GetAttribute("type");
				String ptype = table.GetMarshalType(type);
				if (ptype == "") {
					Console.Write("Funky type: " + type);
					return "";
				}
			
				if (pcnt > 0) {
					pinv += ", ";
				}
				pinv += (ptype + " arg" + pcnt);
				parms.Add(type);
				if (table.IsObject(type)) {
					name += "Object";
					key += "Object";
				} else {
					name += table.GetName(type);
					key += type;
				}
				pcnt++;
			}		 

			if (handlers.ContainsKey(name)) {
				return (String) handlers[name];
			}
			
			String dir;
						
			if (key.IndexOf("Gtk") >= 0) {
				dir = "..\\gtk\\generated";
			} else if (key.IndexOf("Gdk") >= 0) {
				dir = "..\\gdk\\generated";
			} else if (key.IndexOf("Atk") >= 0) {
				dir = "..\\atk\\generated";
			} else if (key.IndexOf("Pango") >= 0) {
				dir = "..\\pango\\generated";
			} else {
				dir = "..\\glib\\generated";
			}

			String sname = name + "Signal";
			String dname = name + "Delegate";
			String cbname = name + "Callback";

			handlers[name] = sname;

			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}

			String filename = dir + "\\" + sname + ".cs";

			FileStream stream = new FileStream (filename, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter (stream);
			
			sw.WriteLine ("// Generated File.  Do not modify.");
			sw.WriteLine ("// <c> 2001-2002 Mike Kestner");
			sw.WriteLine ();
			sw.WriteLine("namespace GtkSharp {");
			sw.WriteLine();
			sw.WriteLine("\tusing System;");
			sw.WriteLine("\tusing System.Runtime.InteropServices;");
			sw.WriteLine();
			sw.Write("\tpublic delegate " + p_ret + " ");
			sw.WriteLine(dname + "(" + pinv + ", int key);");
			sw.WriteLine();
			sw.WriteLine("\tpublic class " + sname + " : SignalCallback {");
			sw.WriteLine();
			sw.WriteLine("\t\tprivate static " + dname + " _Delegate;");
			sw.WriteLine();
			sw.Write("\t\tprivate static " + s_ret + " ");
			sw.WriteLine(cbname + "(" + pinv + ", int key)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (!_Instances.Contains(key))");
			sw.WriteLine("\t\t\t\tthrow new Exception(\"Unexpected signal key\");");
			sw.WriteLine();
			sw.WriteLine("\t\t\t" + sname + " inst = (" + sname + ") _Instances[key];");
			sw.WriteLine("\t\t\tSignalArgs args = new SignalArgs();");
			for (int idx=1; idx < parms.Count; idx++) {
				if (table.IsObject((String)parms[idx])) {
					sw.Write("\t\t\targs.Args[" + idx + "] ");
					sw.WriteLine("= GLib.Object.GetObject(arg" + idx + ");");
				} else {
					sw.WriteLine("\t\t\targs.Args[" + idx + "] = arg" + idx + ";");
				}
			}
			sw.WriteLine("\t\t\tinst._handler (inst._obj, args);");
			if (retval != "void") {
				sw.WriteLine("\t\t\treturn (" + s_ret + ") args.RetVal;");
			}
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			sw.Write("\t\t[DllImport(\"gobject-1.3.dll\", ");
			sw.WriteLine("CallingConvention=CallingConvention.Cdecl)]");
			sw.Write("\t\tstatic extern void g_signal_connect_data(");
			sw.Write("IntPtr obj, String name, " + dname + " cb, int key, IntPtr p,");
			sw.WriteLine(" int flags);");
			sw.WriteLine();
			sw.Write("\t\tpublic " + sname + "(GLib.Object obj, IntPtr raw, ");
			sw.WriteLine("String name, EventHandler eh) : base(obj, eh)");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\tif (_Delegate == null) {");
			sw.WriteLine("\t\t\t\t_Delegate = new " + dname + "(" + cbname + ");");
			sw.WriteLine("\t\t\t}");
			sw.Write("\t\t\tg_signal_connect_data(raw, name, ");
			sw.WriteLine("_Delegate, _key, new IntPtr(0), 0);");
			sw.WriteLine("\t\t}");
			sw.WriteLine();
			sw.WriteLine("\t\t~" + sname + "()");
			sw.WriteLine("\t\t{");
			sw.WriteLine("\t\t\t_Instances.Remove(_key);");
			sw.WriteLine("\t\t\tif(_Instances.Count == 0) {");
			sw.WriteLine("\t\t\t\t_Delegate = null;");
			sw.WriteLine("\t\t\t}");
			sw.WriteLine("\t\t}");
			sw.WriteLine("\t}");
			sw.WriteLine("}");
			sw.Close();

			return sname;
		}
	}
}

