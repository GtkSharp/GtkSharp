// TreeNodeValueAttribute.cs - Attribute to mark properties as TreeNode column values
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// <c> 2003 Novell, Inc.

namespace Gtk {

	using System;

	[AttributeUsage(AttributeTargets.Property)]
	public class TreeNodeValueAttribute : Attribute {
		int col;

		public int Column {
			get {
				return col;
			}
			set {
				col = value;
			}
		}
	}
}

