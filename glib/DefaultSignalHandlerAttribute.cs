//
// DefaultSignalHandlerAttribute.cs
//
// Author:   Mike Kestner  <mkestner@ximian.com>
//
// (C) 2003 Novell, Inc.
//

namespace GLib {

	using System;

	public sealed class DefaultSignalHandlerAttribute : Attribute 
	{
		private string method;
		private System.Type type;

		public DefaultSignalHandlerAttribute () {}

		public string ConnectionMethod 
		{
			get {
				return method;
			}
			set {
				method = value;
			}
		}

		public System.Type Type 
		{
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
}
