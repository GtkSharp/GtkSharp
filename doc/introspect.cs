// introspect.cs: Add introspectable information to a docs file.
//
// Author: Rachel Hestilow <hestilow@ximian.com>
//
// (c) 2002 Rachel Hestilow

namespace GtkSharp.DocGeneration {
	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using System.Xml;

	public class Introspect {
		Hashtable assemblies = new Hashtable ();
		Hashtable primitives = new Hashtable ();
		XmlDocument doc;
		string current_ns;

		public Introspect (XmlDocument doc) {
			this.doc = doc;
			primitives["System.Boolean"] = "bool";
			primitives["System.Byte"] = "byte";
			primitives["System.Char"] = "char";
			primitives["System.Decimal"] = "decimal";
			primitives["System.Double"] = "double";
			primitives["System.Int16"] = "int16";
			primitives["System.Int32"] = "int";
			primitives["System.Int64"] = "int64";
			primitives["System.Object"] = "object";
			primitives["System.SByte"] = "sbyte";
			primitives["System.Single"] = "single";
			primitives["System.String"] = "string";
			primitives["System.UInt16"] = "uint16";
			primitives["System.UInt32"] = "uint";
			primitives["System.UInt64"] = "uint64";
			primitives["System.Void"] = "void";

			/* FIXME: mcs does not support Assembly.GetReferencedAssemblies*/
			foreach (string asm in new string[] {"glib", "atk", "pango", "gdk"}) {
				string key = asm + "-sharp";
				assemblies[key] = Assembly.Load (key);
			}
		}

		public Type LookupType (string typename)
		{
			foreach (Assembly assembly in assemblies.Values) {
				Type type = assembly.GetType (typename);
				if (type != null)
					return type;
			}
			return Type.GetType (typename);
		}

		public string StringifyType (Type type)
		{
			string full = type.ToString ();
			bool isArray;
			if (full.EndsWith ("[]")) {
				full = full.Substring (0, full.Length - 2);
				isArray = true;
			} else
				isArray = false;

			if (primitives.Contains (full)) {
				string ret = (string) primitives[full];
				if (isArray) ret += "[]";
				return ret;
			} else {
				if (String.Compare (full, 0, current_ns, 0, current_ns.Length) == 0) {
					full = full.Substring (current_ns.Length + 1);
				}
				return full;
			}
		}

		public void FixArgs (XmlElement method_node, Type type, Assembly asm, string method_name, string orig, bool isCtor)
		{
			XmlNode args_node = doc.CreateNode ("element", "arguments", "");
			method_node.AppendChild (args_node);

			if (orig == "") return;
			string[] args = orig.Split (',');
			Type[] signature = new Type[args.Length];
			
			int i = 0;
			foreach (string arg in args) {
				string fix = arg.Trim ('@');
				signature[i] = LookupType (fix);
				i++;
			}

			MethodBase method = null;
			MethodBase[] methods;
			if (isCtor)
			{
				MemberInfo[] bases = type.FindMembers (MemberTypes.Constructor | MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance, Type.FilterName, ".ctor");
				ArrayList ctors = new ArrayList ();
				foreach (MemberInfo info in bases) {
					if (info.MemberType == MemberTypes.Constructor)
						ctors.Add (info);
				}
				methods = new MethodBase[ctors.Count];
				ctors.CopyTo (methods);
			}
			else
				methods = type.GetMethods ();

			foreach (MethodBase m in methods) {
				if (m.GetParameters () == null)
					continue;
				if (m.Name != method_name)
					continue;
				if (m.GetParameters ().Length != signature.Length)
					continue;
				bool valid = true;
				for (i = 0; i < signature.Length; i++) {
					// FIXME: cludge
					string t1 = m.GetParameters ()[i].ParameterType.FullName;
					t1 = t1.Trim ('&');
					if (t1 != signature[i].FullName) {
						valid = false;
						break;
					}
				}
				if (!valid) continue;
				method = m;
				break;
			}

			i = 0;
			foreach (ParameterInfo p in method.GetParameters ()) {
				string modifiers = "";
				// FIXME: another mono bug...this is always false 
				if (p.IsOut || p.IsRetval)
					modifiers += "out ";
				XmlElement arg_node = (XmlElement) doc.CreateNode ("element", "argument", "");
				args_node.AppendChild (arg_node);
				arg_node.SetAttribute ("modifiers", modifiers);
				arg_node.SetAttribute ("type", StringifyType (signature[i]));
				arg_node.SetAttribute ("name", p.Name);
				i++;
			}
		}

