namespace Gdk {
	using System;
	using System.Runtime.InteropServices;

	public class Event
	{
		public Event(IntPtr e)
		{
			_event = e;
		}
		protected IntPtr _event;
		public EventType Type
		{
			get
			{
				IntPtr ptr = Marshal.ReadIntPtr (_event);
				return (EventType)((int)ptr);
			}
			set
			{
				Marshal.WriteIntPtr(_event, new IntPtr((int)value));
			}
		}

/* FIXME: Fix or kill later.
		public EventAny Any
		{
			get
			{
				return (EventAll)this;
			}
		}
		public static explicit operator EventAll (Event e)
		{
			return Marshal.PtrToStructure(e._event, EventAll);
		}
*/
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct EventAny
	{
		public IntPtr type;
		public IntPtr window;
		public SByte send_event;
	}
}
