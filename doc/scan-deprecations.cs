namespace GtkSharp.Docs {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;

	public class ScanDeprecations  {

		public static int Main (string[] args)
		{
			string api_filename = "";
			XmlDocument api_doc = new XmlDocument ();

			foreach (string arg in args) {

				try {
					Stream stream = File.OpenRead (arg);
					api_doc.Load (stream);
					stream.Close ();
				} catch (XmlException e) {
					Console.WriteLine (e);
					return 1;
				}

				string ignores = "";
				string kills = "";
				string nonstubs = "";
				ArrayList kill_elems = new ArrayList ();

				XPathNavigator api_nav = api_doc.CreateNavigator ();
				XPathNodeIterator iter = api_nav.Select ("/Type/Members/Member[@Deprecated='true']");
				while (iter.MoveNext ()) {
					XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
					string member_type = elem["MemberType"].InnerText;
					switch (member_type) {
					case "Method":
					case "Property":
					case "Constructor":
					case "Field":
						string summary = elem["Docs"]["summary"].InnerText;
						string remarks = elem["Docs"]["remarks"].InnerText;
						if (summary == "To be added" && remarks == "To be added") {
							kills += " " + elem.GetAttribute ("MemberName") + "(" + member_type + ")";
							kill_elems.Add (elem);
						} else
							nonstubs += " " + elem.GetAttribute ("MemberName") + "(" + member_type + ")";
						break;
					default:
						ignores += " " + elem.GetAttribute ("MemberName") + "(" + member_type + ")";
						break;
					}
				}

				iter = api_nav.Select ("/Type/Base/BaseTypeName");
				if (iter.MoveNext ()) {
					XmlElement elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
					if (elem.InnerText == "System.Enum") {
						iter = api_nav.Select ("/Type/Members/Member[@MemberName='value__']");
						if (iter.MoveNext ()) {
							elem = ((IHasXmlNode)iter.Current).GetNode () as XmlElement;
							kills += " " + elem.GetAttribute ("MemberName") + "(Field)";
							kill_elems.Add (elem);
						}
					}
				}

				foreach (XmlNode node in kill_elems)
					node.ParentNode.RemoveChild (node);
					
				api_doc.Save (arg);

				if (ignores != "" || kills != "" || nonstubs != "") {
					Console.WriteLine (arg + ":");
					if (ignores != "")
						Console.WriteLine ("  Ignored:" + ignores);
					if (kills != "")
						Console.WriteLine ("  Killed:" + kills);
					if (nonstubs != "")
						Console.WriteLine ("  Non-stubbed deprecates:" + nonstubs);
				}

			}
			return 0;
		}
	}
}
