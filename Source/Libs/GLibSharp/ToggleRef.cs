// GLib.ToggleRef.cs - GLib ToggleRef class implementation
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright <c> 2007, 2011 Novell, Inc.
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


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GLib {

	internal class ToggleRef : IDisposable {

		bool hardened;
		// Set by the weak notify when the native object is finalized behind our back (see WeakNotify below).
		// volatile because the notify may run on the GTK/main thread while Free() runs on the GC finalizer thread.
		volatile bool objectFinalized;
		IntPtr handle;
		object reference;
		GCHandle gch;

		public ToggleRef (GLib.Object target)
		{
			handle = target.Handle;
			gch = GCHandle.Alloc (this);
			reference = target;
			g_object_add_toggle_ref (target.Handle, ToggleNotifyCallback, (IntPtr) gch);
			// A weak ref does NOT change the refcount (purely additive to the toggle ref), but its notify
			// fires if the native object is ever finalized without going through our Free(). That is the
			// "torn down behind its back" case (Widget.Destroy / an unexpected extra unref): the wrapper's
			// handle would otherwise stay non-zero and dangling, so any later operation on it dereferences
			// freed -- and possibly address-reused -- memory (GTK_IS_WINDOW/GTK_IS_WIDGET failed, 0xC0000005).
			// WeakNotify zeros the wrapper's handle so those stale operations hit a null handle instead.
			g_object_weak_ref (target.Handle, WeakNotifyCallback, (IntPtr) gch);
			g_object_unref (target.Handle);
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public GLib.Object Target {
			get {
				if (reference == null)
					return null;
				else if (reference is GLib.Object)
					return reference as GLib.Object;

				WeakReference weak = (WeakReference)reference;
				return weak.Target as GLib.Object;
			}
		}

		public void Dispose ()
		{
			lock (PendingDestroys) {
				PendingDestroys.Remove (this);
			}
			Free ();
		}

  		void Free ()
  		{
			// If the native object was already finalized behind our back, the weak notify has fired and the
			// object is gone -- the weak ref auto-removed itself and touching the (freed) handle would crash.
			// Skip every native call; just drop the managed bookkeeping.
			if (!objectFinalized) {
				g_object_weak_unref (handle, WeakNotifyCallback, (IntPtr) gch);
				if (hardened)
					g_object_unref (handle);
				else
					g_object_remove_toggle_ref (handle, ToggleNotifyCallback, (IntPtr) gch);
			}

			reference = null;

			QueueGCHandleFree ();

			handle = IntPtr.Zero;
		}

		internal void Harden ()
		{
			// Added for the benefit of GnomeProgram.  It releases a final ref in
			// an atexit handler which causes toggle ref notifications to occur after 
			// our delegates are gone, so we need a mechanism to override the 
			// notifications.  This method effectively leaks all objects which invoke it, 
			// but since it is only used by Gnome.Program, which is a singleton object 
			// with program duration persistence, who cares.

			g_object_ref (handle);
			g_object_weak_unref (handle, WeakNotifyCallback, (IntPtr) gch);
			g_object_remove_toggle_ref (handle, ToggleNotifyCallback, (IntPtr) gch);
			if (reference is WeakReference)
				reference = (reference as WeakReference).Target;
			hardened = true;
		}

		void Toggle (bool is_last_ref)
		{
			if (is_last_ref && reference is GLib.Object)
				reference = new WeakReference (reference);
			else if (!is_last_ref && reference is WeakReference) {
				WeakReference weak = reference as WeakReference;
				if (weak.IsAlive)
					reference = weak.Target;
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void ToggleNotifyHandler (IntPtr data, IntPtr handle, bool is_last_ref);

		static void RefToggled (IntPtr data, IntPtr handle, bool is_last_ref)
		{
			try {
				GCHandle gch = (GCHandle) data;
				ToggleRef tref = (ToggleRef)gch.Target;
				tref?.Toggle (is_last_ref);
			} catch (Exception e) {
				ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static ToggleNotifyHandler toggle_notify_callback;
		static ToggleNotifyHandler ToggleNotifyCallback {
			get {
				if (toggle_notify_callback == null)
					toggle_notify_callback = new ToggleNotifyHandler (RefToggled);
				return toggle_notify_callback;
			}
		}

		// GWeakNotify: void (*)(gpointer data, GObject *where_the_object_was)
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void WeakNotifyHandler (IntPtr data, IntPtr where_the_object_was);

		static void WeakNotified (IntPtr data, IntPtr where_the_object_was)
		{
			// The native object has been finalized (freed) without going through our Free(). Zero the
			// wrapper's handle so any lingering reference to it (e.g. a leaked GLib timer still holding the
			// managed wrapper) sees IntPtr.Zero and no longer dereferences the freed -- possibly
			// address-reused -- native pointer. Also flag the toggle ref so its eventual Free() skips the
			// now-invalid native calls.
			try {
				GCHandle gch = (GCHandle) data;
				if (!gch.IsAllocated)
					return;
				ToggleRef tref = gch.Target as ToggleRef;
				if (tref == null)
					return;
				tref.objectFinalized = true;
				GLib.Object target = tref.Target;
				if (target != null)
					target.InvalidateHandle (where_the_object_was);
			} catch (Exception e) {
				ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		static WeakNotifyHandler weak_notify_callback;
		static WeakNotifyHandler WeakNotifyCallback {
			get {
				if (weak_notify_callback == null)
					weak_notify_callback = new WeakNotifyHandler (WeakNotified);
				return weak_notify_callback;
			}
		}

		static List<GCHandle> PendingGCHandleFrees = new List<GCHandle> ();
		static bool gc_idle_queued;

		public void QueueGCHandleFree ()
		{
			lock (PendingGCHandleFrees) {
				PendingGCHandleFrees.Add (gch);
				if (!gc_idle_queued){
					Timeout.Add (50, new TimeoutHandler (PerformGCHandleFrees));
					gc_idle_queued = true;
				}
			}
		}

		static bool PerformGCHandleFrees ()
		{
			GCHandle[] handles;

			lock (PendingGCHandleFrees){
				handles = new GCHandle [PendingGCHandleFrees.Count];
				PendingGCHandleFrees.CopyTo (handles, 0);
				PendingGCHandleFrees.Clear ();
				gc_idle_queued = false;
			}

			foreach (GCHandle r in handles)
				r.Free ();

			return false;
		}

		static List<ToggleRef> PendingDestroys = new List<ToggleRef> ();
		static bool idle_queued;

		public void QueueUnref ()
		{
			lock (PendingDestroys) {
				PendingDestroys.Add (this);
				if (!idle_queued){
					Timeout.Add (50, new TimeoutHandler (PerformQueuedUnrefs));
					idle_queued = true;
				}
			}
		}

		static bool PerformQueuedUnrefs ()
		{
			ToggleRef[] references;

			lock (PendingDestroys){
				references = new ToggleRef [PendingDestroys.Count];
				PendingDestroys.CopyTo (references, 0);
				PendingDestroys.Clear ();
				idle_queued = false;
			}

			foreach (ToggleRef r in references)
				r.Free ();

			return false;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_add_toggle_ref(IntPtr raw, ToggleNotifyHandler notify_cb, IntPtr data);
		static d_g_object_add_toggle_ref g_object_add_toggle_ref = FuncLoader.LoadFunction<d_g_object_add_toggle_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_add_toggle_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_remove_toggle_ref(IntPtr raw, ToggleNotifyHandler notify_cb, IntPtr data);
		static d_g_object_remove_toggle_ref g_object_remove_toggle_ref = FuncLoader.LoadFunction<d_g_object_remove_toggle_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_remove_toggle_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_object_ref(IntPtr raw);
		static d_g_object_ref g_object_ref = FuncLoader.LoadFunction<d_g_object_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_unref(IntPtr raw);
		static d_g_object_unref g_object_unref = FuncLoader.LoadFunction<d_g_object_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_unref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_weak_ref(IntPtr raw, WeakNotifyHandler notify_cb, IntPtr data);
		static d_g_object_weak_ref g_object_weak_ref = FuncLoader.LoadFunction<d_g_object_weak_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_weak_ref"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_object_weak_unref(IntPtr raw, WeakNotifyHandler notify_cb, IntPtr data);
		static d_g_object_weak_unref g_object_weak_unref = FuncLoader.LoadFunction<d_g_object_weak_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_object_weak_unref"));

	}
}

