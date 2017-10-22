using System;
using System.Xml;

namespace GtkSharp.Generation
{
	public static class XmlElementExtensions
	{
		public static bool GetAttributeAsBoolean (this XmlElement elt, string name)
		{
			string value = elt.GetAttribute (name);

			if (String.IsNullOrEmpty (value)) {
				return false;
			} else {
				return XmlConvert.ToBoolean (value);
			}
		}
	}
}

