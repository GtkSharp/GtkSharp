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
		private string args_type;
		
		public static String GetName(XmlElement sig)
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
			
			string s_ret = SymbolTable.GetCSType(retval);
			string p_ret = SymbolTable.GetMarshalType(retval);
			if ((s_ret == "") || (p_ret == "")) {
				Console.Write("Funky type: " + retval);
				return "";
			}
			
			string key = retval;
			string pinv = "";
			string name = SymbolTable.GetName(retval);
			string argfields = "";
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
				string ptype = SymbolTable.GetMarshalType(type);
				if (ptype == "") {
					Console.Write("Funky type: " + type);
					return "";
				}
			
				if (pcnt > 0) {
					pinv += ", ";
				}

				if (SymbolTable.IsStruct(type))
					pinv += "ref ";
				pinv += (ptype + " arg" + pcnt);
				parms.Add(type);
				if (SymbolTable.IsObject(type)) {
					name += "Object";
					key += "Object";
				} else {
					name += SymbolTable.GetName(type);
					key += type;
				}
				pcnt++;
			}		 

			if (handlers.ContainsKey(name)) {
				return (String) handlers[name];
			}
			
			String dir;
			char sep = Path.DirectorySeparatorChar;
						
			if (key.IndexOf("Gtk") >= 0) {
				dir = ".." + sep + "gtk" + sep + "generated";
			} else if (key.IndexOf("Gdk") >= 0) {
				dir = ".." + sep + "gdk" + sep + "generated";
			} else if (key.IndexOf("Atk") >= 0) {
				dir = ".." + sep + "atk" + sep + "generated";
			} else if (key.IndexOf("Pango") >= 0) {
				dir = ".." + sep + "pango" + sep + "generated";
			} else if (key.IndexOf("Gnome") >= 0) {
				dir = ".." + sep + "gnome" + sep + "generated";
			} else {
				dir = ".." + sep + "glib" + sep + "generated";
			}

			String sname = name + "Signal";
			String dname = name + "Delegate";
			String cbname = name + "Callback";

			handlers[name] = sname;

			if (!Directory.Exists(dir)) {
				Directory.CreateDirectory(dir);
			}

			String filename = dir + sep + sname + ".cs";

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
					if (SymbolTable.IsObject((String)parms[idx])) {
						sw.Write("\t\t\targs.Args[" + (idx-1) + "] ");
						sw.WriteLine("= GLib.Object.GetObject(arg" + idx + ");");
					} else {
						string ctype = (string) parms[idx];
						sw.WriteLine("\t\t\targs.Args[" + (idx-1) + "] = " + SymbolTable.FromNative (ctype, "arg" + idx)  + ";");
						ClassBase wrapper = SymbolTable.GetClassGen (ctype);
						if ((wrapper != null && !(wrapper is InterfaceGen)) || SymbolTable.IsManuallyWrapped (ctype) || SymbolTable.IsBoxed (ctype)) {
							sw.WriteLine("\t\t\tif (args.Args[" + (idx-1) + "] == null)");
							sw.WriteLine("\t\t\t\targs.Args[{0}] = new {1}(arg{2});", idx-1, SymbolTable.GetCSType (ctype), idx);
						}
					}
				}
				sw.WriteLine("\t\t\tobject[] argv = new object[2];");
				sw.WriteLine("\t\t\targv[0] = inst._obj;");
				sw.WriteLine("\t\t\targv[1] = args;");
				sw.WriteLine("\t\t\tinst._handler.DynamicInvoke(argv);");
				if (retval != "void") {
					sw.WriteLine("\t\t\treturn (" + s_ret + ") args.RetVal;");
				}
				sw.WriteLine("\t\t}");
				sw.WriteLine();
			}
			sw.Write("\t\t[DllImport(\"gobject-2.0\")]");
			sw.Write("\t\tstatic extern void g_signal_connect_data(");
			sw.Write("IntPtr obj, String name, " + dname + " cb, int key, IntPtr p,");
			sw.WriteLine(" int flags);");
			sw.WriteLine();
			sw.Write("\t\tpublic " + sname + "(GLib.Object obj, IntPtr raw, ");
			sw.WriteLine("String name, MulticastDelegate eh, Type argstype) : base(obj, eh, argstype)");
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

