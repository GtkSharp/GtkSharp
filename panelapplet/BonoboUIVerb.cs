using System;
using System.Runtime.InteropServices;

namespace PanelApplet
{

	public delegate void ContextMenuItemCallback ();

	[StructLayout (LayoutKind.Sequential)]
	public struct BonoboUIVerb
	{
		string verb;
		ContextMenuItemCallback callback;
		IntPtr user_data;
		IntPtr dummy;

		public BonoboUIVerb (string name, ContextMenuItemCallback cb)
		{
			verb = name;
			callback = cb;
			user_data = IntPtr.Zero;
			dummy = IntPtr.Zero;
		}
	}
}
