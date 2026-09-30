// Gtk.ICellLayout.cs - convenience over the generated ICellLayout
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	// SetAttributes used to be declared on the interface itself, which left every
	// implementor owing an implementation. Gtk 4's gir marks more types as
	// implementing GtkCellLayout -- GtkCellArea among them -- and codegen does not
	// synthesise the member for them, so the build broke on types that never had
	// to care. It is written purely in terms of ClearAttributes and AddAttribute,
	// both of which the interface already has, so an extension method serves every
	// implementor and works on netstandard2.0, where default interface members do
	// not.
	public static class CellLayoutExtensions {

		public static void SetAttributes (this ICellLayout layout, CellRenderer cell,
		                                  params object[] attrs)
		{
			if (attrs.Length % 2 != 0)
				throw new ArgumentException ("attrs should contain pairs of attribute/col");

			layout.ClearAttributes (cell);
			for (int i = 0; i < attrs.Length - 1; i += 2)
				layout.AddAttribute (cell, (string) attrs [i], (int) attrs [i + 1]);
		}
	}
}
