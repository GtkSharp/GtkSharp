// TreeNodeAttribute.cs - Attribute to specify TreeNode information for a class
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// <c> 2003 Novell, Inc.

namespace Gtk {

	using System;

	[AttributeUsage(AttributeTargets.Class)]
	public class TreeNodeAttribute : Attribute {
		int col_count;

		public int ColumnCount {
			get {
				return col_count;
			}
			set {
				col_count = value;
			}
		}
	}
}

