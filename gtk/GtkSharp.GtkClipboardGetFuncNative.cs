
namespace GtkSharp {

	using System;
	using System.Collections;
	public delegate void GtkClipboardGetFuncNative(IntPtr clipboard, ref Gtk.SelectionData selection_data, uint info, uint obj_id);

	public class GtkClipboardGetFuncWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr clipboard, ref Gtk.SelectionData selection_data, uint info, uint obj_id)
		{
			object[] _args = new object[4];
			_args[0] = (Gtk.Clipboard) GLib.Opaque.GetOpaque(clipboard);
			if (_args[0] == null)
				_args[0] = new Gtk.Clipboard(clipboard);
			_args[1] = selection_data;
			_args[2] = info;
                        _args[3] = Gtk.Clipboard.clipboard_objects[obj_id];

			_managed ((Gtk.Clipboard) _args[0], ref selection_data, (uint) _args[2], _args[3]);
		}

		public GtkClipboardGetFuncNative NativeDelegate;
		protected Gtk.ClipboardGetFunc _managed;

		public GtkClipboardGetFuncWrapper (Gtk.ClipboardGetFunc managed) : base ()
		{
			NativeDelegate = new GtkClipboardGetFuncNative (NativeCallback);
			_managed = managed;
		}
	}

}
