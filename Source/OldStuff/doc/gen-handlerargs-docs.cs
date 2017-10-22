// gen-handlerargs-docs.cs - Generate documentation for event handlers/args
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

	public class GenHandlerArgsDocs  {

		public static int Main (string[] args)
		{
			Hashtable hndlrs = new Hashtable ();
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

					foreach (EventInfo ei in t.GetEvents (flags)) {
						foreach (Attribute attr in ei.GetCustomAttributes (false)) {
							if (attr.ToString () == "GLib.SignalAttribute") {
								if (ei.EventHandlerType.ToString() == "System.EventHandler") 
									break;
								ArrayList sigs;
								if (hndlrs.Contains (ei.EventHandlerType))
									sigs = hndlrs [ei.EventHandlerType] as ArrayList;
								else {
									sigs = new ArrayList ();
									hndlrs [ei.EventHandlerType] = sigs;
								}
										
								sigs.Add (t + "." + ei.Name);
								break;
							}
						}
					}
				}
			}

			if (hndlrs.Count == 0) 
				return 0;

			foreach (Type hndlr in hndlrs.Keys) {

				string filename = "en/" + hndlr.Namespace + "/" + hndlr.Name + ".xml";

				try {
					Stream stream = File.OpenRead (filename);
					api_doc.Load (stream);
					stream.Close ();
				} catch (XmlException e) {
					Console.WriteLine (e);
					return 1;
				}

				Type arg_type = hndlr.GetMethod ("Invoke").GetParameters ()[1].ParameterType;

				XPathNavigator api_nav = api_doc.CreateNavigator ();
				XPathNodeIterator iter = api_nav.Select ("/Type/Docs");
				if (iter.MoveNext ()) {
					XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
					XmlElement summ = elem ["summary"];
					XmlElement rem = elem ["remarks"];
					string summary = summ.InnerXml;
					string remarks = rem.InnerXml;
					if (summary == "To be added." && remarks == "To be added.") {
						Console.WriteLine (filename + ": Documenting summary and remarks");
						summ.InnerXml = "Event handler.";
						ArrayList sigs = hndlrs[hndlr] as ArrayList;
						string rems;
						if (sigs.Count > 1) {
							rems = "<para>The following events utilize this delegate:</para><para><list type=\"bullet\">";
							foreach (string ev in sigs)
								rems += "<item><term><see cref=\"M:" + ev + "\"/></term></item>";
							rems += "</list></para>";
						} else
							rems = "<para>The <see cref=\"M:" + sigs[0] + "\"/> event utilizes this delegate:</para>";
						rems += "<para>Event data is passed via the <see cref=\"T:" + arg_type + "\"/> parameter.</para><para>To attach a <see cref=\"T:" + hndlr + "\"/> to an event, add the " + hndlr.Name + " instance to the event.  The methods referenced by the " + hndlr.Name + " instance are invoked whenever the event is raised, until the " + hndlr.Name + " is removed from the event.</para>";
						rem.InnerXml = rems;
					}
					XPathNavigator param_nav = api_doc.CreateNavigator ();
					XPathNodeIterator param_iter = param_nav.Select ("/Type/Docs/param");
					while (param_iter.MoveNext ()) {
						XmlElement param = ((IHasXmlNode)param_iter.Current).GetNode () as XmlElement;
						if (param.InnerXml == "To be added.") {
							string param_name = param.GetAttribute ("name");
							switch (param_name) {
							case "o":
								param.InnerXml = "Event sender.";
								break;
							case "args":
								param.InnerXml = "Event arguments.";
								break;
							default:
								Console.WriteLine (filename + ": Unexpected param " + param.GetAttribute ("name"));
								break;
							}
							Console.WriteLine (filename + ": Documenting param " + param.GetAttribute ("name"));
						}
					}
				}
				api_doc.Save (filename);

				filename = "en/" + arg_type.Namespace + "/" + arg_type.Name + ".xml";

				try {
					Stream stream = File.OpenRead (filename);
					api_doc.Load (stream);
					stream.Close ();
				} catch (XmlException e) {
					Console.WriteLine (e);
					return 1;
				}

				api_nav = api_doc.CreateNavigator ();
				iter = api_nav.Select ("/Type/Docs");
				if (iter.MoveNext ()) {
					XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
					XmlElement summ = elem ["summary"];
					XmlElement rem = elem ["remarks"];
					string summary = summ.InnerXml;
					string remarks = rem.InnerXml;
					if (summary == "To be added." && remarks == "To be added.") {
						Console.WriteLine (filename + ": Documenting summary and remarks");
						summ.InnerXml = "Event data.";
						ArrayList sigs = hndlrs[hndlr] as ArrayList;
						string rems;
						if (sigs.Count > 1) {
							rems = "<para>The following events invoke <see cref=\"T:" + hndlr + "\"/> delegates which pass event data via this class:</para><para><list type=\"bullet\">";
							foreach (string ev in sigs)
								rems += "<item><term><see cref=\"M:" + ev + "\"/></term></item>";
							rems += "</list></para>";
						} else
							rems = "<para>The <see cref=\"M:" + sigs[0] + "\"/> event invokes <see cref=\"T:" + hndlr + "\"/> delegates which pass event data via this class.</para>";
						rem.InnerXml = rems;
					}
				}

				api_nav = api_doc.CreateNavigator ();
				iter = api_nav.Select ("/Type/Members/Member[@MemberName='.ctor']");
				if (iter.MoveNext ()) {
					XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
					XmlElement summ = elem ["Docs"] ["summary"];
					XmlElement rem = elem ["Docs"] ["remarks"];
					XmlElement ret = elem ["Docs"] ["returns"];
					string summary = summ.InnerXml;
					string remarks = rem.InnerXml;
					if (summary == "To be added." && remarks == "To be added.") {
						Console.WriteLine (filename + ": Documenting constructor");
						summ.InnerXml = "Public Constructor.";
						if (ret != null)
							ret.InnerXml = "A new <see cref=\"T:" + arg_type + "\"/>.";
						rem.InnerXml = "Create a new <see cref=\"T:" + arg_type + "\"/> instance with this constructor if you need to invoke a <see cref=\"T:" + hndlr + "\"/> delegate.";
					}
				}
				api_doc.Save (filename);

			}
			return 0;
		}
	}
}
