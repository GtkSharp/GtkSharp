//  ModuleCallbackFillAuthentication.cs - GnomeVfsModuleCallback* bindings.
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
	public class ModuleCallbackFillAuthentication : ModuleCallback {
		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackFillAuthenticationIn {
			public string Uri;
			public string Protocol;
			public string Server;
			public string Object;
			public int Port;
			public string Authtype;
			public string Username;
			public string Domain;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackFillAuthenticationOut {
			public bool Valid;
			public string Username;
			public string Domain;
			public string Password;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		private delegate void ModuleCallbackFillAuthenticationNative (ref ModuleCallbackFillAuthenticationIn authIn, int inSize,
									      ref ModuleCallbackFillAuthenticationOut authOut, int outSize,
									      IntPtr data);

		private delegate void ModuleCallbackAsyncFillAuthenticationNative (ref ModuleCallbackFillAuthenticationIn authIn, int inSize,
										   ref ModuleCallbackFillAuthenticationOut authOut, int outSize,
										   IntPtr data, IntPtr resp, IntPtr resp_data);

		// ModuleCallbackFillAuthenticationIn fields.
		private string uri;
		private string protocol;
		private string server;
		private string obj;
		private int port;
		private string authtype;
		private string username;
		private string domain;
		// ModuleCallbackFillAuthenticationOut fields.
		private bool valid;
		private string password;

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
		
		public bool Valid { 
			set {
				valid = value;
			}
		}
		
		public string Password {
			set {
				password = value;
			}
		}
		
		public override event ModuleCallbackHandler Callback;

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_push (string callback_name, ModuleCallbackFillAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void Push ()
		{
			gnome_vfs_module_callback_push ("fill-authentication",
							new ModuleCallbackFillAuthenticationNative (OnNativeCallback),
							IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_pop (string callback_name);

		public override void Pop ()
		{
			gnome_vfs_module_callback_pop ("fill-authentication");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_set_default (string callback_name, ModuleCallbackFillAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefault ()
		{
			gnome_vfs_module_callback_set_default ("fill-authentication",
							       new ModuleCallbackFillAuthenticationNative (OnNativeCallback),
							       IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_push (string callback_name, ModuleCallbackAsyncFillAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void PushAsync ()
		{
			gnome_vfs_async_module_callback_push ("fill-authentication",
							      new ModuleCallbackAsyncFillAuthenticationNative (OnNativeAsyncCallback),
							      IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_pop (string callback_name);

		public override void PopAsync ()
		{
			gnome_vfs_async_module_callback_pop ("fill-authentication");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_set_default (string callback_name, ModuleCallbackAsyncFillAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefaultAsync ()
		{
			gnome_vfs_async_module_callback_set_default ("fill-authentication",
								     new ModuleCallbackAsyncFillAuthenticationNative (OnNativeAsyncCallback),
								     IntPtr.Zero, IntPtr.Zero);
		}
		
		private void OnNativeAsyncCallback (ref ModuleCallbackFillAuthenticationIn authIn, int inSize,
						    ref ModuleCallbackFillAuthenticationOut authOut, int outSize,
						    IntPtr data, IntPtr resp, IntPtr resp_data)
		{
			OnNativeCallback (ref authIn, inSize, ref authOut, outSize, data);
		}
		
		private void OnNativeCallback (ref ModuleCallbackFillAuthenticationIn authIn, int inSize,
					       ref ModuleCallbackFillAuthenticationOut authOut, int outSize, IntPtr data)
		{
			// Copy the authIn fields.
			uri = authIn.Uri;
			protocol = authIn.Protocol;
			server = authIn.Server;
			obj = authIn.Object;
			port = authIn.Port;
			authtype = authIn.Authtype;
			username = authIn.Username;
			domain = authIn.Domain;
			
			// Activate the callback.
			ModuleCallbackHandler handler = Callback;
			if (handler != null) {
				handler (this);
				// Copy the values back to the authOut.
				authOut.Valid = valid;
				authOut.Username = username;
				authOut.Domain = domain;
				authOut.Password = password;
			}
		}
	}
}
