//  ModuleCallbackStatusMessage.cs - GnomeVfsModuleCallback* bindings.
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
	public class ModuleCallbackStatusMessage : ModuleCallback {
		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackStatusMessageIn {
			public string Uri;
			public string Message;
			public int Percentage;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ModuleCallbackStatusMessageOut {
			private int Dummy;
			private IntPtr _reserved1;
			private IntPtr _reserved2;
		}

		private delegate void ModuleCallbackStatusMessageNative (ref ModuleCallbackStatusMessageIn authIn, int inSize,
									 ref ModuleCallbackStatusMessageOut authOut, int outSize,
									 IntPtr data);

		private delegate void ModuleCallbackAsyncStatusMessageNative (ref ModuleCallbackStatusMessageIn authIn, int inSize,
									      ref ModuleCallbackStatusMessageOut authOut, int outSize,
									      IntPtr data, IntPtr resp, IntPtr resp_data);

		// ModuleCallbackStatusMessageIn fields.
		private string uri;
		private string message;
		private int percentage;

		public string Uri {
			get {
				return uri;
			}
		}
		
		public string Message {
			get {
				return message;
			}
		}
		
		public int Percentage {
			get {
				return percentage;
			}
		}

		public override event ModuleCallbackHandler Callback;

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_push (string callback_name, ModuleCallbackStatusMessageNative callback, IntPtr data, IntPtr destroy);

		public override void Push ()
		{
			gnome_vfs_module_callback_push ("status-message",
							new ModuleCallbackStatusMessageNative (OnNativeCallback),
							IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_pop (string callback_name);

		public override void Pop ()
		{
			gnome_vfs_module_callback_pop ("status-message");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_module_callback_set_default (string callback_name, ModuleCallbackStatusMessageNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefault ()
		{
			gnome_vfs_module_callback_set_default ("status-message",
							       new ModuleCallbackStatusMessageNative (OnNativeCallback),
							       IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_push (string callback_name, ModuleCallbackAsyncStatusMessageNative callback, IntPtr data, IntPtr destroy);

		public override void PushAsync ()
		{
			gnome_vfs_async_module_callback_push ("status-message",
							      new ModuleCallbackAsyncStatusMessageNative (OnNativeAsyncCallback),
							      IntPtr.Zero, IntPtr.Zero);
		}
		
		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_pop (string callback_name);

		public override void PopAsync ()
		{
			gnome_vfs_async_module_callback_pop ("status-message");
		}

		[DllImport ("gnomevfs-2")]
		private static extern void gnome_vfs_async_module_callback_set_default (string callback_name, ModuleCallbackAsyncStatusMessageNative callback, IntPtr data, IntPtr destroy);

		public override void SetDefaultAsync ()
		{
			gnome_vfs_async_module_callback_set_default ("status-message",
								     new ModuleCallbackAsyncStatusMessageNative (OnNativeAsyncCallback),
								     IntPtr.Zero, IntPtr.Zero);
		}
		
		private void OnNativeAsyncCallback (ref ModuleCallbackStatusMessageIn authIn, int inSize,
						    ref ModuleCallbackStatusMessageOut authOut, int outSize,
						    IntPtr data, IntPtr resp, IntPtr resp_data)
		{
			OnNativeCallback (ref authIn, inSize, ref authOut, outSize, data);
		}
		
		private void OnNativeCallback (ref ModuleCallbackStatusMessageIn authIn, int inSize,
					       ref ModuleCallbackStatusMessageOut authOut, int outSize, IntPtr data)
		{
			// Copy the authIn fields.
			uri = authIn.Uri;
			message = authIn.Message;
			percentage = authIn.Percentage;
			
			// Activate the callback.
			ModuleCallbackHandler handler = Callback;
			if (handler != null) {
				handler (this);
			}
		}
	}
}
