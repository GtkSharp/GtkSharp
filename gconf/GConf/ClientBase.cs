namespace GConf
{
	using System;
	using System.Runtime.InteropServices;
	
	public abstract class ClientBase
	{
		public abstract object Get (string key);
		internal abstract void SetValue (string key, Value val);

		public void Set (string key, object val)
		{
			SetValue (key, new Value (val));
		}

		[DllImport("gconf-2")]
		static extern bool gconf_is_initialized ();
	
		[DllImport("gconf-2")]
		static extern bool gconf_init (int argc, IntPtr argv, out IntPtr err);
	
		internal void Initialize ()
		{
			if (!gconf_is_initialized ())
			{
				IntPtr err;
				gconf_init (0, IntPtr.Zero, out err);
				if (err != IntPtr.Zero)
					throw new GLib.GException (err);
			}
		}
	}
}

