//  ModuleCallbackFullAuthentication.cs - GnomeVfsModuleCallback* bindings.
//
//  Authors:  Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
//  Copyright (c) 2004 Jeroen Zwartepoorte
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

namespace Gnome.Vfs {
	public class ModuleCallbackFullAuthentication : ModuleCallback
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackFullAuthenticationIn {
			public ModuleCallbackFullAuthenticationFlags Flags;
			public string Uri;
			public string Protocol;
			public string Server;
			public string Object;
			public int Port;
			public string Authtype;
			public string Username;
			public string Domain;
			public string DefaultUser;
			public string DefaultDomain;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackFullAuthenticationOut {
			public bool AbortAuth;
			public string Username;
			public string Domain;
			public string Password;
			public bool SavePassword;
			public string Keyring;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		private delegate void ModuleCallbackFullAuthenticationNative (ref ModuleCallbackFullAuthenticationIn authIn, int inSize,
									      ref ModuleCallbackFullAuthenticationOut authOut, int outSize,
									      IntPtr data);

		private delegate void ModuleCallbackAsyncFullAuthenticationNative (ref ModuleCallbackFullAuthenticationIn authIn, int inSize,
										   ref ModuleCallbackFullAuthenticationOut authOut, int outSize,
										   IntPtr data, IntPtr resp, IntPtr resp_data);

		// ModuleCallbackFullAuthenticationIn fields.
		private ModuleCallbackFullAuthenticationFlags flags;
		private string uri;
		private string protocol;
		private string server;
		private string obj;
		private int port;
		private string authtype;
		private string username;
		private string domain;
		private string defaultUser;
		private string defaultDomain;
		// ModuleCallbackFullAuthenticationOut fields.
		private bool abortAuth;
		private string password;
		private bool savePassword;
		private string keyring;

		public ModuleCallbackFullAuthenticationFlags Flags {
			get {
				return flags;
			}
		}
		
		public string Uri {
			get {
				return uri;
			}
		}
		
		public string Protocol {
			get {
				return protocol;
			}
		}
		
		public string Server {
			get {
				return server;
			}
		}
		
		public string Object {
			get {
				return obj;
			}
		}
		
		public int Port {
			get {
				return port;
			}
		}
		
		public string AuthType {
			get {
				return authtype;
			}
		}
		
		public string Username {
			get {
				return username;
			}
			set {
				username = value;
			}
		}
		
		public string Domain {
			get {
				return domain;
			}
			set {
				domain = value;
			}
		}
		
		public string DefaultUser {
			get {
				return defaultUser;
			}
		}
		
		public string DefaultDomain {
			get {
				return defaultDomain;
			}
		}
		
		public bool AbortAuth {
			set {
				abortAuth = value;
			}
		}
		
		public string Password {
			set {
				password = value;
			}
		}
		
		public bool SavePassword {
			set {
				savePassword = value;
			}
		}
		
		public string Keyring {
			set {
				keyring = value;
			}
		}
		
		public override event ModuleCallbackHandler Callback;

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_push (string callback_name, ModuleCallbackFullAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void Push ()
		{
			gnome_vfs_module_callback_push ("full-authentication",
							new ModuleCallbackFullAuthenticationNative (OnNativeCallback),
							IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_pop (string callback_name);

		public override void Pop ()
		{
			gnome_vfs_module_callback_pop ("full-authentication");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_set_default (string callback_name, ModuleCallbackFullAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefault ()
		{
			gnome_vfs_module_callback_set_default ("full-authentication",
							       new ModuleCallbackFullAuthenticationNative (OnNativeCallback),
							       IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_push (string callback_name, ModuleCallbackAsyncFullAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void PushAsync ()
		{
			gnome_vfs_async_module_callback_push ("full-authentication",
							      new ModuleCallbackAsyncFullAuthenticationNative (OnNativeAsyncCallback),
							      IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_pop (string callback_name);

		public override void PopAsync ()
		{
			gnome_vfs_async_module_callback_pop ("full-authentication");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_set_default (string callback_name, ModuleCallbackAsyncFullAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefaultAsync ()
		{
			gnome_vfs_async_module_callback_set_default ("full-authentication",
								     new ModuleCallbackAsyncFullAuthenticationNative (OnNativeAsyncCallback),
								     IntPtr.Zero, IntPtr.Zero);
		}
		
		private void OnNativeAsyncCallback (ref ModuleCallbackFullAuthenticationIn authIn, int inSize,
						    ref ModuleCallbackFullAuthenticationOut authOut, int outSize,
						    IntPtr data, IntPtr resp, IntPtr resp_data)
		{
			OnNativeCallback (ref authIn, inSize, ref authOut, outSize, data);
		}
		
		private void OnNativeCallback (ref ModuleCallbackFullAuthenticationIn authIn, int inSize,
					       ref ModuleCallbackFullAuthenticationOut authOut, int outSize, IntPtr data)
		{
			// Copy the authIn fields.
			flags = authIn.Flags;
			uri = authIn.Uri;
			protocol = authIn.Protocol;
			server = authIn.Server;
			obj = authIn.Object;
			port = authIn.Port;
			authtype = authIn.Authtype;
			username = authIn.Username;
			domain = authIn.Domain;
			defaultUser = authIn.DefaultUser;
			defaultDomain = authIn.DefaultDomain;
			
			// Activate the callback.
			ModuleCallbackHandler handler = Callback;
			if (handler != null) {
				handler (this);
				// Copy the values back to the authOut.
				authOut.AbortAuth = abortAuth;
				authOut.Username = username;
				authOut.Domain = domain;
				authOut.Password = password;
				authOut.SavePassword = savePassword;
				authOut.Keyring = keyring;
			}
		}
		
		private void DumpAuthIn (ref ModuleCallbackFullAuthenticationIn authIn)
		{
			Console.WriteLine ("Flags:         {0}", authIn.Flags);
			Console.WriteLine ("Uri:           {0}", authIn.Uri);
			Console.WriteLine ("Protocol:      {0}", authIn.Protocol);
			Console.WriteLine ("Server:        {0}", authIn.Server);
			Console.WriteLine ("Object:        {0}", authIn.Object);
			Console.WriteLine ("Port:          {0}", authIn.Port);
			Console.WriteLine ("Authtype:      {0}", authIn.Authtype);
			Console.WriteLine ("Username:      {0}", authIn.Username);
			Console.WriteLine ("Domain:        {0}", authIn.Domain);
			Console.WriteLine ("DefaultUser:   {0}", authIn.DefaultUser);
			Console.WriteLine ("DefaultDomain: {0}", authIn.DefaultDomain);
		}
	}
}
