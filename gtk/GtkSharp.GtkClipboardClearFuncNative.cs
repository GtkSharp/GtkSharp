
namespace GtkSharp {

	using System;
	using System.Collections;
	public delegate void GtkClipboardClearFuncNative(IntPtr clipboard, uint objid);

	public class GtkClipboardClearFuncWrapper : GLib.DelegateWrapper {

		public void NativeCallback (IntPtr clipboard, uint objid)
		{
			if (RemoveIfNotAlive ()) return;
			object[] _args = new object[2];
			_args[0] = (Gtk.Clipboard) GLib.Object.GetObject(clipboard);
			if (_args[0] == null)
				_args[0] = new Gtk.Clipboard(clipboard);
                        _args[1] = Gtk.Clipboard.clipboard_objects[objid];
			_managed ((Gtk.Clipboard) _args[0], _args[1]);
                        Gtk.Clipboard.clipboard_objects.Remove (objid);
		}

		public GtkClipboardClearFuncNative NativeDelegate;
		protected Gtk.ClipboardClearFunc _managed;

		public GtkClipboardClearFuncWrapper (Gtk.ClipboardClearFunc managed, object o) : base (o)
		{
			NativeDelegate = new GtkClipboardClearFuncNative (NativeCallback);
			_managed = managed;
		}
	}

}
