// Client.cs -
//
// Author: Rachel Hestilow  <hestilow@nullenvoid.com>
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


namespace GConf
{
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	
	public class Client : ClientBase
	{
		IntPtr Raw = IntPtr.Zero;
		Hashtable dirs = new Hashtable ();

		[DllImport("gconf-2")]
		static extern IntPtr gconf_client_get_default ();

		public Client ()
		{
			Initialize ();
			Raw = gconf_client_get_default ();
		}

		internal Client (IntPtr raw)
		{
			Initialize ();
			Raw = raw;
		}

		[DllImport("gconf-2")]
		static extern void gconf_client_set (IntPtr client, string key, IntPtr val, out IntPtr err);

		internal override void SetValue (string key, Value val)
		{
			IntPtr err;
			gconf_client_set (Raw, key, val.Handle, out err);
			if (err != IntPtr.Zero)
				throw new GLib.GException (err);
		}

		[DllImport("gconf-2")]
		static extern IntPtr gconf_client_get (IntPtr client, string key, out IntPtr err);

		public override object Get (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			IntPtr err;
			IntPtr raw = gconf_client_get (Raw, key, out err);
			if (err != IntPtr.Zero)
				throw new GLib.GException (err);
			
			if (raw == IntPtr.Zero)
				throw new NoSuchKeyException (key);

			Value val = new Value (raw);
			return val.Get ();
		}

		[DllImport("gconf-2")]
		static extern uint gconf_client_add_dir (IntPtr client, string dir, int preload, out IntPtr err);

		DirectoryConnections GetConnections (string dir)
		{
			if (dirs.Contains (dir))
				return dirs [dir] as DirectoryConnections;

			IntPtr err;
			gconf_client_add_dir (Raw, dir, 0, out err);
			if (err != IntPtr.Zero)
				throw new GLib.GException (err);

			DirectoryConnections result = new DirectoryConnections (this, dir);
			dirs.Add (dir, result);
			return result;
		}
		
		class DirectoryConnections {

			string path;
			IntPtr handle;
			Hashtable connections = new Hashtable ();

			public DirectoryConnections (Client client, string dir)
			{
				handle = client.Raw;
				path = dir;
			}

			[DllImport("gconf-2")]
			static extern uint gconf_client_notify_add (IntPtr client, string dir, NotifyFuncNative func, IntPtr user_data, IntPtr destroy_notify, out IntPtr err);
		
			public void Add (NotifyEventHandler notify)
			{
				IntPtr error;
				uint id = gconf_client_notify_add (handle, path, NotifyWrapper.Wrap (notify), IntPtr.Zero, IntPtr.Zero, out error);
				if (error != IntPtr.Zero)
					throw new GLib.GException (error);

				connections [id] = notify;
			}

			[DllImport("gconf-2")]
			static extern void gconf_client_notify_remove (IntPtr client, uint cnxn);

			public void Remove (NotifyEventHandler notify)
			{
				ArrayList removes = new ArrayList ();
				foreach (uint id in connections.Keys)
					if (connections [id] == notify)
						removes.Add (id);
				foreach (uint id in removes) {
					gconf_client_notify_remove (handle, id);
					connections.Remove (id);
				}
			}
		}

		public void AddNotify (string dir, NotifyEventHandler notify)
		{
			GetConnections (dir).Add (notify);
		}

		public void RemoveNotify (string dir, NotifyEventHandler notify)
		{
			GetConnections (dir).Remove (notify);
		}

		[DllImport("gconf-2")]
		static extern void gconf_client_suggest_sync (IntPtr client, out IntPtr err);

		public void SuggestSync ()
		{
			IntPtr err;
			gconf_client_suggest_sync (Raw, out err);
			if (err != IntPtr.Zero)
				throw new GLib.GException (err);
		}
	}
}

