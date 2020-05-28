using GtkSharp;

namespace Gtk
{

	using System;
	using System.Runtime.InteropServices;

	public partial class CellRenderer
	{
		static GetSizeNativeDelegate GetSize_cb_delegate;

		static GetSizeNativeDelegate GetSizeVMCallback {
			get {
				if (GetSize_cb_delegate == null)
					GetSize_cb_delegate = new GetSizeNativeDelegate (GetSize_cb);
				return GetSize_cb_delegate;
			}
		}

		static void OverrideGetSize (GLib.GType gtype)
		{
			OverrideGetSize (gtype, GetSizeVMCallback);
		}

		/// <summary>
		/// sets callback GetSize in GtkCellRendererClass-Struct
		/// </summary>
		/// <param name="gtype"></param>
		/// <param name="callback"></param>
		static void OverrideGetSize (GLib.GType gtype, GetSizeNativeDelegate callback)
		{
			unsafe {
				IntPtr* raw_ptr = (IntPtr*) (((long) gtype.GetClassPtr ()) + (long) class_abi.GetFieldOffset ("get_size"));
				*raw_ptr = Marshal.GetFunctionPointerForDelegate ((Delegate) callback);
			}
		}

		// We have to implement this VM manually because x_offset, y_offset, width and height params may be NULL and therefore cannot be treated as "out int"
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void GetSizeNativeDelegate (IntPtr item, IntPtr widget, IntPtr cell_area_ptr, IntPtr x_offset, IntPtr y_offset, IntPtr width, IntPtr height);

		static void GetSize_cb (IntPtr item, IntPtr widget, IntPtr cell_area_ptr, IntPtr x_offset, IntPtr y_offset, IntPtr width, IntPtr height)
		{
			try {
				CellRenderer obj = GLib.Object.GetObject (item, false) as CellRenderer;
				Gtk.Widget widg = GLib.Object.GetObject (widget, false) as Gtk.Widget;
				Gdk.Rectangle cell_area = Gdk.Rectangle.Zero;
				if (cell_area_ptr != IntPtr.Zero)
					cell_area = Gdk.Rectangle.New (cell_area_ptr);
				int a, b, c, d;

				obj.OnGetSize (widg, ref cell_area, out a, out b, out c, out d);
				if (x_offset != IntPtr.Zero)
					Marshal.WriteInt32 (x_offset, a);
				if (y_offset != IntPtr.Zero)
					Marshal.WriteInt32 (y_offset, b);
				if (width != IntPtr.Zero)
					Marshal.WriteInt32 (width, c);
				if (height != IntPtr.Zero)
					Marshal.WriteInt32 (height, d);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		[GLib.DefaultSignalHandler (Type = typeof(Gtk.CellRenderer), ConnectionMethod = nameof(OverrideGetSize))]
		protected virtual void OnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			InternalOnGetSize (widget, ref cell_area, out x_offset, out y_offset, out width, out height);
		}

		private void InternalOnGetSize (Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			IntPtr native_cell_area = GLib.Marshaller.StructureToPtrAlloc (cell_area);
			gtksharp_cellrenderer_base_get_size (Handle, widget?.Handle ?? IntPtr.Zero, native_cell_area, out x_offset, out y_offset, out width, out height);
			cell_area = Gdk.Rectangle.New (native_cell_area);
			Marshal.FreeHGlobal (native_cell_area);
		}

		static GetSizeNativeDelegate InternalGetSizeNativeDelegate (GLib.GType gtype)
		{
			GetSizeNativeDelegate unmanaged = null;
			unsafe {
				IntPtr* raw_ptr = (IntPtr*) (((long) gtype.GetClassPtr ()) + (long) class_abi.GetFieldOffset ("get_size"));
				if (*raw_ptr == IntPtr.Zero)
					return default;

				unmanaged = Marshal.GetDelegateForFunctionPointer<GetSizeNativeDelegate> (*raw_ptr);
			}

			return unmanaged;
		}

		static void gtksharp_cellrenderer_base_get_size (IntPtr cell, IntPtr widget, IntPtr cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			/*
			const gchar *__gtype_prefix = "__gtksharp_";
			#define HAS_PREFIX(a) (*((guint64 *)(a)) == *((guint64 *) __gtype_prefix))

			static GObjectClass *
			get_threshold_class (GObject *obj)
			{
				GType gtype = G_TYPE_FROM_INSTANCE (obj);
				while (HAS_PREFIX (g_type_name (gtype)))
					gtype = g_type_parent (gtype);
				GObjectClass *klass = g_type_class_peek (gtype);
				if (klass == NULL) klass = g_type_class_ref (gtype);
				return klass;
			}			 
			 
			 void gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height)
			{
				GtkCellRendererClass *klass = (GtkCellRendererClass *) get_threshold_class (G_OBJECT (cell));
				if (klass->get_size)
					(* klass->get_size) (cell, widget, cell_area, x_offset, y_offset, width, height);
			}
			 */

			x_offset = 0;
			y_offset = 0;
			width = 0;
			height = 0;

			if (cell == IntPtr.Zero) {
				return;
			}

			var obj = TryGetObject (cell);
			if (obj == default)
				return;
			var parent = obj.NativeType;
			while ((parent = parent.GetBaseType ()) != GLib.GType.None) {
				
				if (parent.ToString ().StartsWith ("__gtksharp_")) {
					continue;
				}

				var GetSize = InternalGetSizeNativeDelegate (parent);
				if (GetSize != default) {
					var a = Marshal.AllocHGlobal (sizeof(int));
					var b = Marshal.AllocHGlobal (sizeof(int));
					var c = Marshal.AllocHGlobal (sizeof(int));
					var d = Marshal.AllocHGlobal (sizeof(int));
					try {
						GetSize (cell, widget, cell_area, a, b, c, d);
						if (a != IntPtr.Zero)
							Marshal.WriteInt32 (a, x_offset);
						if (b != IntPtr.Zero)
							Marshal.WriteInt32 (b, y_offset);
						if (c != IntPtr.Zero)
							Marshal.WriteInt32 (c, width);
						if (d != IntPtr.Zero)
							Marshal.WriteInt32 (d, height);
					} finally {
						Marshal.FreeHGlobal (a);
						Marshal.FreeHGlobal (b);
						Marshal.FreeHGlobal (c);
						Marshal.FreeHGlobal (d);
					}
				}

				return;
			}
		}

	}

}