// NotifyWrapper.cs -
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
	
	internal delegate void NotifyFuncNative (IntPtr client_ptr, uint id, IntPtr entry_ptr, IntPtr user_data);
			
	internal class NotifyWrapper
	{
		static ArrayList refs = new ArrayList ();
		NotifyEventHandler notify;
		NotifyFuncNative native;
			
		void NotifyCB (IntPtr client_ptr, uint id, IntPtr entry_ptr, IntPtr user_data)
		{
			Client client = new Client (client_ptr);
			_Entry entry = new _Entry (entry_ptr);
			if (entry.ValuePtr == IntPtr.Zero) {
				notify (client, new NotifyEventArgs (entry.Key, null));
				return;
			}
			Value val = new Value (entry.ValuePtr);
			val.Managed = false;
			notify (client, new NotifyEventArgs (entry.Key, val.Get ()));
		}

		public NotifyWrapper (NotifyEventHandler notify)
		{
			this.notify = notify;
			this.native = new NotifyFuncNative (this.NotifyCB);
		}

		public static NotifyFuncNative Wrap (NotifyEventHandler notify)
		{
			NotifyWrapper wrapper = new NotifyWrapper (notify);
			refs.Add (wrapper);
			return wrapper.native;
		}
	}
}

