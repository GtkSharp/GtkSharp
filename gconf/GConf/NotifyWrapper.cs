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

