// GTK.Widget.cs - GTK Widget class implementation
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// (c) 2001 Mike Kestner

namespace Gtk {

        using System;
        using System.Collections;
        using System.Runtime.InteropServices;
	using GLib;
	using Gdk;


        public class Widget : Object {

		private static readonly string DelEvName = "delete-event";

		private Hashtable Signals = new Hashtable ();

		// Properties

		// FIXME: Implement Parent, Style, Event, & ExtensionEvents

                /// <summary>
                ///     AppPaintable Property
                /// </summary>
                ///
                /// <remarks>
                ///     FIXME: What's this?
                /// </remarks>

		public bool AppPaintable {
			get {
				bool val;
				GetProperty ("app-paintable", out val);
				return val;
			}
			set {
				SetProperty ("app-paintable", value);
			}
		}

                /// <summary>
                ///     CanDefault Property
                /// </summary>
                ///
                /// <remarks>
                ///     Indicates if the Widget can be the default for focus.
                /// </remarks>

		public bool CanDefault {
			get {
				bool val;
				GetProperty ("can-default", out val);
				return val;
			}
			set {
				SetProperty ("can-default", value);
			}
		}

                /// <summary>
                ///     CanFocus Property
                /// </summary>
                ///
                /// <remarks>
                ///     Indicates if the Widget can obtain the input focus.
                /// </remarks>

		public bool CanFocus {
			get {
				bool val;
				GetProperty ("can-focus", out val);
				return val;
			}
			set {
				SetProperty ("can-focus", value);
			}
		}

                /// <summary>
                ///     CompositeChild Property
                /// </summary>
                ///
                /// <remarks>
                ///     FIXME: What's this?
                /// </remarks>

		public bool CompositeChild {
			get {
				bool val;
				GetProperty ("composite-child", out val);
				return val;
			}
			set {
				SetProperty ("composite-child", value);
			}
		}

                /// <summary>
                ///     HasDefault Property
                /// </summary>
                ///
                /// <remarks>
                ///     Indicates if the Widget is the default for focus.
                /// </remarks>

		public bool HasDefault {
			get {
				bool val;
				GetProperty ("has-default", out val);
				return val;
			}
			set {
				SetProperty ("has-default", value);
			}
		}

                /// <summary>
                ///     HasFocus Property
                /// </summary>
                ///
                /// <remarks>
                ///     Indicates if the Widget has the input focus.
                /// </remarks>

		public bool HasFocus {
			get {
				bool val;
				GetProperty ("has-focus", out val);
				return val;
			}
			set {
				SetProperty ("has-focus", value);
			}
		}

                /// <summary>
                ///     HeightRequest Property
                /// </summary>
                ///
                /// <remarks>
                ///     The desired height in pixels for the widget.
                /// </remarks>

		public int HeightRequest {
			get {
				int val;
				GetProperty ("height-request", out val);
				return val;
			}
			set {
				SetProperty ("height-request", value);
			}
		}

                /// <summary>
                ///     Name Property
                /// </summary>
                ///
                /// <remarks>
                ///     The name of the widget.
                /// </remarks>

		public String Name {
			get {
				String val;
				GetProperty ("name", out val);
				return val;
			}
			set {
				SetProperty ("name", value);
			}
		}

                /// <summary>
                ///     ReceivesDefault Property
                /// </summary>
                ///
                /// <remarks>
                ///     FIXME: What does this do?
                /// </remarks>

		public bool ReceivesDefault {
			get {
				bool val;
				GetProperty ("receives-default", out val);
				return val;
			}
			set {
				SetProperty ("receives-default", value);
			}
		}

                /// <summary>
                ///     Sensitive Property
                /// </summary>
                ///
                /// <remarks>
                ///     Indicates if the Widget is sensitive to input.
                /// </remarks>

		public bool Sensitive {
			get {
				bool val;
				GetProperty ("sensitive", out val);
				return val;
			}
			set {
				SetProperty ("sensitive", value);
			}
		}

                /// <summary>
                ///     Visible Property
                /// </summary>
                ///
                /// <remarks>
                ///     Indicates if the Widget is visible.
                /// </remarks>

		public bool Visible {
			get {
				bool val;
				GetProperty ("visible", out val);
				return val;
			}
			set {
				SetProperty ("visible", value);
			}
		}

                /// <summary>
                ///     WidthRequest Property
                /// </summary>
                ///
                /// <remarks>
                ///     The desired height in pixels for the widget.
                /// </remarks>

		public int WidthRequest {
			get {
				int val;
				GetProperty ("width-request", out val);
				return val;
			}
			set {
				SetProperty ("width-request", value);
			}
		}

                /// <summary>
                ///     DeleteEvent Event
                /// </summary>
                ///
                /// <remarks>
                ///     Signal emitted when a widget is deleted by the 
		///	windowing environment.
                /// </remarks>

		public event EventHandler DeleteEvent {
			add {
				if (Events [DelEvName] == null)
					Signals [DelEvName] = new SimpleEvent (
							this, RawObject,
							DelEvName, value);

				Events.AddHandler(DelEvName, value);
			}
                        remove {
				Events.RemoveHandler(DelEvName, value);
				if (Events [DelEvName] == null)
					Signals.Remove (DelEvName);
			}
		}

                /// <summary>
                ///     Show Method
                /// </summary>
                ///
                /// <remarks>
                ///     Makes the Widget visible on the display.
                /// </remarks>

                [DllImport("gtk-1.3.dll")]
                static extern void gtk_widget_show (IntPtr obj);

                public void Show ()
                {
                        gtk_widget_show (RawObject);
                }
        }
}
