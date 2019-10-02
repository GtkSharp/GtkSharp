// Gtk.Clipboard.cs - Customizations for the Clipboard class
//
// Authors:  Mike Kestner  <mkestner@novell.com>
//
// Copyright (c) 2005  Novell, Inc.
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
	using System.Runtime.InteropServices;

	public partial class Clipboard {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_clipboard_set_with_data(IntPtr raw, TargetEntry[] targets, int n_targets, GtkSharp.ClipboardGetFuncNative get_func, GtkSharp.ClipboardClearFuncNative clear_func, IntPtr data);
		static d_gtk_clipboard_set_with_data gtk_clipboard_set_with_data = FuncLoader.LoadFunction<d_gtk_clipboard_set_with_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_clipboard_set_with_data"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_clipboard_set_with_owner(IntPtr raw, TargetEntry[] targets, int n_targets, GtkSharp.ClipboardGetFuncNative get_func, GtkSharp.ClipboardClearFuncNative clear_func, IntPtr owner);
		static d_gtk_clipboard_set_with_owner gtk_clipboard_set_with_owner = FuncLoader.LoadFunction<d_gtk_clipboard_set_with_owner>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_clipboard_set_with_owner"));

		void ClearProxy (Clipboard clipboard)
		{
			if (Data ["clear_func"] != null)  {
				ClipboardClearFunc clear = Data ["clear_func"] as ClipboardClearFunc;
				clear (clipboard);
			}
			SetPersistentData (null, null, null);
		}

		void SetPersistentData (object get_func_wrapper, object clear_func, object clear_proxy_wrapper)
		{
			Data ["get_func_wrapper"] = get_func_wrapper;
			Data ["clear_func"] = clear_func;
			Data ["clear_proxy_wrapper"] = clear_proxy_wrapper;
		}

		public bool SetWithData (TargetEntry[] targets, ClipboardGetFunc get_func, ClipboardClearFunc clear_func)
		{
			ClipboardClearFunc clear_proxy = new ClipboardClearFunc (ClearProxy);
			GtkSharp.ClipboardGetFuncWrapper get_func_wrapper = new GtkSharp.ClipboardGetFuncWrapper (get_func);
			GtkSharp.ClipboardClearFuncWrapper clear_proxy_wrapper = new GtkSharp.ClipboardClearFuncWrapper (clear_proxy);
			bool ret = gtk_clipboard_set_with_data (Handle, targets, targets.Length, get_func_wrapper.NativeDelegate, clear_proxy_wrapper.NativeDelegate, IntPtr.Zero);
			SetPersistentData (get_func_wrapper, clear_func, clear_proxy_wrapper);
			return ret;
		}

		public bool SetWithOwner (TargetEntry[] targets, ClipboardGetFunc get_func, ClipboardClearFunc clear_func, GLib.Object owner)
		{
			ClipboardClearFunc clear_proxy = new ClipboardClearFunc (ClearProxy);
			GtkSharp.ClipboardGetFuncWrapper get_func_wrapper = new GtkSharp.ClipboardGetFuncWrapper (get_func);
			GtkSharp.ClipboardClearFuncWrapper clear_proxy_wrapper = new GtkSharp.ClipboardClearFuncWrapper (clear_proxy);
			bool ret = gtk_clipboard_set_with_owner (Handle, targets, targets.Length, get_func_wrapper.NativeDelegate, clear_proxy_wrapper.NativeDelegate, owner == null ? IntPtr.Zero : owner.Handle);
			SetPersistentData (get_func_wrapper, clear_func, clear_proxy_wrapper);
			return ret;
		}

		[Obsolete ("Replaced by Text property.")]
		public void SetText (string text)
		{
			Text = text;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_clipboard_wait_for_rich_text(IntPtr raw, IntPtr buffer, out IntPtr format, out UIntPtr length);
		static d_gtk_clipboard_wait_for_rich_text gtk_clipboard_wait_for_rich_text = FuncLoader.LoadFunction<d_gtk_clipboard_wait_for_rich_text>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_clipboard_wait_for_rich_text"));

		public byte[] WaitForRichText(Gtk.TextBuffer buffer, out Gdk.Atom format) 
		{
			UIntPtr length;
			IntPtr format_as_native;
			IntPtr raw_ret = gtk_clipboard_wait_for_rich_text (Handle, buffer == null ? IntPtr.Zero : buffer.Handle, out format_as_native, out length);
			format = format_as_native == IntPtr.Zero ? null : (Gdk.Atom) GLib.Opaque.GetOpaque (format_as_native, typeof (Gdk.Atom), false);
			if (raw_ret == IntPtr.Zero)
				return new byte [0];
			int sz = (int) (uint) length;
			byte[] ret = new byte [sz];
			Marshal.Copy (ret, 0, raw_ret, sz);
			return ret;
		}

		public delegate void RichTextReceivedFunc (Gtk.Clipboard clipboard, Gdk.Atom format, byte[] text);

		static RichTextReceivedFuncNative rt_rcvd_marshaler;

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void RichTextReceivedFuncNative (IntPtr clipboard, IntPtr format, IntPtr text, UIntPtr length, IntPtr data);


		void RichTextReceivedCallback (IntPtr clipboard_ptr, IntPtr format_ptr, IntPtr text_ptr, UIntPtr length, IntPtr data)
		{
			try {
				Gtk.Clipboard clipboard = GLib.Object.GetObject(clipboard_ptr) as Gtk.Clipboard;
				Gdk.Atom format = format_ptr == IntPtr.Zero ? null : (Gdk.Atom) GLib.Opaque.GetOpaque (format_ptr, typeof (Gdk.Atom), false);
				int sz = (int) (uint) length;
				byte[] text = new byte [sz];
				Marshal.Copy (text, 0, text_ptr, sz);
				GCHandle gch = (GCHandle) data;
				RichTextReceivedFunc cb = gch.Target as RichTextReceivedFunc;
				cb (clipboard, format, text);
				gch.Free ();
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_clipboard_request_rich_text(IntPtr raw, IntPtr buffer, RichTextReceivedFuncNative cb, IntPtr user_data);
		static d_gtk_clipboard_request_rich_text gtk_clipboard_request_rich_text = FuncLoader.LoadFunction<d_gtk_clipboard_request_rich_text>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_clipboard_request_rich_text"));

		public void RequestRichText (Gtk.TextBuffer buffer, RichTextReceivedFunc cb) 
		{
			if (rt_rcvd_marshaler == null)
				rt_rcvd_marshaler = new RichTextReceivedFuncNative (RichTextReceivedCallback);
			gtk_clipboard_request_rich_text (Handle, buffer == null ? IntPtr.Zero : buffer.Handle, rt_rcvd_marshaler, (IntPtr) GCHandle.Alloc (cb));
		}
	}
}

