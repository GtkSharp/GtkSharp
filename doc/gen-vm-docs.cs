namespace GtkSharp.Docs {

	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Xml.XPath;

	public class GenVMDocs  {

		public static int Main (string[] args)
		{
			string api_filename = "";
			XmlDocument api_doc = new XmlDocument ();

			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			foreach (string arg in args) {

				Assembly assembly;
				try {
					assembly = Assembly.LoadFile (arg);
				} catch (XmlException e) {
					Console.WriteLine (e);
					return 1;
				}

				foreach (Type t in assembly.GetTypes ()) {
					if (!t.IsSubclassOf (typeof (GLib.Object)))
						continue;

					Hashtable sigs = new Hashtable ();

					foreach (EventInfo ei in t.GetEvents (flags)) {
						foreach (Attribute attr in ei.GetCustomAttributes (false)) {
							if (attr.ToString () == "GLib.SignalAttribute") {
								sigs [((GLib.SignalAttribute) attr).CName] = ei.Name;
								break;
							}
						}
					}

					if (sigs.Count == 0) continue;

					Hashtable vms = new Hashtable ();

					foreach (MethodInfo mi in t.GetMethods (flags)) {
						foreach (Attribute attr in mi.GetCustomAttributes (false)) {
							if (attr.ToString () == "GLib.DefaultSignalHandlerAttribute") {
								string conn_name = ((GLib.DefaultSignalHandlerAttribute) attr).ConnectionMethod;
								if (sigs.ContainsValue (conn_name.Substring (8)))
									vms [mi.Name] = conn_name.Substring (8);
								break;
							}
						}
					}

					if (vms.Count == 0) continue;

					string filename = "en/" + t.Namespace + "/" + t.Name + ".xml";

					try {
						Stream stream = File.OpenRead (filename);
						api_doc.Load (stream);
						stream.Close ();
						Console.WriteLine ("opened:" + filename);
					} catch (XmlException e) {
						Console.WriteLine (e);
						return 1;
					}

					XPathNavigator api_nav = api_doc.CreateNavigator ();

					foreach (string vm in vms.Keys) {

						XPathNodeIterator iter = api_nav.Select ("/Type/Members/Member[@MemberName='" + vm + "']");
						if (iter.MoveNext ()) {
							XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
							XmlElement summ = elem ["Docs"] ["summary"];
							XmlElement rem = elem ["Docs"] ["remarks"];
							string summary = summ.InnerXml;
							string remarks = rem.InnerXml;
							if (summary == "To be added" && remarks == "To be added") {
								summ.InnerXml = "Default handler for the <see cref=\"M:" + t + "." + vms [vm] + "\" /> event.";
								rem.InnerXml = "Override this method in a subclass to provide a default handler for the <see cref=\"M:" + t + "." + vms [vm] + "\" /> event.";
							} else
								Console.WriteLine ("Member had docs:" + vm);
						} else {
							Console.WriteLine ("Member not found:" + vm);
						}

						api_doc.Save (filename);
					}
				}
			}
			return 0;
		}
	}
}
