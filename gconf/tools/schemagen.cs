namespace GConf.Tools
{
	using GConf;
	using System;
	using System.Xml;
	using System.Collections;
	using System.IO;
	using System.Text;

	class SchemaGen
	{
		private Hashtable keys = new Hashtable ();
		private Hashtable classes = new Hashtable ();

		static void Die (string error)
		{
			Console.WriteLine (error);
			Environment.Exit (1);
		}

		void DieInvalid (string filename)
		{
			Die (filename + " is an invalid schema");
		}

		void Parse (string filename)
		{
			XmlDocument doc = new XmlDocument ();
			try {
				Stream stream = File.OpenRead (filename);
				doc.Load (stream);
				stream.Close ();
			} catch (XmlException e) {
				Die ("Could not parse " + filename);
			}

			XmlElement root = doc.DocumentElement;
			if (!(root != null && root.HasChildNodes && root.Name == "gconfschemafile"))
				DieInvalid (filename);

			foreach (XmlNode slist in root.ChildNodes)
			{
				if (!(slist != null && slist.Name == "schemalist"))
					continue;

				foreach (XmlNode schema_node in slist.ChildNodes)
				{
					XmlElement schema = schema_node as XmlElement;
					if (!(schema != null && schema.Name == "schema"))
						continue;
					
					XmlElement key = schema["applyto"];
					XmlElement type = schema["cstype"];
					if (type == null)
						type = schema["type"];
					if (key == null || type == null)
						DieInvalid (filename);
					keys.Add (key.InnerText, type.InnerText);

					if (type.HasAttribute ("class"))
						classes.Add (key.InnerText, type.GetAttribute ("class"));
				}
			}
		}

		string ExtractPrefix ()
		{
			string path = null;

			foreach (string key in keys.Keys)
			{
				int slash = key.LastIndexOf ('/');
				if (slash < 0)
					continue;
				
				string new_path = key.Substring (0, slash + 1);
				if (path == null || new_path.Length < path.Length)
					path = new_path;
			}

			return path;
		}

		string BaseName (string key)
		{
			int slash = key.LastIndexOf ('/');
			if (slash < 0)
				return key;
			return key.Substring (slash + 1);
		}

		string PropertyName (string key)
		{
			string basename = BaseName (key);
			StringBuilder sb = new StringBuilder ();
			
			bool needs_caps = true;
			foreach (char orig in basename)
			{
				char c = orig;
				if (c == '_' || c == '-')
				{
					needs_caps = true;
					continue;
				}
				
				if (needs_caps)
				{
					c = c.ToString ().ToUpper ()[0];
					needs_caps = false;
				}
				
				sb.Append (c);
			}

			return sb.ToString ();
		}

		string CSType (string key, string type)
		{
			switch (type)
			{
			case "float":
				return "double";
			case "string":
				return "string";
			case "int":
				return "int";
			case "bool":
				return "bool";
			case "color":
				return "System.Drawing.Color";
			case "enum":
				if (classes.Contains (key))
					return (string) classes[key];
				else
					return "string";
			case "list":
				return "System.Array";
			default:
				return null;
			}
		}

		void GenerateSimpleProperty (TextWriter sw, string key, string key_str, string type, string cstype)
		{
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn ({0}) client.Get ({1});", cstype, key_str);
			sw.WriteLine ("\t\t\t}");
			
			sw.WriteLine ("\t\t\tset {");
			sw.WriteLine ("\t\t\t\tclient.Set ({0}, value);", key_str);
			sw.WriteLine ("\t\t\t}");
		}

		void GenerateColorProperty (TextWriter sw, string key, string key_str, string type, string cstype)
		{
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\treturn System.Drawing.ColorTranslator.FromHtml ((string) client.Get ({0}));", key_str);
			sw.WriteLine ("\t\t\t}");
			
			sw.WriteLine ("\t\t\tset {");
			sw.WriteLine ("\t\t\t\tclient.Set ({0}, System.Drawing.ColorTranslator.ToHtml (value));", key_str);
			sw.WriteLine ("\t\t\t}");
		}

		void GenerateEnumProperty (TextWriter sw, string key, string key_str, string type, string cstype)
		{
			sw.WriteLine ("\t\t\tget {");
			sw.WriteLine ("\t\t\t\ttry {");
			sw.WriteLine ("\t\t\t\t\treturn ({0}) System.Enum.Parse (typeof ({0}), (string) client.Get ({1}));", cstype, key_str);
			sw.WriteLine ("\t\t\t\t} catch (System.Exception e) {");
			sw.WriteLine ("\t\t\t\t\treturn ({0}) 0;", cstype);
			sw.WriteLine ("\t\t\t\t}");
			sw.WriteLine ("\t\t\t}");
			
			sw.WriteLine ("\t\t\tset {");
			sw.WriteLine ("\t\t\t\tclient.Set ({0}, System.Enum.GetName (typeof ({1}), value));", key_str, cstype);
			sw.WriteLine ("\t\t\t}");
		}

		void GenerateEvent (TextWriter sw, string name, string key_str)
		{
			sw.WriteLine ();
			sw.WriteLine ("\t\tpublic static event GConf.NotifyEventHandler {0}Changed", name);
			sw.WriteLine ("\t\t{");
			sw.WriteLine ("\t\t\tadd {");
			sw.WriteLine ("\t\t\t\tclient.AddNotify ({0}, value);", key_str);
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t\tremove{");
			sw.WriteLine ("\t\t\t\tclient.RemoveNotify ({0}, value);", key_str);
			sw.WriteLine ("\t\t\t}");
			sw.WriteLine ("\t\t}");
		}

		void Generate (string ns_str)
		{
			string path = ExtractPrefix ();
			TextWriter sw = Console.Out;

			sw.WriteLine ("namespace {0}", ns_str);
			sw.WriteLine ("{");

			sw.WriteLine ("\tpublic class Settings");
			sw.WriteLine ("\t{");
			sw.WriteLine ("\t\tstatic GConf.Client client = new GConf.Client ();");

			GenerateEvent (sw, "", "\"" + path.Substring (0, path.Length - 1) + "\""); 

			foreach (string key in keys.Keys)
			{
				string type = (string) keys[key];
				string cstype = CSType (key, type);
				string key_str = "\"" + key + "\"";

				if (cstype == null)
				{
					Console.Error.WriteLine ("Warning: unknown type \"{0}\" for key {1}", type, key);
					continue;
				}

				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic static {0} {1}", cstype, PropertyName (key));
				sw.WriteLine ("\t\t{");

				switch (type)
				{
				case "color":
					GenerateColorProperty (sw, key, key_str, type, cstype);
					break;
				case "enum":
					GenerateEnumProperty (sw, key, key_str, type, cstype);
					break;
				default:
					GenerateSimpleProperty (sw, key, key_str, type, cstype);
					break;
				}

				sw.WriteLine ("\t\t}");

				GenerateEvent (sw, PropertyName (key), key_str);
			}

			sw.WriteLine ("\t}");
			sw.WriteLine ();
			sw.WriteLine ("\tpublic class SettingKeys");
			sw.WriteLine ("\t{");

			foreach (string key in keys.Keys)
			{
				sw.WriteLine ("\t\tpublic static string {0}", PropertyName (key));
				sw.WriteLine ("\t\t{");
				sw.WriteLine ("\t\t\tget {");
				sw.WriteLine ("\t\t\t\t return \"{0}\";", key);
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t}");
			}
			
			sw.WriteLine ("\t}");
			sw.WriteLine ("}");
		}

		public static void Main (string[] args)
		{
			if (args.Length < 2)
				Die ("Usage: gconfsharp-schemagen namespace schemafile");
			string ns = args[0];
			string filename = args[1];
			
			SchemaGen gen = new SchemaGen ();
			gen.Parse (filename);
			gen.Generate (ns);
		}
	}
}

