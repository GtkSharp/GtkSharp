//  ModuleCallbackAuthentication.cs - GnomeVfsModuleCallback* bindings.
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
	public class ModuleCallbackAuthentication : ModuleCallback {
		public enum AuthenticationType {
			Basic,
			Digest
		};
	
		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackAuthenticationIn {
			public string Uri;
			public string Realm;
			public bool PreviousAttemptFailed;
			public AuthenticationType AuthType;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackAuthenticationOut {
			public string Username;
			public string Password;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		private delegate void ModuleCallbackAuthenticationNative (ref ModuleCallbackAuthenticationIn authIn, int inSize,
									  ref ModuleCallbackAuthenticationOut authOut, int outSize,
									  IntPtr data);

		private delegate void ModuleCallbackAsyncAuthenticationNative (ref ModuleCallbackAuthenticationIn authIn, int inSize,
									       ref ModuleCallbackAuthenticationOut authOut, int outSize,
									       IntPtr data, IntPtr resp, IntPtr resp_data);

		private string uri;
		private string realm;
		private bool previousAttemptFailed;
		private AuthenticationType authType;
		private string username;
		private string password;

		public string Uri {
			get {
				return uri;
			}
		}
		
		public string Realm {
			get {
				return realm;
			}
		}
		
		public bool PreviousAttemptFailed {
			get {
				return previousAttemptFailed;
			}
		}
		
		public AuthenticationType AuthType {
			get {
				return authType;
			}
		}
		
		public string Username {
			set {
				username = value;
			}
		}
		
		public string Password {
			set {
				password = value;
			}
		}

		public override event ModuleCallbackHandler Callback;
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_push (string callback_name, ModuleCallbackAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void Push ()
		{
			gnome_vfs_module_callback_push ("simple-authentication",
							new ModuleCallbackAuthenticationNative (OnNativeCallback),
							IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_pop (string callback_name);

		public override void Pop ()
		{
			gnome_vfs_module_callback_pop ("simple-authentication");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_set_default (string callback_name, ModuleCallbackAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefault ()
		{
			gnome_vfs_module_callback_set_default ("simple-authentication",
							       new ModuleCallbackAuthenticationNative (OnNativeCallback),
							       IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_push (string callback_name, ModuleCallbackAsyncAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void PushAsync ()
		{
			gnome_vfs_async_module_callback_push ("simple-authentication",
							      new ModuleCallbackAsyncAuthenticationNative (OnNativeAsyncCallback),
							      IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_pop (string callback_name);

		public override void PopAsync ()
		{
			gnome_vfs_async_module_callback_pop ("simple-authentication");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_set_default (string callback_name, ModuleCallbackAsyncAuthenticationNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefaultAsync ()
		{
			gnome_vfs_async_module_callback_set_default ("simple-authentication",
								     new ModuleCallbackAsyncAuthenticationNative (OnNativeAsyncCallback),
								     IntPtr.Zero, IntPtr.Zero);
		}
		
		private void OnNativeAsyncCallback (ref ModuleCallbackAuthenticationIn authIn, int inSize,
						    ref ModuleCallbackAuthenticationOut authOut, int outSize,
						    IntPtr data, IntPtr resp, IntPtr resp_data)
		{
			OnNativeCallback (ref authIn, inSize, ref authOut, outSize, data);
		}
		
		private void OnNativeCallback (ref ModuleCallbackAuthenticationIn authIn, int inSize,
					       ref ModuleCallbackAuthenticationOut authOut, int outSize, IntPtr data)
		{
			// Copy the authIn fields.
			uri = authIn.Uri;
			realm = authIn.Realm;
			previousAttemptFailed = authIn.PreviousAttemptFailed;
			authType = authIn.AuthType;
			
			// Activate the callback.
			ModuleCallbackHandler handler = Callback;
			if (handler != null) {
				handler (this);
				// Copy the values back to the authOut.
				authOut.Username = username;
				authOut.Password = password;
			}
		}
	}
}