		public void FixProperty (XmlElement prop, Assembly asm, Type type)
		{
			PropertyInfo[] props = type.GetProperties (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			string prop_name = prop.GetAttribute ("name");
			
			PropertyInfo pinfo = null;
			foreach (PropertyInfo i in props) {
				if (i.Name == prop_name) {
					pinfo = i;
					break;
				}
			}

			// FIXME: mono bug: why is PropertyType sometimes null?
			if (pinfo.PropertyType != null) 
				prop.SetAttribute ("type", StringifyType (pinfo.PropertyType));
			else
				prop.SetAttribute ("type", "[unknown]");
		}

		public void FixMethod (XmlElement method, Assembly asm, Type type)
		{
			MethodInfo[] methods = type.GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			string method_name = method.GetAttribute ("name");
			string op = "op_Explicit -> ";
			
			if (String.Compare (method_name, 0, op, 0, op.Length) == 0) {
				method.SetAttribute ("type", StringifyType (LookupType (method_name.Substring (op.Length))));
				method.SetAttribute ("name", "op_Explicit");
				return;
			}
			
			FixArgs (method, type, asm, method_name, method.GetAttribute ("args"), false); 
			
			MethodInfo minfo = null;
			foreach (MethodInfo i in methods) {
				if (i.Name == method_name) {
					minfo = i;
					break;
				}
			}
			method.SetAttribute ("type", StringifyType (minfo.ReturnType));
		}

		public void FixConstructor (XmlElement method, Assembly asm, Type type)
		{
			string method_name = method.GetAttribute ("name");
			
			FixArgs (method, type, asm, ".ctor", method.GetAttribute ("args"), true); 
		}

		public bool BaseClassImplements (Type type, Type target)
		{
			if (type.BaseType == Type.GetType ("System.Object"))
				return false;
			foreach (Type iface in type.BaseType.GetInterfaces ()) {
				if (iface == target) 
					return true;
			}

			return BaseClassImplements (type.BaseType, target);
		}

		public void FixClass (XmlElement klass, string ns)
		{
			current_ns = ns;
			string asm = klass.GetAttribute ("assembly");
			Assembly assembly;
			if (assemblies.Contains (asm))
				assembly = (Assembly) assemblies[asm];
			else {
				assembly = Assembly.Load (asm);
				assemblies[asm] = assembly;
			}

			Type type = LookupType (ns + "." + klass.GetAttribute ("name"));
			if (type.BaseType != null && type.BaseType != Type.GetType ("System.Object") && type.BaseType != Type.GetType ("System.Enum"))
				klass.SetAttribute ("base", StringifyType (type.BaseType));

			Type[] unfiltered = type.GetInterfaces ();
			ArrayList ifaces = new ArrayList ();
			if (unfiltered != null && unfiltered.Length > 0) {
				foreach (Type iface in unfiltered) {
					if (type.BaseType == null || !BaseClassImplements (type, iface))
						ifaces.Add (iface);
				}
			}

			if (ifaces.Count > 0) {
				XmlNode implements = doc.CreateNode ("element", "implements", "");
				klass.AppendChild (implements);
				foreach (Type iface in ifaces) {
					XmlNode iface_node = doc.CreateNode ("element", "interface", "");
					implements.AppendChild (iface_node);
					XmlText text = (XmlText) doc.CreateNode ("text", "", "");
					text.Value = StringifyType (iface);
					iface_node.AppendChild (text);
				}
			}

			foreach (XmlNode member in klass.ChildNodes) {
				switch (member.Name) {
				case "property":
					FixProperty ((XmlElement) member, assembly, type);
					break;
				case "method":
					FixMethod ((XmlElement) member, assembly, type);
					break;
				case "constructor":
					FixConstructor ((XmlElement) member, assembly, type);
					break;
				default:
					break;
				}
			}
		}
		
		public static int Main (string[] args)
		{
			if (args.Length != 2) {
				Console.WriteLine ("usage: introspect <infile> <outfile>");
				return 0;
			}

			XmlDocument doc = new XmlDocument ();
			try {
				doc.Load (args[0]);
			} catch (XmlException e) {
				Console.WriteLine ("Failed to load {0}", args[0]);
				return 1;
			}

			Introspect introspector = new Introspect (doc);

			XmlElement root = doc.DocumentElement;
			foreach (XmlNode ns in root.ChildNodes) {
				if (ns.Name != "namespace") continue;

				XmlElement ns_elem = (XmlElement) ns;
				foreach (XmlNode klass in ns.ChildNodes) {
					if (klass.Name != "class") continue;
					introspector.FixClass ((XmlElement) klass, ns_elem.GetAttribute ("name"));
				}
			}

			FileStream stream = new FileStream (args[1], FileMode.Create, FileAccess.Write);
			doc.Save (stream);

			return 0;
		}
	}
}
