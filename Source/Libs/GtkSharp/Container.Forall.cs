using GtkSharp;

namespace Gtk
{
	using System;
	using System.Runtime.InteropServices;

	public partial class Container
	{
		internal static D TryGetDelegate<D>(IntPtr _cb)
		{
			try {
				var d = Marshal.GetDelegateForFunctionPointer<D>(_cb);
				return d;
			} catch {
				return default;
			}
		}

		static ForAllNativeDelegate ForAll_cb_delegate;

		static ForAllNativeDelegate ForAllVMCallback {
			get {
				if (ForAll_cb_delegate == null)
					ForAll_cb_delegate = new ForAllNativeDelegate(ForAll_cb);
				return ForAll_cb_delegate;
			}
		}

		static void OverrideForAll(GLib.GType gtype)
		{
			OverrideForAll(gtype, ForAllVMCallback);
		}

		/// <summary>
		/// sets callback forall in GtkContainerClass-Struct
		/// </summary>
		/// <param name="gtype"></param>
		/// <param name="callback"></param>
		static void OverrideForAll(GLib.GType gtype, ForAllNativeDelegate callback)
		{
			unsafe {
				IntPtr* raw_ptr = (IntPtr*) (((long) gtype.GetClassPtr()) + (long) class_abi.GetFieldOffset("forall"));
				*raw_ptr = Marshal.GetFunctionPointerForDelegate((Delegate) callback);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ForAllNativeDelegate(IntPtr inst, bool include_internals, IntPtr cb, IntPtr data);

		internal class ForAllCallbackHandler
		{
			internal IntPtr cb { get; }
			internal IntPtr data { get; }
			internal bool include_internals { get; }

			internal ForAllCallbackHandler(IntPtr cb, IntPtr data, bool include_internals)
			{
				this.cb = cb;
				this.data = data;
				this.include_internals = include_internals;
			}

			internal void Invoke(Widget w)
			{
				var nativeCb = TryGetDelegate<CallbackNative>(cb);
				var inv = new GtkSharp.CallbackInvoker(nativeCb, data);
				inv.Handler?.Invoke(w);
			}
		}

		static void ForAll_cb(IntPtr inst, bool include_internals, IntPtr cb, IntPtr data)
		{
			try {
				//GtkContainer's unmanaged dispose calls forall, but by that time the managed object is gone
				//so it couldn't do anything useful, and resurrecting it would cause a resurrection cycle.
				//In that case, just chain to the native base in case it can do something.

				var fcb = new ForAllCallbackHandler(cb, data, include_internals);
				if (GLib.Object.TryGetObject(inst) is Container container) {
					container.ForAll(include_internals, fcb.Invoke);
				} else {
					gtksharp_container_base_forall(inst, include_internals, fcb.Invoke, data);
				}
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException(e, false);
			}
		}

		[GLib.DefaultSignalHandler(Type = typeof(Gtk.Container), ConnectionMethod = nameof(OverrideForAll))]
		protected virtual void ForAll(bool include_internals, Callback callback)
		{
			InternalForAll(include_internals, callback);
		}

		private void InternalForAll(bool include_internals, Callback callback)
		{
			if (!(callback.Target is ForAllCallbackHandler)) {
				throw new ApplicationException(
					$"{nameof(ForAll)} can only be called as \"base.{nameof(ForAll)}()\". Use {nameof(Forall)}() or {nameof(Foreach)}().");
			}

			gtksharp_container_base_forall(Handle, include_internals, callback, ((ForAllCallbackHandler) callback.Target).data);
		}

		static ForAllNativeDelegate InternalForAllNativeDelegate(GLib.GType gtype)
		{
			ForAllNativeDelegate unmanaged = null;
			unsafe {
				IntPtr* raw_ptr = (IntPtr*) (((long) gtype.GetClassPtr()) + (long) class_abi.GetFieldOffset("forall"));
				if (*raw_ptr == IntPtr.Zero)
					return default;

				unmanaged = Marshal.GetDelegateForFunctionPointer<ForAllNativeDelegate>(*raw_ptr);
			}

			return unmanaged;
		}


		static void gtksharp_container_base_forall(IntPtr container, bool include_internals, Callback cb, IntPtr data)
		{
			// gtksharp_container_base_forall from gtkglue:
			// Find and call the first base callback that's not the GTK# callback. The GTK# callback calls down the whole
			// managed override chain, so calling it on a subclass-of-a-managed-container-subclass causes a stack overflow.

			// GtkContainerClass *parent = (GtkContainerClass *) G_OBJECT_GET_CLASS (container);
			// while ((parent = g_type_class_peek_parent (parent))) {
			//     if (strncmp (G_OBJECT_CLASS_NAME (parent), "__gtksharp_", 11) != 0) {
			//         if (parent->forall) {
			//             (*parent->forall) (container, include_internals, cb, data);
			//         }
			//         return;
			//     }
			// }
			if (container == IntPtr.Zero)
				return;

			var obj = TryGetObject(container);
			if (obj == default)
				return;
			
			var parent = obj.NativeType;
			while ((parent = parent.GetBaseType()) != GLib.GType.None) {
				if (parent.ToString().StartsWith("__gtksharp_")) {
					continue;
				}

				var forAll = InternalForAllNativeDelegate(parent);
				if (forAll != default && cb.Target is ForAllCallbackHandler cbh) {
					GtkSharp.CallbackWrapper cb_wrapper = new GtkSharp.CallbackWrapper(cb);
					forAll(container, include_internals, cbh.cb, data);
				}

				return;
			}
		}
	}
}
