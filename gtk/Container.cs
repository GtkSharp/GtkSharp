// Container.cs - customizations to Gtk.Container
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Gtk {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public partial class Container : IEnumerable {

		[DllImport("gtksharpglue-3")]
		static extern void gtksharp_container_child_get_property (IntPtr container, IntPtr child, IntPtr property, ref GLib.Value value);

		public GLib.Value ChildGetProperty (Gtk.Widget child, string property_name) {
			GLib.Value value = new GLib.Value ();

			IntPtr native = GLib.Marshaller.StringToPtrGStrdup (property_name);
			gtksharp_container_child_get_property (Handle, child.Handle, native, ref value);
			GLib.Marshaller.Free (native);
			return value;
		}

		public IEnumerator GetEnumerator ()
		{
			return Children.GetEnumerator ();
		}

		class ChildAccumulator {
			public ArrayList Children = new ArrayList ();

			public void Add (Gtk.Widget widget)
			{
				Children.Add (widget);
			}
		}

		public IEnumerable AllChildren {
			get {
				ChildAccumulator acc = new ChildAccumulator ();
				Forall (new Gtk.Callback (acc.Add));
				return acc.Children;
			}
		}

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool gtk_container_get_focus_chain (IntPtr raw, out IntPtr list_ptr);

		[DllImport ("libgtk-win32-3.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_container_set_focus_chain (IntPtr raw, IntPtr list_ptr);

		public Widget[] FocusChain {
			get {
				IntPtr list_ptr;
				bool success = gtk_container_get_focus_chain (Handle, out list_ptr);
				if (!success)
					return new Widget [0];

				GLib.List list = new GLib.List (list_ptr);
				Widget[] result = new Widget [list.Count];
				for (int i = 0; i < list.Count; i++)
					result [i] = list [i] as Widget;
				return result;
			}
			set {
				GLib.List list = new GLib.List (IntPtr.Zero);
				foreach (Widget val in value)
					list.Append (val.Handle);
				gtk_container_set_focus_chain (Handle, list.Handle);
			}

		}

		[DllImport("gtksharpglue-3")]
		static extern void gtksharp_container_base_forall (IntPtr handle, bool include_internals, IntPtr cb, IntPtr data);

		[DllImport("gtksharpglue-3")]
		static extern void gtksharp_container_override_forall (IntPtr gtype, ForallDelegate cb);

		[DllImport("gtksharpglue-3")]
		static extern void gtksharp_container_invoke_gtk_callback (IntPtr cb, IntPtr handle, IntPtr data);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void ForallDelegate (IntPtr container, bool include_internals, IntPtr cb, IntPtr data);

		static ForallDelegate ForallOldCallback;
		static ForallDelegate ForallCallback;

		public struct CallbackInvoker {
			IntPtr cb;
			IntPtr data;

			internal CallbackInvoker (IntPtr cb, IntPtr data)
			{
				this.cb = cb;
				this.data = data;
			}

			internal IntPtr Data {
				get {
					return data;
				}
			}

			internal IntPtr Callback {
				get {
					return cb;
				}
			}

			public void Invoke (Widget w)
			{
				gtksharp_container_invoke_gtk_callback (cb, w.Handle, data);
			}
		}

		static void ForallOld_cb (IntPtr container, bool include_internals, IntPtr cb, IntPtr data)
		{
			try {
				//GtkContainer's unmanaged dispose calls forall, but by that time the managed object is gone
				//so it couldn't do anything useful, and resurrecting it would cause a resurrection cycle.
				//In that case, just chain to the native base in case it can do something.
				Container obj = (Container) GLib.Object.TryGetObject (container);
				if (obj != null) {
					CallbackInvoker invoker = new CallbackInvoker (cb, data);
					obj.ForAll (include_internals, invoker);
				} else {
					gtksharp_container_base_forall (container, include_internals, cb, data);
				}
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static void OverrideForallOld (GLib.GType gtype)
		{
			if (ForallOldCallback == null)
				ForallOldCallback = new ForallDelegate (ForallOld_cb);
			gtksharp_container_override_forall (gtype.Val, ForallOldCallback);
		}

		[Obsolete ("Override the ForAll(bool,Gtk.Callback) method instead")]
		[GLib.DefaultSignalHandler (Type=typeof(Gtk.Container), ConnectionMethod="OverrideForallOld")]
		protected virtual void ForAll (bool include_internals, CallbackInvoker invoker)
		{
			gtksharp_container_base_forall (Handle, include_internals, invoker.Callback, invoker.Data);
		}

		static void Forall_cb (IntPtr container, bool include_internals, IntPtr cb, IntPtr data)
		{
			try {
				//GtkContainer's unmanaged dispose calls forall, but by that time the managed object is gone
				//so it couldn't do anything useful, and resurrecting it would cause a resurrection cycle.
				//In that case, just chain to the native base in case it can do something.
				Container obj = (Container) GLib.Object.TryGetObject (container);
				if (obj != null) {
					CallbackInvoker invoker = new CallbackInvoker (cb, data);
					obj.ForAll (include_internals, new Gtk.Callback (invoker.Invoke));
				} else {
					gtksharp_container_base_forall (container, include_internals, cb, data);
				}
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static void OverrideForall (GLib.GType gtype)
		{
			if (ForallCallback == null)
				ForallCallback = new ForallDelegate (Forall_cb);
			gtksharp_container_override_forall (gtype.Val, ForallCallback);
		}

		[GLib.DefaultSignalHandler (Type=typeof(Gtk.Container), ConnectionMethod="OverrideForall")]
		protected virtual void ForAll (bool include_internals, Gtk.Callback callback)
		{
			CallbackInvoker invoker;
			try {
				invoker = (CallbackInvoker)callback.Target;
			} catch {
				throw new ApplicationException ("ForAll can only be called as \"base.ForAll()\". Use Forall() or Foreach().");
			}
			gtksharp_container_base_forall (Handle, include_internals, invoker.Callback, invoker.Data);
		}

		// Compatibility code for old ChildType() virtual method
		static IntPtr ObsoleteChildType_cb (IntPtr raw)
		{
			try {
				Container obj = GLib.Object.GetObject (raw, false) as Container;
				GLib.GType gtype = obj.ChildType ();
				return gtype.Val;
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}

			return GLib.GType.Invalid.Val;
		}

		static ChildTypeNativeDelegate ObsoleteChildTypeVMCallback;

		static void OverrideObsoleteChildType (GLib.GType gtype)
		{
			if (ObsoleteChildTypeVMCallback == null)
				ObsoleteChildTypeVMCallback = new ChildTypeNativeDelegate (ObsoleteChildType_cb);
			OverrideChildType (gtype, ObsoleteChildTypeVMCallback); // -> autogenerated method
		}

		[Obsolete ("Replaced by OnChildType for implementations and SupportedChildType for callers.")]
		[GLib.DefaultSignalHandler (Type=typeof(Gtk.Container), ConnectionMethod="OverrideObsoleteChildType")]
		public virtual GLib.GType ChildType() {
			return InternalChildType (); // -> autogenerated method
		}

		public class ContainerChild {
			protected Container parent;
			protected Widget child;

			public ContainerChild (Container parent, Widget child)
			{
				this.parent = parent;
				this.child = child;
			}

			public Container Parent {
				get {
					return parent;
				}
			}

			public Widget Child {
				get {
					return child;
				}
			}
		}

		public virtual ContainerChild this [Widget w] {
			get {
				return new ContainerChild (this, w);
			}
		}
	}
}
