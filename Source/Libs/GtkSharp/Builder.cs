// Builder.cs - customizations to Gtk.Builder
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

namespace Gtk {

	using System;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;

	public partial class Builder {

		[System.Serializable]
		public class HandlerNotFoundException : SystemException
		{
			string handler_name;
			string signal_name;
			System.Reflection.EventInfo evnt;
			Type delegate_type;
		
			public HandlerNotFoundException (string handler_name, string signal_name,
							 System.Reflection.EventInfo evnt, Type delegate_type)
				: this (handler_name, signal_name, evnt, delegate_type, null)
			{
			}
		
			public HandlerNotFoundException (string handler_name, string signal_name,
							 System.Reflection.EventInfo evnt, Type delegate_type, Exception inner)
				: base ("No handler " + handler_name + " found for signal " + signal_name,
					inner)
			{
				this.handler_name = handler_name;
				this.signal_name = signal_name;
				this.evnt = evnt;
				this.delegate_type = delegate_type;
			}
		
			public HandlerNotFoundException (string message, string handler_name, string signal_name,
							 System.Reflection.EventInfo evnt, Type delegate_type)
				: base ((message != null) ? message : "No handler " + handler_name + " found for signal " + signal_name,
					null)
			{
				this.handler_name = handler_name;
				this.signal_name = signal_name;
				this.evnt = evnt;
				this.delegate_type = delegate_type;
			}
		
			protected HandlerNotFoundException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base (info, context)
			{
				handler_name = info.GetString ("HandlerName");
				signal_name = info.GetString ("SignalName");
				evnt = info.GetValue ("Event", typeof (System.Reflection.EventInfo)) as System.Reflection.EventInfo;
				delegate_type = info.GetValue ("DelegateType", typeof (Type)) as Type;
			}
		
			public string HandlerName
			{
				get {
					return handler_name;
				}
			}
		
			public string SignalName
			{
				get {
					return signal_name;
				}
			}
		
			public System.Reflection.EventInfo Event
			{
				get {
					return evnt;
				}
			}
		
			public Type DelegateType
			{
				get {
					return delegate_type;
				}
			}
		
			public override void GetObjectData (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			{
				base.GetObjectData (info, context);
				info.AddValue ("HandlerName", handler_name);
				info.AddValue ("SignalName", signal_name);
				info.AddValue ("Event", evnt);
				info.AddValue ("DelegateType", delegate_type);
			}
		}
		
		
		[AttributeUsage (AttributeTargets.Field)]
		public class ObjectAttribute : Attribute
		{
			private string name;
			private bool specified;
		
			public ObjectAttribute (string name)
			{
				specified = true;
				this.name = name;
			}
		
			public ObjectAttribute ()
			{
				specified = false;
			}
		
			public string Name
			{
				get { return name; }
			}
		
			public bool Specified
			{
				get { return specified; }
			}
		}
		
		public IntPtr GetRawObject(string name) {
			IntPtr native_name = GLib.Marshaller.StringToPtrGStrdup (name);
			IntPtr raw_ret = gtk_builder_get_object(Handle, native_name);
			GLib.Marshaller.Free (native_name);
			return raw_ret;
		}
		
		public Builder (System.IO.Stream s) : this (s, null)
		{
		}
		
		public Builder (System.IO.Stream s, string translation_domain)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
		
			AddFromStream (s);
			TranslationDomain = translation_domain;
		}
		
		public Builder (string resource_name) : this (Assembly.GetCallingAssembly (), resource_name, null)
		{
		}
		
		public Builder (string resource_name, string translation_domain)
			: this (Assembly.GetCallingAssembly (), resource_name, translation_domain)
		{
		}
		
