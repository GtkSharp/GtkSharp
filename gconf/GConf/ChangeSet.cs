namespace GConf
{
	using System;
	using System.Runtime.InteropServices;
	
	public class ChangeSet : ClientBase, IDisposable
	{
		IntPtr Raw = IntPtr.Zero;
	
		[DllImport("gconf-2")]
		static extern IntPtr gconf_change_set_new ();

		public ChangeSet ()
		{
			Initialize ();
			Raw = gconf_change_set_new ();
		}

		~ChangeSet ()
		{
			Dispose ();
		}

		[DllImport("gconf-2")]
		static extern void gconf_change_set_unref (IntPtr cs);

		public void Dispose ()
		{
			if (Raw != IntPtr.Zero)
			{
				gconf_change_set_unref (Raw);
				Raw = IntPtr.Zero;
			}
		}

		[DllImport("gconf-2")]
		static extern void gconf_change_set_set (IntPtr cs, string key, IntPtr val);
		
		protected override void SetValue (string key, Value val)
		{
			gconf_change_set_set (Raw, key, val.Handle);
		}

		[DllImport("gconf-2")]
		static extern bool gconf_change_set_check_value (IntPtr cs, string key, out IntPtr val);

		public override object Get (string key)
		{
			IntPtr raw_val;
			if (gconf_change_set_check_value (Raw, key, out raw_val) && raw_val != IntPtr.Zero) {
				Value val = new Value (raw_val);
				val.Managed = false;
				return val.Get ();
			} else {
				throw new NoSuchKeyException ();
			}
		}
	}
}

