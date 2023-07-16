// SignalConnector.cs - helper for Gtk.Builder
//
// Authors: Stephane Delcroix  <stephane@delcroix.org>
// The biggest part of this code is adapted from glade#, by
//	Ricardo Fernández Pascual <ric@users.sourceforge.net>
//	Rachel Hestilow <hestilow@ximian.com>
//
// Copyright (c) 2002 Ricardo Fernández Pascual
// Copyright (c) 2003 Rachel Hestilow
// Copyright (c) 2008 Novell, Inc.
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
using System.Runtime.InteropServices;

namespace Gtk
{
	internal class SignalConnector
	{
        readonly Type handler_type;
        readonly object handler;
		internal object template_object_instance;

		public SignalConnector (object handler)
		{
			this.handler = handler;
			handler_type = handler.GetType ();
		}
	
		public SignalConnector (Type handler_type)
		{
			this.handler = null;
			this.handler_type = handler_type;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_builder_connect_signals_full(IntPtr raw, GtkSharp.BuilderConnectFuncNative func, IntPtr user_data);
		static readonly d_gtk_builder_connect_signals_full gtk_builder_connect_signals_full = FuncLoader.LoadFunction<d_gtk_builder_connect_signals_full>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_builder_connect_signals_full"));
	
		public void ConnectSignals(Builder builder) {
			GtkSharp.BuilderConnectFuncWrapper func_wrapper = new GtkSharp.BuilderConnectFuncWrapper (new BuilderConnectFunc (ConnectFunc));
			gtk_builder_connect_signals_full(builder.Handle, func_wrapper.NativeDelegate, IntPtr.Zero);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_widget_class_set_connect_func(IntPtr class_ptr, GtkSharp.BuilderConnectFuncNative connect_func, IntPtr data);
		static readonly d_gtk_widget_class_set_connect_func gtk_widget_class_set_connect_func = FuncLoader.LoadFunction<d_gtk_widget_class_set_connect_func>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), nameof(gtk_widget_class_set_connect_func)));

		public void ConnectSignals(GLib.GType gtype)
		{
			var func_wrapper = new GtkSharp.BuilderConnectFuncWrapper (new BuilderConnectFunc (ConnectFunc));
			gtk_widget_class_set_connect_func(gtype.GetClassPtr (), func_wrapper.NativeDelegate, IntPtr.Zero);
		}
	
		public void ConnectFunc (Builder builder, GLib.Object objekt, string signal_name, string handler_name, GLib.Object connect_object, GLib.ConnectFlags flags)
		{
			/* search for the event to connect */
			System.Reflection.MemberInfo[] evnts = objekt.GetType ().
				FindMembers (System.Reflection.MemberTypes.Event,
						System.Reflection.BindingFlags.Instance
						| System.Reflection.BindingFlags.Static
						| System.Reflection.BindingFlags.Public
						| System.Reflection.BindingFlags.NonPublic,
						new System.Reflection.MemberFilter (SignalFilter), signal_name);
			foreach (System.Reflection.EventInfo ei in evnts) {
				bool connected = false;
				System.Reflection.MethodInfo add = ei.GetAddMethod ();
				System.Reflection.ParameterInfo[] addpi = add.GetParameters ();
				if (addpi.Length == 1) { /* this should be always true, unless there's something broken */
					Type delegate_type = addpi[0].ParameterType;
	
					/* look for an instance method */
					if (connect_object != null || handler != null)
						try {
							Delegate d = Delegate.CreateDelegate (delegate_type, connect_object != null ? connect_object : handler, handler_name);
							add.Invoke (objekt, new object[] { d } );
							connected = true;
						} catch (ArgumentException) { /* ignore if there is not such instance method */
						}

					/* look for an instance method for template */
					if (!connected && template_object_instance != null)
						try {
							Delegate d = Delegate.CreateDelegate (delegate_type, template_object_instance, handler_name);
							add.Invoke (objekt, new object[] { d });
							connected = true;
						} catch (ArgumentException) { /* ignore if there is not such instance method */
						}

					/* look for a static method if no instance method has been found */
					if (!connected && handler_type != null)
						try  {
							Delegate d = Delegate.CreateDelegate (delegate_type, handler_type, handler_name);
							add.Invoke (objekt, new object[] { d } );
							connected = true;
						} catch (ArgumentException) { /* ignore if there is not such static method */
						}
	
					if (!connected) {
						string msg = ExplainError (ei.Name, delegate_type, handler_type, handler_name);
						throw new HandlerNotFoundException (msg, handler_name, signal_name, ei, delegate_type);
					}
				}
			}
		}
	
		static bool SignalFilter (System.Reflection.MemberInfo m, object filterCriteria)
		{
			string signame = (filterCriteria as string);
			object[] attrs = m.GetCustomAttributes (typeof (GLib.SignalAttribute), false);
			if (attrs.Length > 0)
			{
				foreach (GLib.SignalAttribute a in attrs)
				{
					if (signame == a.CName)
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				/* this tries to match the names when no attributes are present.
					It is only a fallback. */
				signame = signame.ToLower ().Replace ("_", "");
				string evname = m.Name.ToLower ();
				return signame == evname;
			}
		}
	
		static string GetSignature (System.Reflection.MethodInfo method)
		{
			if (method == null)
				return null;
	
			System.Reflection.ParameterInfo [] parameters = method.GetParameters ();
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			sb.Append ('(');
			foreach (System.Reflection.ParameterInfo info in parameters) {
				sb.Append (info.ParameterType.ToString ());
				sb.Append (',');
			}
			if (sb.Length != 0)
				sb.Length--;
	
			sb.Append (')');
			return sb.ToString ();
		}
	
		static string GetSignature (Type delegate_type)
		{
			System.Reflection.MethodInfo method = delegate_type.GetMethod ("Invoke");
			return GetSignature (method);
		}
	
		static string GetSignature (Type klass, string method_name)
		{
			const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic |
							System.Reflection.BindingFlags.Public |
							System.Reflection.BindingFlags.Static |
							System.Reflection.BindingFlags.Instance;

			try {
				System.Reflection.MethodInfo method = klass.GetMethod (method_name, flags);
				return GetSignature (method);
			} catch {
				// May be more than one method with that name and none matches
				return null;
			}
		}

		static string ExplainError (string event_name, Type deleg, Type klass, string method)
		{
			if (deleg == null || klass == null || method == null)
				return null;
	
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			string expected = GetSignature (deleg);
			string actual = GetSignature (klass, method);
			if (actual == null)
				return null;

			sb.AppendFormat ("The handler for the event {0} should take '{1}', " +
				"but the signature of the provided handler ('{2}') is '{3}'\n",
				event_name, expected, method, actual);
			
			return sb.ToString ();
		}
	}
}
