// SelectionModelAdapter.cs - Gtk SelectionModel adapter customizations
//
// This code is inserted after the automatically generated code.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	// GtkSelectionModel's GIR declares <prerequisite name="Gio.ListModel"/>:
	// every selection model IS a list model, and the selection interface has no
	// way to ask what is in the model. GirToGapi drops prerequisites and the
	// api.xml has no place to put them, so the generated interface derives from
	// GLib.IWrapper alone.
	//
	// That was invisible while every selection model a program held was a
	// bound concrete type -- Gtk.SingleSelection and Adw.ViewStackPages are
	// generated as `: GLib.Object, GLib.IListModel, Gtk.ISelectionModel`, so
	// the cast to GLib.IListModel succeeded on the object itself. But
	// SelectionModelAdapter.GetObject falls back to wrapping the handle in the
	// adapter whenever the concrete GType is not one this binding knows, and
	// GtkStackPages is a private type that appears in no gir. So
	// `Gtk.Stack.Pages` -- the only way in Gtk 4 to enumerate a stack's pages,
	// and the object a StackSwitcher is driven from -- came back as an adapter
	// that threw InvalidCastException on `(GLib.IListModel) pages` and had no
	// NItems, no GetObject and no items-changed of its own.
	//
	// The members are implemented explicitly, so they are reached through
	// ISelectionModel (which now carries GLib.IListModel with it) rather than
	// colliding with the adapter's own static GetObject overloads.
	public partial interface ISelectionModel : GLib.IListModel {
	}

	public partial class SelectionModelAdapter : GLib.IListModel {

		private GLib.IListModel _asListModel;

		private GLib.IListModel AsListModel {
			get {
				if (_asListModel == null)
					_asListModel = new GLib.ListModelAdapter (Handle);
				return _asListModel;
			}
		}

		event GLib.ItemsChangedHandler GLib.IListModel.ItemsChanged {
			add { AsListModel.ItemsChanged += value; }
			remove { AsListModel.ItemsChanged -= value; }
		}

		IntPtr GLib.IListModel.GetItem (uint position)
		{
			return AsListModel.GetItem (position);
		}

		GLib.GType GLib.IListModel.ItemType {
			get { return AsListModel.ItemType; }
		}

		uint GLib.IListModel.NItems {
			get { return AsListModel.NItems; }
		}

		GLib.Object GLib.IListModel.GetObject (uint position)
		{
			return AsListModel.GetObject (position);
		}

		void GLib.IListModel.EmitItemsChanged (uint position, uint removed, uint added)
		{
			AsListModel.EmitItemsChanged (position, removed, added);
		}
	}
}
