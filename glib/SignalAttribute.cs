//
// SignalAttribute.cs
//
// Author:
//   Ricardo Fernández Pascual <ric@users.sourceforge.net>
//
// (C) Ricardo Fernández Pascual <ric@users.sourceforge.net>
//

namespace GLib {

	using System;

	/// <summary>
	///   Marks events genrated from glib signals
	/// </summary>
	///
	/// <remarks>
	///   This attribute indentifies events generated from glib signals 
	///   and allows obtaining its original name.
	/// </remarks>
	[Serializable]
	public sealed class SignalAttribute : Attribute 
	{
		private string cname;

		public SignalAttribute (string cname)
		{
			this.cname = cname;
		}

		private SignalAttribute () {}

		public string CName 
		{
			get {
				return cname;
			}
		}
	}
}
