namespace GConf
{
	using System;
	using System.Runtime.InteropServices;
	
	internal struct _NativeEntry
	{
		public string key;
		public IntPtr value;
	}
	
	internal class _Entry
	{
		_NativeEntry native;
		
		public _Entry (IntPtr raw)
		{
			native = (_NativeEntry) Marshal.PtrToStructure (raw, typeof (_NativeEntry));
		}

		public string Key
		{
			get { return native.key; }
		}

		internal IntPtr ValuePtr
		{
			get { return native.value; }
		}
	}
}