		public Builder (Assembly assembly, string resource_name, string translation_domain) : this ()
		{
			if (GetType() != typeof (Builder))
				throw new InvalidOperationException ("Cannot chain to this constructor from subclasses.");
		
			if (assembly == null)
				assembly = Assembly.GetCallingAssembly ();
		
			System.IO.Stream s = assembly.GetManifestResourceStream (resource_name);
			if (s == null)
				throw new ArgumentException ("Cannot get resource file '" + resource_name + "'",
				                             "resource_name");
		
			AddFromStream (s);
			TranslationDomain = translation_domain;
		}
		
		public void Autoconnect (object handler)
		{
			BindFields (handler);
			(new SignalConnector (this, handler)).ConnectSignals ();
		}
		
		public void Autoconnect (Type handler_class)
		{
			BindFields (handler_class);
			(new SignalConnector (this, handler_class)).ConnectSignals ();
		}
		
		class SignalConnector
		{
			Builder builder;
			Type handler_type;
			object handler;
		
			public SignalConnector (Builder builder, object handler)
			{
				this.builder = builder;
				this.handler = handler;
				handler_type = handler.GetType ();
			}
		
			public SignalConnector (Builder builder, Type handler_type)
			{
				this.builder = builder;
				this.handler = null;
				this.handler_type = handler_type;		
			}
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_builder_connect_signals_full(IntPtr raw, GtkSharp.BuilderConnectFuncNative func, IntPtr user_data);
		static d_gtk_builder_connect_signals_full gtk_builder_connect_signals_full = FuncLoader.LoadFunction<d_gtk_builder_connect_signals_full>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_builder_connect_signals_full"));
		
			public void ConnectSignals() {
				GtkSharp.BuilderConnectFuncWrapper func_wrapper = new GtkSharp.BuilderConnectFuncWrapper (new BuilderConnectFunc (ConnectFunc));
				gtk_builder_connect_signals_full(builder.Handle, func_wrapper.NativeDelegate, IntPtr.Zero);
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
					/* this tries to match the names when no attibutes are present.
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
		
			const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic |
							System.Reflection.BindingFlags.Public |
							System.Reflection.BindingFlags.Static |
							System.Reflection.BindingFlags.Instance;
			static string GetSignature (Type klass, string method_name)
			{
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
		
		void AddFromStream (Stream stream)
		{
			var size = (int)stream.Length;
			var buffer = new byte[size];
			stream.Read (buffer, 0, size);
			stream.Close ();

			// If buffer contains a BOM, omit it while reading, otherwise AddFromString(text) crashes
			var offset = 0;
			if (size >= 3 && buffer [0] == 0xEF && buffer [1] == 0xBB && buffer [2] == 0xBF) {
				offset = 3;
			}

			var text = Encoding.UTF8.GetString (buffer, offset, size - offset);
			AddFromString (text);
		}
		
		void BindFields (object target)
		{
			BindFields (target, target.GetType ());
		}
		
		void BindFields (Type type)
		{
			BindFields (null, type);
		}
		
		void BindFields (object target, Type type)
		{
			System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;
			if (target != null)
				flags |= System.Reflection.BindingFlags.Instance;
			else
				flags |= System.Reflection.BindingFlags.Static;
		
			do {
				System.Reflection.FieldInfo[] fields = type.GetFields (flags);
				if (fields == null)
					return;
		
				foreach (System.Reflection.FieldInfo field in fields)
				{
					object[] attrs = field.GetCustomAttributes (typeof (ObjectAttribute), false);
					if (attrs == null || attrs.Length == 0)
						continue;
					// The widget to field binding must be 1:1, so only check
					// the first attribute.
					ObjectAttribute attr = (ObjectAttribute) attrs[0];
					GLib.Object gobject;
					if (attr.Specified)
						gobject = GetObject (attr.Name);
					else
						gobject = GetObject (field.Name);
		
					if (gobject != null)
						try {
							field.SetValue (target, gobject, flags, null, null);
						} catch (Exception e) {
							Console.WriteLine ("Unable to set value for field " + field.Name);
							throw e;
						}
				}
				type = type.BaseType;
			}
			while (type != typeof(object) && type != null);
		}
	}
}

