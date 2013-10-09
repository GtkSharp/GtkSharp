// GLib.Source.cs - Source class implementation
//
// Author: Duncan Mak  <duncan@ximian.com>
//
// Copyright (c) 2002 Mike Kestner
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


namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public delegate bool GSourceFunc ();

	//
	// Base class for IdleProxy and TimeoutProxy
	//
	internal class SourceProxy {
		internal Delegate real_handler;
		internal Delegate proxy_handler;
		internal uint ID;

		internal void Remove ()
		{
			lock (Source.source_handlers)
				Source.source_handlers.Remove (ID);
			real_handler = null;
			proxy_handler = null;
		}
	}

	public partial class Source : GLib.Opaque {

		private Source () {}

		internal static Hashtable source_handlers = new Hashtable ();
		
		[DllImport ("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_source_remove (uint tag);

		public static bool Remove (uint tag)
		{
			lock (Source.source_handlers)
				source_handlers.Remove (tag);
			return g_source_remove (tag);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_source_get_context(IntPtr raw);

		public GLib.MainContext Context {
			get  {
				IntPtr raw_ret = g_source_get_context(Handle);
				GLib.MainContext ret = raw_ret == IntPtr.Zero ? null : new MainContext (raw_ret);
				return ret;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern int g_source_get_priority(IntPtr raw);

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_priority(IntPtr raw, int priority);

		public int Priority {
			get  {
				int raw_ret = g_source_get_priority(Handle);
				int ret = raw_ret;
				return ret;
			}
			set  {
				g_source_set_priority(Handle, value);
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_source_get_name(IntPtr raw);

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_name(IntPtr raw, IntPtr name);

		public string Name {
			get  {
				IntPtr raw_ret = g_source_get_name(Handle);
				string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
				return ret;
			}
			set  {
				IntPtr native_value = GLib.Marshaller.StringToPtrGStrdup (value);
				g_source_set_name(Handle, native_value);
				GLib.Marshaller.Free (native_value);
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_source_get_type();

		public static GLib.GType GType {
			get {
				IntPtr raw_ret = g_source_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_add_child_source(IntPtr raw, IntPtr child_source);

		public void AddChildSource(GLib.Source child_source) {
			g_source_add_child_source(Handle, child_source == null ? IntPtr.Zero : child_source.Handle);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_add_poll(IntPtr raw, IntPtr fd);

		public void AddPoll(GLib.PollFD fd) {
			IntPtr native_fd = GLib.Marshaller.StructureToPtrAlloc (fd);
			g_source_add_poll(Handle, native_fd);
			fd = GLib.PollFD.New (native_fd);
			Marshal.FreeHGlobal (native_fd);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_source_attach(IntPtr raw, IntPtr context);

		public uint Attach(GLib.MainContext context) {
			uint raw_ret = g_source_attach(Handle, context == null ? IntPtr.Zero : context.Handle);
			uint ret = raw_ret;
			return ret;
		}

		uint Attach() {
			return Attach (null);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_source_get_can_recurse(IntPtr raw);

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_can_recurse(IntPtr raw, bool can_recurse);

		public bool CanRecurse {
			get {
				bool raw_ret = g_source_get_can_recurse(Handle);
				bool ret = raw_ret;
				return ret;
			}
			set {
				g_source_set_can_recurse(Handle, value);
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_get_current_time(IntPtr raw, IntPtr timeval);

		[Obsolete]
		public void GetCurrentTime(GLib.TimeVal timeval) {
			IntPtr native_timeval = GLib.Marshaller.StructureToPtrAlloc (timeval);
			g_source_get_current_time(Handle, native_timeval);
			timeval = GLib.TimeVal.New (native_timeval);
			Marshal.FreeHGlobal (native_timeval);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_source_get_id(IntPtr raw);

		public uint Id {
			get {
				uint raw_ret = g_source_get_id(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern long g_source_get_ready_time(IntPtr raw);

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_ready_time(IntPtr raw, long ready_time);

		public long ReadyTime {
			get {
				long raw_ret = g_source_get_ready_time(Handle);
				long ret = raw_ret;
				return ret;
			}
			set {
				g_source_set_ready_time(Handle, value);
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern long g_source_get_time(IntPtr raw);

		public long Time {
			get {
				long raw_ret = g_source_get_time(Handle);
				long ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_source_is_destroyed(IntPtr raw);

		public bool IsDestroyed {
			get {
				bool raw_ret = g_source_is_destroyed(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_modify_unix_fd(IntPtr raw, IntPtr tag, int new_events);

		public void ModifyUnixFd(IntPtr tag, GLib.IOCondition new_events) {
			g_source_modify_unix_fd(Handle, tag, (int) new_events);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern int g_source_query_unix_fd(IntPtr raw, IntPtr tag);

		public GLib.IOCondition QueryUnixFd(IntPtr tag) {
			int raw_ret = g_source_query_unix_fd(Handle, tag);
			GLib.IOCondition ret = (GLib.IOCondition) raw_ret;
			return ret;
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_remove_child_source(IntPtr raw, IntPtr child_source);

		public void RemoveChildSource(GLib.Source child_source) {
			g_source_remove_child_source(Handle, child_source == null ? IntPtr.Zero : child_source.Handle);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_remove_poll(IntPtr raw, IntPtr fd);

		public void RemovePoll(GLib.PollFD fd) {
			IntPtr native_fd = GLib.Marshaller.StructureToPtrAlloc (fd);
			g_source_remove_poll(Handle, native_fd);
			fd = GLib.PollFD.New (native_fd);
			Marshal.FreeHGlobal (native_fd);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_remove_unix_fd(IntPtr raw, IntPtr tag);

		public void RemoveUnixFd(IntPtr tag) {
			g_source_remove_unix_fd(Handle, tag);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_callback_indirect(IntPtr raw, IntPtr callback_data, IntPtr callback_funcs);

		public void SetCallbackIndirect(IntPtr callback_data, GLib.SourceCallbackFuncs callback_funcs) {
			IntPtr native_callback_funcs = GLib.Marshaller.StructureToPtrAlloc (callback_funcs);
			g_source_set_callback_indirect(Handle, callback_data, native_callback_funcs);
			callback_funcs = GLib.SourceCallbackFuncs.New (native_callback_funcs);
			Marshal.FreeHGlobal (native_callback_funcs);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_funcs(IntPtr raw, IntPtr value);

		public GLib.SourceFuncs Funcs {
			set {
				IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
				g_source_set_funcs(Handle, native_value);
				value = GLib.SourceFuncs.New (native_value);
				Marshal.FreeHGlobal (native_value);
			}
		}

		/*
		 * commented out because there is already a custom implementation for Remove
		 *
		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_source_remove(uint tag);

		public static bool Remove(uint tag) {
			bool raw_ret = g_source_remove(tag);
			bool ret = raw_ret;
			return ret;
		}
		*/

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_source_remove_by_funcs_user_data(IntPtr funcs, IntPtr user_data);

		public static bool RemoveByFuncsUserData(GLib.SourceFuncs funcs, IntPtr user_data) {
			IntPtr native_funcs = GLib.Marshaller.StructureToPtrAlloc (funcs);
			bool raw_ret = g_source_remove_by_funcs_user_data(native_funcs, user_data);
			bool ret = raw_ret;
			funcs = GLib.SourceFuncs.New (native_funcs);
			Marshal.FreeHGlobal (native_funcs);
			return ret;
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern bool g_source_remove_by_user_data(IntPtr user_data);

		public static bool RemoveByUserData(IntPtr user_data) {
			bool raw_ret = g_source_remove_by_user_data(user_data);
			bool ret = raw_ret;
			return ret;
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_set_name_by_id(uint tag, IntPtr name);

		public static void SetNameById(uint tag, string name) {
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_source_set_name_by_id(tag, native_name);
			GLib.Marshaller.Free (native_name);
		}

		public Source(IntPtr raw) : base(raw) {}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_source_new(IntPtr source_funcs, uint struct_size);

		public Source (GLib.SourceFuncs source_funcs, uint struct_size)
		{
			IntPtr native_source_funcs = GLib.Marshaller.StructureToPtrAlloc (source_funcs);
			Raw = g_source_new(native_source_funcs, struct_size);
			source_funcs = GLib.SourceFuncs.New (native_source_funcs);
			Marshal.FreeHGlobal (native_source_funcs);
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr g_source_ref(IntPtr raw);

		protected override void Ref (IntPtr raw)
		{
			if (!Owned) {
				g_source_ref (raw);
				Owned = true;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_unref(IntPtr raw);

		protected override void Unref (IntPtr raw)
		{
			if (Owned) {
				g_source_unref (raw);
				Owned = false;
			}
		}

		[DllImport("libglib-2.0-0.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void g_source_destroy(IntPtr raw);

		protected override void Free (IntPtr raw)
		{
			g_source_destroy (raw);
		}

		class FinalizerInfo {
			IntPtr handle;

			public FinalizerInfo (IntPtr handle)
			{
				this.handle = handle;
			}

			public bool Handler ()
			{
				g_source_destroy (handle);
				return false;
			}
		}

		~Source ()
		{
			if (!Owned)
				return;
			FinalizerInfo info = new FinalizerInfo (Handle);
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (info.Handler));
		}
	}

}