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
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	public delegate bool GSourceFunc ();

	//
	// Base class for IdleProxy and TimeoutProxy
	//
	internal class SourceProxy : IDisposable {
		internal Delegate real_handler;
		internal Delegate proxy_handler;
		internal uint ID;

		~SourceProxy ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Both branches remove our delegate from the
			// managed list of handlers, but only
			// Source.Remove will remove it from the
			// unmanaged list also.

			if (disposing)
				Remove ();
			else
				Source.Remove (ID);
		}

		internal void Remove ()
		{
			Source.RemoveSourceHandler (ID);
			real_handler = null;
			proxy_handler = null;
		}
	}

	public partial class Source : GLib.Opaque {

		private static IDictionary<uint, SourceProxy> source_handlers = new Dictionary<uint, SourceProxy> ();

		private Source () {}

		public Source(IntPtr raw) : base(raw) {}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_source_new(IntPtr source_funcs, uint struct_size);
		static d_g_source_new g_source_new = FuncLoader.LoadFunction<d_g_source_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_new"));

		public Source (GLib.SourceFuncs source_funcs, uint struct_size)
		{
			IntPtr native_source_funcs = GLib.Marshaller.StructureToPtrAlloc (source_funcs);
			Raw = g_source_new(native_source_funcs, struct_size);
			source_funcs = GLib.SourceFuncs.New (native_source_funcs);
			Marshal.FreeHGlobal (native_source_funcs);
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

		internal static void AddSourceHandler (uint id, SourceProxy proxy)
		{
			lock (Source.source_handlers) {
				source_handlers [id] = proxy;
			}
		}

		internal static void RemoveSourceHandler (uint id)
		{
			lock (Source.source_handlers) {
				source_handlers.Remove (id);
			}
		}

		internal static bool RemoveSourceHandler (Delegate hndlr)
		{
			bool result = false;
			List<uint> keys = new List<uint> ();

			lock (source_handlers) {
				foreach (uint code in source_handlers.Keys) {
					var p = Source.source_handlers [code];

					if (p != null && p.real_handler == hndlr) {
						keys.Add (code);
						result = g_source_remove (code);
					}
				}

				foreach (var key in keys) {
					Source.RemoveSourceHandler (key);
				}
			}

			return result;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_source_remove(uint tag);
		static d_g_source_remove g_source_remove = FuncLoader.LoadFunction<d_g_source_remove>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_remove"));

		public static bool Remove (uint tag)
		{
			// g_source_remove always returns true, so we follow that
			bool ret = true;

			lock (Source.source_handlers) {
				if (source_handlers.Remove (tag)) {
					ret = g_source_remove (tag);
				}
			}
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_source_get_type();
		static d_g_source_get_type g_source_get_type = FuncLoader.LoadFunction<d_g_source_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_type"));

		public static GLib.GType GType {
			get {
				IntPtr raw_ret = g_source_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_source_get_context(IntPtr raw);
		static d_g_source_get_context g_source_get_context = FuncLoader.LoadFunction<d_g_source_get_context>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_context"));

		public GLib.MainContext Context {
			get  {
				IntPtr raw_ret = g_source_get_context(Handle);
				GLib.MainContext ret = raw_ret == IntPtr.Zero ? null : new MainContext (raw_ret);
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_source_get_priority(IntPtr raw);
		static d_g_source_get_priority g_source_get_priority = FuncLoader.LoadFunction<d_g_source_get_priority>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_priority"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_priority(IntPtr raw, int priority);
		static d_g_source_set_priority g_source_set_priority = FuncLoader.LoadFunction<d_g_source_set_priority>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_priority"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_source_get_name(IntPtr raw);
		static d_g_source_get_name g_source_get_name = FuncLoader.LoadFunction<d_g_source_get_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_name"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_name(IntPtr raw, IntPtr name);
		static d_g_source_set_name g_source_set_name = FuncLoader.LoadFunction<d_g_source_set_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_name"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_add_child_source(IntPtr raw, IntPtr child_source);
		static d_g_source_add_child_source g_source_add_child_source = FuncLoader.LoadFunction<d_g_source_add_child_source>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_add_child_source"));

		public void AddChildSource(GLib.Source child_source) {
			g_source_add_child_source(Handle, child_source == null ? IntPtr.Zero : child_source.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_add_poll(IntPtr raw, IntPtr fd);
		static d_g_source_add_poll g_source_add_poll = FuncLoader.LoadFunction<d_g_source_add_poll>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_add_poll"));

		public void AddPoll(GLib.PollFD fd) {
			IntPtr native_fd = GLib.Marshaller.StructureToPtrAlloc (fd);
			g_source_add_poll(Handle, native_fd);
			fd = GLib.PollFD.New (native_fd);
			Marshal.FreeHGlobal (native_fd);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_source_attach(IntPtr raw, IntPtr context);
		static d_g_source_attach g_source_attach = FuncLoader.LoadFunction<d_g_source_attach>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_attach"));

		public uint Attach(GLib.MainContext context) {
			uint raw_ret = g_source_attach(Handle, context == null ? IntPtr.Zero : context.Handle);
			uint ret = raw_ret;
			return ret;
		}

		uint Attach() {
			return Attach (null);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_source_get_can_recurse(IntPtr raw);
		static d_g_source_get_can_recurse g_source_get_can_recurse = FuncLoader.LoadFunction<d_g_source_get_can_recurse>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_can_recurse"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_can_recurse(IntPtr raw, bool can_recurse);
		static d_g_source_set_can_recurse g_source_set_can_recurse = FuncLoader.LoadFunction<d_g_source_set_can_recurse>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_can_recurse"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_get_current_time(IntPtr raw, IntPtr timeval);
		static d_g_source_get_current_time g_source_get_current_time = FuncLoader.LoadFunction<d_g_source_get_current_time>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_current_time"));

		[Obsolete]
		public void GetCurrentTime(GLib.TimeVal timeval) {
			IntPtr native_timeval = GLib.Marshaller.StructureToPtrAlloc (timeval);
			g_source_get_current_time(Handle, native_timeval);
			timeval = GLib.TimeVal.New (native_timeval);
			Marshal.FreeHGlobal (native_timeval);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate uint d_g_source_get_id(IntPtr raw);
		static d_g_source_get_id g_source_get_id = FuncLoader.LoadFunction<d_g_source_get_id>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_id"));

		public uint Id {
			get {
				uint raw_ret = g_source_get_id(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate long d_g_source_get_ready_time(IntPtr raw);
		static d_g_source_get_ready_time g_source_get_ready_time = FuncLoader.LoadFunction<d_g_source_get_ready_time>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_ready_time"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_ready_time(IntPtr raw, long ready_time);
		static d_g_source_set_ready_time g_source_set_ready_time = FuncLoader.LoadFunction<d_g_source_set_ready_time>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_ready_time"));

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
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate long d_g_source_get_time(IntPtr raw);
		static d_g_source_get_time g_source_get_time = FuncLoader.LoadFunction<d_g_source_get_time>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_get_time"));

		public long Time {
			get {
				long raw_ret = g_source_get_time(Handle);
				long ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_source_is_destroyed(IntPtr raw);
		static d_g_source_is_destroyed g_source_is_destroyed = FuncLoader.LoadFunction<d_g_source_is_destroyed>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_is_destroyed"));

		public bool IsDestroyed {
			get {
				bool raw_ret = g_source_is_destroyed(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_modify_unix_fd(IntPtr raw, IntPtr tag, int new_events);
		static d_g_source_modify_unix_fd g_source_modify_unix_fd = FuncLoader.LoadFunction<d_g_source_modify_unix_fd>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_modify_unix_fd"));

		public void ModifyUnixFd(IntPtr tag, GLib.IOCondition new_events) {
			g_source_modify_unix_fd(Handle, tag, (int) new_events);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int d_g_source_query_unix_fd(IntPtr raw, IntPtr tag);
		static d_g_source_query_unix_fd g_source_query_unix_fd = FuncLoader.LoadFunction<d_g_source_query_unix_fd>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_query_unix_fd"));

		public GLib.IOCondition QueryUnixFd(IntPtr tag) {
			int raw_ret = g_source_query_unix_fd(Handle, tag);
			GLib.IOCondition ret = (GLib.IOCondition) raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_remove_child_source(IntPtr raw, IntPtr child_source);
		static d_g_source_remove_child_source g_source_remove_child_source = FuncLoader.LoadFunction<d_g_source_remove_child_source>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_remove_child_source"));

		public void RemoveChildSource(GLib.Source child_source) {
			g_source_remove_child_source(Handle, child_source == null ? IntPtr.Zero : child_source.Handle);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_remove_poll(IntPtr raw, IntPtr fd);
		static d_g_source_remove_poll g_source_remove_poll = FuncLoader.LoadFunction<d_g_source_remove_poll>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_remove_poll"));

		public void RemovePoll(GLib.PollFD fd) {
			IntPtr native_fd = GLib.Marshaller.StructureToPtrAlloc (fd);
			g_source_remove_poll(Handle, native_fd);
			fd = GLib.PollFD.New (native_fd);
			Marshal.FreeHGlobal (native_fd);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_remove_unix_fd(IntPtr raw, IntPtr tag);
		static d_g_source_remove_unix_fd g_source_remove_unix_fd = FuncLoader.LoadFunction<d_g_source_remove_unix_fd>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_remove_unix_fd"));

		public void RemoveUnixFd(IntPtr tag) {
			g_source_remove_unix_fd(Handle, tag);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_callback_indirect(IntPtr raw, IntPtr callback_data, IntPtr callback_funcs);
		static d_g_source_set_callback_indirect g_source_set_callback_indirect = FuncLoader.LoadFunction<d_g_source_set_callback_indirect>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_callback_indirect"));

		public void SetCallbackIndirect(IntPtr callback_data, GLib.SourceCallbackFuncs callback_funcs) {
			IntPtr native_callback_funcs = GLib.Marshaller.StructureToPtrAlloc (callback_funcs);
			g_source_set_callback_indirect(Handle, callback_data, native_callback_funcs);
			callback_funcs = GLib.SourceCallbackFuncs.New (native_callback_funcs);
			Marshal.FreeHGlobal (native_callback_funcs);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_funcs(IntPtr raw, IntPtr value);
		static d_g_source_set_funcs g_source_set_funcs = FuncLoader.LoadFunction<d_g_source_set_funcs>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_funcs"));

		public GLib.SourceFuncs Funcs {
			set {
				IntPtr native_value = GLib.Marshaller.StructureToPtrAlloc (value);
				g_source_set_funcs(Handle, native_value);
				value = GLib.SourceFuncs.New (native_value);
				Marshal.FreeHGlobal (native_value);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_source_remove_by_funcs_user_data(IntPtr funcs, IntPtr user_data);
		static d_g_source_remove_by_funcs_user_data g_source_remove_by_funcs_user_data = FuncLoader.LoadFunction<d_g_source_remove_by_funcs_user_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_remove_by_funcs_user_data"));

		public static bool RemoveByFuncsUserData(GLib.SourceFuncs funcs, IntPtr user_data) {
			IntPtr native_funcs = GLib.Marshaller.StructureToPtrAlloc (funcs);
			bool raw_ret = g_source_remove_by_funcs_user_data(native_funcs, user_data);
			bool ret = raw_ret;
			funcs = GLib.SourceFuncs.New (native_funcs);
			Marshal.FreeHGlobal (native_funcs);
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_g_source_remove_by_user_data(IntPtr user_data);
		static d_g_source_remove_by_user_data g_source_remove_by_user_data = FuncLoader.LoadFunction<d_g_source_remove_by_user_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_remove_by_user_data"));

		public static bool RemoveByUserData(IntPtr user_data) {
			bool raw_ret = g_source_remove_by_user_data(user_data);
			bool ret = raw_ret;
			return ret;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_set_name_by_id(uint tag, IntPtr name);
		static d_g_source_set_name_by_id g_source_set_name_by_id = FuncLoader.LoadFunction<d_g_source_set_name_by_id>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_set_name_by_id"));

		public static void SetNameById(uint tag, string name) {
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			g_source_set_name_by_id(tag, native_name);
			GLib.Marshaller.Free (native_name);
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_g_source_ref(IntPtr raw);
		static d_g_source_ref g_source_ref = FuncLoader.LoadFunction<d_g_source_ref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_ref"));

		protected override void Ref (IntPtr raw)
		{
			if (!Owned) {
				g_source_ref (raw);
				Owned = true;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_unref(IntPtr raw);
		static d_g_source_unref g_source_unref = FuncLoader.LoadFunction<d_g_source_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_unref"));

		protected override void Unref (IntPtr raw)
		{
			if (Owned) {
				g_source_unref (raw);
				Owned = false;
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_g_source_destroy(IntPtr raw);
		static d_g_source_destroy g_source_destroy = FuncLoader.LoadFunction<d_g_source_destroy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GLib), "g_source_destroy"));

		protected override void Free (IntPtr raw)
		{
			g_source_destroy (raw);
		}
	}
}

