namespace Gdk {
	using System;
	using System.Runtime.InteropServices;

	public enum EventType
	{
		Nothing			= -1,
		Delete			= 0,
		Destroy			= 1,
		Expose			= 2,
		MotionNotify		= 3,
		ButtonPress		= 4,
		TwoButtonPress		= 5,
		ThreeButtonPress	= 6,
		ButtonRelease		= 7,
		KeyPress		= 8,
		KeyRelease		= 9,
		EnterNotify		= 10,
		LeaveNotify		= 11,
		FocusChange		= 12,
		Configure		= 13,
		Map			= 14,
		Unmap			= 15,
		PropertyNotify		= 16,
		SelectionClear		= 17,
		SelectionRequest	= 18,
		SelectionNotify		= 19,
		ProximityIn		= 20,
		ProximityOut		= 21,
		DragEnter		= 22,
		DragLeave		= 23,
		DragMotion		= 24,
		DragStatus		= 25,
		DropStart		= 26,
		DropFinished		= 27,
		ClientEvent		= 28,
		VisibilityNotify	= 29,
		NoExpose		= 30,
		Scroll			= 31,
		WindowState		= 32,
		Setting			= 33
	}

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
