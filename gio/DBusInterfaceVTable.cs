// Copyright (c) 2011 Novell, Inc.
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
using System.Collections;
using System.Runtime.InteropServices;

namespace GLib {

	public partial class DBusInterfaceVTable {

		[StructLayout(LayoutKind.Sequential)]
		struct NativeStruct {
			public GLibSharp.DBusInterfaceMethodCallFuncNative method_call;
			public GLibSharp.DBusInterfaceGetPropertyFuncNative get_property;
			public GLibSharp.DBusInterfaceSetPropertyFuncNative set_property;

			[MarshalAs (UnmanagedType.ByValArray, SizeConst=8)]
			private IntPtr[] Padding;
		}

		IntPtr handle;
		NativeStruct native_cbs;

		~DBusInterfaceVTable ()
		{
			Marshaller.Free (handle);
		}

		public DBusInterfaceVTable (DBusInterfaceMethodCallFunc method_call, DBusInterfaceGetPropertyFunc get_property, DBusInterfaceSetPropertyFunc set_property)
		{
			this.method_call = method_call;
			this.get_property = get_property;
			this.set_property = set_property;
			native_cbs.method_call = OnMethodCall;
			native_cbs.get_property = OnGetProperty;
			native_cbs.set_property = OnSetProperty;
			handle = Marshaller.StructureToPtrAlloc (native_cbs);
		}

		public IntPtr OnGetProperty (IntPtr connection, IntPtr sender, IntPtr object_path, IntPtr interface_name, IntPtr property_name, out IntPtr error, IntPtr user_data)
		{
			error = IntPtr.Zero;

			try {
				if (get_property == null)
					return IntPtr.Zero;

				GLib.Variant __ret = get_property (GLib.Object.GetObject(connection) as GLib.DBusConnection, GLib.Marshaller.Utf8PtrToString (sender), GLib.Marshaller.Utf8PtrToString (object_path), GLib.Marshaller.Utf8PtrToString (interface_name), GLib.Marshaller.Utf8PtrToString (property_name));
				return __ret == null ? IntPtr.Zero : __ret.Handle;
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, true);
				// NOTREACHED: Above call does not return.
				throw e;
			}
		}

		public void OnMethodCall (IntPtr connection, IntPtr sender, IntPtr object_path, IntPtr interface_name, IntPtr method_name, IntPtr parameters, IntPtr invocation, IntPtr user_data)
		{
			try {
				if (method_call == null)
					return;

				method_call (GLib.Object.GetObject(connection) as GLib.DBusConnection, GLib.Marshaller.Utf8PtrToString (sender), GLib.Marshaller.Utf8PtrToString (object_path), GLib.Marshaller.Utf8PtrToString (interface_name), GLib.Marshaller.Utf8PtrToString (method_name), new GLib.Variant(parameters), GLib.Object.GetObject(invocation) as GLib.DBusMethodInvocation);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		public bool OnSetProperty (IntPtr connection, IntPtr sender, IntPtr object_path, IntPtr interface_name, IntPtr property_name, IntPtr value, out IntPtr error, IntPtr user_data)
		{
			error = IntPtr.Zero;

			try {
				if (set_property == null)
					return false;

				return set_property (GLib.Object.GetObject(connection) as GLib.DBusConnection, GLib.Marshaller.Utf8PtrToString (sender), GLib.Marshaller.Utf8PtrToString (object_path), GLib.Marshaller.Utf8PtrToString (interface_name), GLib.Marshaller.Utf8PtrToString (property_name), new GLib.Variant(value));
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
				return false;
			}
		}

		DBusInterfaceMethodCallFunc method_call;
		public GLib.DBusInterfaceMethodCallFunc MethodCall {
			get { return method_call; }
			set { method_call = value; }
		}

		GLib.DBusInterfaceGetPropertyFunc get_property;
		public GLib.DBusInterfaceGetPropertyFunc GetProperty {
			get { return get_property; }
			set { get_property = value; }
		}

		GLib.DBusInterfaceSetPropertyFunc set_property;
		public GLib.DBusInterfaceSetPropertyFunc SetProperty {
			get { return set_property; }
			set { set_property = value; }
		}
	}
}
