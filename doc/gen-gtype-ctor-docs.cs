// gen-gtype-ctor.cs - Generate documentation for Constructor (GType).
//
// Author: John Luke  <john.luke@gmail.com>
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

namespace GtkSharp.Docs
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Xml.XPath;

	public class GenGtypeConstructorDocs
	{
		public static int Main (string[] args)
		{
			XmlDocument api_doc = new XmlDocument ();

			foreach (string arg in args)
			{
				Assembly assembly;
				try
				{
					assembly = Assembly.LoadFile (arg);
				}
				catch (XmlException e)
				{
					Console.WriteLine (e);
					return 1;
				}

				foreach (Type t in assembly.GetTypes ())
				{
					if (!t.IsSubclassOf (typeof (GLib.Object)))
						continue;

					// protected .ctor (GLib.GType)
					ConstructorInfo ci = t.GetConstructor (BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {typeof (GLib.GType)}, null);
					if (ci == null)
						continue;

					string filename = "en/" + t.Namespace + "/" + t.Name + ".xml";

					try
					{
						Stream stream = File.OpenRead (filename);
						api_doc.Load (stream);
						stream.Close ();
					}
					catch (XmlException e)
					{
						Console.WriteLine (e);
						return 1;
					}

					XPathNavigator api_nav = api_doc.CreateNavigator ();

					XPathNodeIterator iter = api_nav.Select ("/Type/Members/Member[@MemberName='.ctor']/Parameters/Parameter[@Type='GLib.GType']");
					if (iter.MoveNext ())
					{
						XmlElement elem = ((IHasXmlNode)iter.Current).GetNode ().ParentNode.ParentNode as XmlElement;
						XmlElement summ = elem ["Docs"] ["summary"];
						XmlElement rem = elem ["Docs"] ["remarks"];
						XmlElement param = elem ["Docs"] ["param"];
						string summary = summ.InnerXml;
						string remarks = rem.InnerXml;
						if (summary == "To be added." && remarks == "To be added.")
						{
							Console.WriteLine (filename + ": Documenting ctor(GType)");
							summ.InnerXml = "Obsolete Protected Constructor.";
							rem.InnerXml = "Do not use.  Replaced by <see cref=\"M:GLib.Object.CreateNativeObject\" /> which registers native types automatically.  Subclasses should chain to the IntPtr constructor passing <see cref=\"M:System.IntPtr.Zero\" /> and call CreateNativeObject instead of using this constructor.  This constructor is provided for backward compatibility if you have manually registered a native <see cref=\"T:GLib.GType\" /> value for your subclass.";
							param.InnerXml = "Native type value.";
						}
					}
					api_doc.Save (filename);
				}
			}
		return 0;
		}
	}
}
