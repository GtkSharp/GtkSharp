
using System.Xml;

namespace GtkSharp.Generation
{
	public class UnionGen : StructBase {

		public UnionGen (XmlElement ns, XmlElement elem) : base (ns, elem)
		{
		}

		public override bool Union {
			get {
				return true;
			}
		}
	}
}
