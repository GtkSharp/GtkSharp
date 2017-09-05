// gen-vm-docs.cs - Generate documentation for virtual methods.
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
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

					foreach (EventInfo ei in t.GetEvents (flags))
						foreach (GLib.SignalAttribute attr in ei.GetCustomAttributes (typeof (GLib.SignalAttribute), false))
							sigs [attr.CName] = ei.Name;
					

					if (sigs.Count == 0) continue;

					Hashtable vms = new Hashtable ();

					foreach (MethodInfo mi in t.GetMethods (flags)) {
						foreach (GLib.DefaultSignalHandlerAttribute attr in mi.GetCustomAttributes (typeof (GLib.DefaultSignalHandlerAttribute), false)) {
							string conn_name = attr.ConnectionMethod;
							if (sigs.ContainsValue (conn_name.Substring (8)))
								vms [mi.Name] = conn_name.Substring (8);
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

					bool dirty = false;
					foreach (string vm in vms.Keys) {

						XPathNodeIterator iter = api_nav.Select ("/Type/Members/Member[@MemberName='" + vm + "']");
						if (iter.MoveNext ()) {
							XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
							XmlElement summ = elem ["Docs"] ["summary"];
							XmlElement rem = elem ["Docs"] ["remarks"];
							string summary = summ.InnerXml;
							string remarks = rem.InnerXml;
							if (summary == "To be added." && remarks == "To be added.") {
								summ.InnerXml = "Default handler for the <see cref=\"M:" + t + "." + vms [vm] + "\" /> event.";
								rem.InnerXml = "Override this method in a subclass to provide a default handler for the <see cref=\"M:" + t + "." + vms [vm] + "\" /> event.";
								dirty = true;
							} else
								Console.WriteLine ("Member had docs:" + vm);
						} else {
							Console.WriteLine ("Member not found:" + vm);
						}

						if (dirty)
							api_doc.Save (filename);
					}
				}
			}
			return 0;
		}
	}
}
