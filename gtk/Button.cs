// GTK.Button.cs - GTK Button class implementation
//
// Authors: Bob Smith <bob@thestuff.net>
//	    Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Bob Smith and Mike Kestner

namespace Gtk {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using GLib;

	/// <summary>
	///	Button Class
	/// </summary>
	/// 
	/// <remarks>
	///	A Button user interface element.
	/// </remarks>

	public class Button : Widget {

		private Hashtable Signals = new Hashtable ();

		/// <summary>
		///	Button Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Button with no label.
		/// </remarks>

		[DllImport("gtk-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr gtk_button_new ();

		public Button ()
		{
			RawObject = gtk_button_new ();
		}

		/// <summary>
		///	Button Object Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Button Wrapper from a raw object. This 
		///	constructor is primarily used internally by gtk-sharp,
		///	but could conceivably be called by an application if
		///	the need to wrap a raw button object presents itself.
		/// </remarks>
		///
		/// <param name="obj">
		///	Raw object reference from the native library.
		/// </param>

		public Button (IntPtr obj)
		{
			RawObject = obj;
		}

		/// <summary>
		///	Button Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Button with the specified label.
		///	Note, that underlines in the label provided will
		///	be converted into mnemonics and the label will be
		///	interpreted as a stock system identifier if possible.
		///	If this behavior is not desired, more control can be 
		///	obtained with an overloaded constructor.
		/// </remarks>
		///
		/// <param name="label">
		///	Text label or stock system id to display on the button.
		/// </param>

		[DllImport("gtk-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr gtk_button_new_from_stock (IntPtr str);

		public Button (String label)
		{
			RawObject = gtk_button_new_from_stock (
					Marshal.StringToHGlobalAnsi (label));
		}

		/// <summary>
		///	Button Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Button with the specified label.
		///	Underlines in the label can be converted to mnemonics
		///	based on the specified flag.  The label can identify  
		///	a stock button if desired as well.
		/// </remarks>
		///
		/// <param name="label">
		///	Text label to display on the button face.
		/// </param>
		///
		/// <param name="stock">
		///	Indicates if the stock system should be used.  If the
		///	label does not represent a known stock button, it will
		///	instead be used verbatim with mnemonic if an underline
		///	is included.
		/// </param>
		///
		/// <param name="mnemonic">
		///	Convert underscore to a mnemonic which can be used
		///	to activate the button with the keyboard.
		/// </param>

		[DllImport("gtk-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr gtk_button_new_with_mnemonic (IntPtr str);

		[DllImport("gtk-1.3.dll", CharSet=CharSet.Ansi,
			   CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr gtk_button_new_with_label (IntPtr str);

		public Button (String label, bool stock, bool mnemonic)
		{
			if (stock) 
				RawObject = gtk_button_new_from_stock (
					Marshal.StringToHGlobalAnsi (label));
			else if (mnemonic)
				RawObject = gtk_button_new_with_mnemonic (
					Marshal.StringToHGlobalAnsi (label));
			else
				RawObject = gtk_button_new_with_label (
					Marshal.StringToHGlobalAnsi (label));
		}

		/// <summary>
		///	Label Property
		/// </summary>
		/// 
		/// <remarks>
		///	Text label to display on the button face.
		/// </remarks>

		public String Label {
			get {
				String val;
				GetProperty ("label", out val);
				return val;
			}
			set {
				SetProperty ("label", value);
			}
		}

		/// <summary>
		///	Relief Property
		/// </summary>
		/// 
		/// <remarks>
		///	Relief style used to draw the button. 
		/// </remarks>

		public ReliefStyle Relief {
			get {
				int val;
				GetProperty ("relief", out val);
				return (ReliefStyle) val;
			}
			set {
				SetProperty ("relief", (int) value);
			}
		}

		/// <summary>
		///	UseStock Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if stock button images should be used.
		/// </remarks>

		public bool UseStock {
			get {
				bool val;
				GetProperty ("use-stock", out val);
				return val;
			}
			set {
				SetProperty ("use-stock", value);
			}
		}

		/// <summary>
		///	UseUnderline Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates if underlines in the label text should be 
		///	treated as a mnemonic.
		/// </remarks>

		public bool UseUnderline {
			get {
				bool val;
				GetProperty ("use-underline", out val);
				return val;
			}
			set {
				SetProperty ("use-underline", value);
			}
		}

		/// <summary>
		///	Activated Event
		/// </summary>
		/// 
		/// <remarks>
		///	Signal indicating that the button has been activated.
		/// </remarks>

		private static readonly string ActName = "activate";

		public event EventHandler Activated
		{
			add {
				if (Events [ActName] == null)
					Signals [ActName] = new SimpleSignal (
							this, RawObject, 
							ActName, value);

				Events.AddHandler (ActName, value);
			}
                        remove {
				Events.RemoveHandler (ActName, value);
				if (Events [ActName] == null)
					Signals.Remove (ActName);
			}
		}

		/// <summary>
		///	Clicked Event
		/// </summary>
		/// 
		/// <remarks>
		///	Signal indicating that the button has been clicked.
		/// </remarks>

		private static readonly string ClkName = "clicked";

		public event EventHandler Clicked
		{
			add {
				if (Events [ClkName] == null)
					Signals [ClkName] = new SimpleSignal (
							this, RawObject, 
							ClkName, value);

				Events.AddHandler (ClkName, value);
			}
                        remove {
				Events.RemoveHandler (ClkName, value);
				if (Events [ClkName] == null)
					Signals.Remove (ClkName);
			}
		}

		/// <summary>
		///	Entered Event
		/// </summary>
		/// 
		/// <remarks>
		///	Signal indicating that the focus has entered the button.
		/// </remarks>

		private static readonly string EnterName = "enter";

		public event EventHandler Entered
		{
			add {
				if (Events [EnterName] == null)
					Signals [EnterName] = new SimpleSignal (
							this, RawObject, 
							EnterName, value);

				Events.AddHandler (EnterName, value);
			}
                        remove {
				Events.RemoveHandler (EnterName, value);
				if (Events [EnterName] == null)
					Signals.Remove (EnterName);
			}
		}

		/// <summary>
		///	Left Event
		/// </summary>
		/// 
		/// <remarks>
		///	Signal indicating that the focus has left the button.
		/// </remarks>

		private static readonly string LeaveName = "leave";

		public event EventHandler Left
		{
			add {
				if (Events [LeaveName] == null)
					Signals [LeaveName] = new SimpleSignal (
							this, RawObject, 
							LeaveName, value);

				Events.AddHandler (LeaveName, value);
			}
                        remove {
				Events.RemoveHandler (LeaveName, value);
				if (Events [LeaveName] == null)
					Signals.Remove (LeaveName);
			}
		}

		/// <summary>
		///	Pressed Event
		/// </summary>
		/// 
		/// <remarks>
		///	Signal indicating that the button has been pressed.
		/// </remarks>

		private static readonly string PressName = "pressed";

		public event EventHandler Pressed
		{
			add {
				if (Events [PressName] == null)
					Signals [PressName] = new SimpleSignal (
							this, RawObject, 
							PressName, value);

				Events.AddHandler (PressName, value);
			}
                        remove {
				Events.RemoveHandler (PressName, value);
				if (Events [PressName] == null)
					Signals.Remove (PressName);
			}
		}

		/// <summary>
		///	Released Event
		/// </summary>
		/// 
		/// <remarks>
		///	Signal indicating that the button has been released.
		/// </remarks>

		private static readonly string RelName = "released";

		public event EventHandler Released
		{
			add {
				if (Events [RelName] == null)
					Signals [RelName] = new SimpleSignal (
							this, RawObject, 
							RelName, value);

				Events.AddHandler (RelName, value);
			}
                        remove {
				Events.RemoveHandler (RelName, value);
				if (Events [RelName] == null)
					Signals.Remove (RelName);
			}
		}

		/// <summary>
		///	Click Method
		/// </summary>
		/// 
		/// <remarks>
		///	Emit a Signal indicating that the button has been 
		///	clicked.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern void gtk_button_clicked (IntPtr obj);

		public void Click ()
		{
			gtk_button_clicked (RawObject);
		}

		/// <summary>
		///	Enter Method
		/// </summary>
		/// 
		/// <remarks>
		///	Emit a Signal indicating that the focus has entered 
		///	the button.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern void gtk_button_enter (IntPtr obj);

		public void Enter ()
		{
			gtk_button_enter (RawObject);
		}

		/// <summary>
		///	Leave Method
		/// </summary>
		/// 
		/// <remarks>
		///	Emit a Signal indicating that the focus has left the
		///	button.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern void gtk_button_leave (IntPtr obj);

		public void Leave ()
		{
			gtk_button_leave (RawObject);
		}

		/// <summary>
		///	Pressed Method
		/// </summary>
		/// 
		/// <remarks>
		///	Emit a Signal indicating that the button has been 
		///	pressed.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern void gtk_button_pressed (IntPtr obj);

		public void Press ()
		{
			gtk_button_pressed (RawObject);
		}

		/// <summary>
		///	Release Method
		/// </summary>
		/// 
		/// <remarks>
		///	Emit a Signal indicating that the button has been 
		///	released.
		/// </remarks>

		[DllImport("gtk-1.3.dll")]
		static extern void gtk_button_released (IntPtr obj);

		public void Release ()
		{
			gtk_button_released (RawObject);
		}

	}
}
