// Gtk.Widget - Gtk 3 compatibility surface over Gtk 4
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;

	/// <summary>
	/// The parts of the Gtk 3 GtkWidget API that Gtk 4 removed, re-provided on top of what
	/// replaced them.
	/// </summary>
	/// <remarks>
	/// <para>The input events are the substance here. Gtk 3 delivered input as signals on the
	/// widget - button-press-event, motion-notify-event and so on - gated by an event mask, and
	/// only to widgets that owned a GdkWindow. Gtk 4 deleted all of that: there are no per-widget
	/// GdkWindows, no event masks, and input arrives through GtkEventControllers that you attach
	/// to a widget explicitly.</para>
	/// <para>Each event below therefore owns a controller, created on first subscription and kept
	/// for the widget's lifetime. Nothing is attached to a widget nobody listens to, which matters
	/// because every wrapper in the process inherits these members.</para>
	/// <para>Two differences are real and cannot be papered over:</para>
	/// <list type="bullet">
	/// <item><description>Delivery ORDER. Gtk 3 walked up the GdkWindow stack; Gtk 4 runs
	/// controllers in a capture phase (root to target) and then a bubble phase (target to root).
	/// These attach in the bubble phase, which corresponds to Gtk 3's order, but a widget that
	/// relied on interleaving with Gtk's own internal handlers may see a different sequence.</description></item>
	/// <item><description>Screen coordinates. Gtk 4 does not tell a client where the pointer is
	/// on the screen, so <c>XRoot</c>/<c>YRoot</c> repeat the widget-local values rather than
	/// inventing a number.</description></item>
	/// </list>
	/// </remarks>
	public partial class Widget {

		// ------------------------------------------------------------ controllers

		GestureClick _compatClickGesture;
		EventControllerMotion _compatMotionController;
		EventControllerKey _compatKeyController;
		EventControllerScroll _compatScrollController;
		EventControllerFocus _compatFocusController;

		GestureClick CompatClickGesture {
			get {
				if (_compatClickGesture == null) {
					// Button 0: every button, which is what a Gtk 3 button-press-event handler
					// saw. GtkGestureClick defaults to 1 (primary only) and would silently drop
					// right-click handling that used to work.
					_compatClickGesture = new GestureClick { Button = 0 };
					_compatClickGesture.PropagationPhase = PropagationPhase.Bubble;
					_compatClickGesture.Pressed += OnCompatPressed;
					_compatClickGesture.Released += OnCompatReleased;
					AddController(_compatClickGesture);
				}

				return _compatClickGesture;
			}
		}

		EventControllerMotion CompatMotionController {
			get {
				if (_compatMotionController == null) {
					_compatMotionController = new EventControllerMotion();
					_compatMotionController.PropagationPhase = PropagationPhase.Bubble;
					_compatMotionController.Motion += OnCompatMotion;
					_compatMotionController.Enter += OnCompatEnter;
					_compatMotionController.Leave += OnCompatLeave;
					AddController(_compatMotionController);
				}

				return _compatMotionController;
			}
		}

		EventControllerKey CompatKeyController {
			get {
				if (_compatKeyController == null) {
					_compatKeyController = new EventControllerKey();
					_compatKeyController.PropagationPhase = PropagationPhase.Bubble;
					_compatKeyController.KeyPressed += OnCompatKeyPressed;
					_compatKeyController.KeyReleased += OnCompatKeyReleased;
					AddController(_compatKeyController);
				}

				return _compatKeyController;
			}
		}

		EventControllerScroll CompatScrollController {
			get {
				if (_compatScrollController == null) {
					// BothAxes|Kinetic is the closest equivalent of Gtk 3's
					// ScrollMask|SmoothScrollMask, which is what any scroll handler asked for.
					_compatScrollController = new EventControllerScroll(
						EventControllerScrollFlags.BothAxes | EventControllerScrollFlags.Kinetic);
					_compatScrollController.PropagationPhase = PropagationPhase.Bubble;
					_compatScrollController.Scroll += OnCompatScroll;
					AddController(_compatScrollController);
				}

				return _compatScrollController;
			}
		}

		EventControllerFocus CompatFocusController {
			get {
				if (_compatFocusController == null) {
					_compatFocusController = new EventControllerFocus();
					_compatFocusController.PropagationPhase = PropagationPhase.Bubble;
					_compatFocusController.Enter += OnCompatFocusEnter;
					_compatFocusController.Leave += OnCompatFocusLeave;
					AddController(_compatFocusController);
				}

				return _compatFocusController;
			}
		}

		// ------------------------------------------------------------ pointer events

		ButtonPressEventHandler _buttonPressEvent;
		ButtonReleaseEventHandler _buttonReleaseEvent;
		MotionNotifyEventHandler _motionNotifyEvent;
		EnterNotifyEventHandler _enterNotifyEvent;
		LeaveNotifyEventHandler _leaveNotifyEvent;
		ScrollEventHandler _scrollEvent;

		/// <summary>Stands in for GtkWidget::button-press-event, over GtkGestureClick::pressed.</summary>
		public event ButtonPressEventHandler ButtonPressEvent {
			add { var unused = CompatClickGesture; _buttonPressEvent += value; }
			remove { _buttonPressEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::button-release-event, over GtkGestureClick::released.</summary>
		public event ButtonReleaseEventHandler ButtonReleaseEvent {
			add { var unused = CompatClickGesture; _buttonReleaseEvent += value; }
			remove { _buttonReleaseEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::motion-notify-event.</summary>
		public event MotionNotifyEventHandler MotionNotifyEvent {
			add { var unused = CompatMotionController; _motionNotifyEvent += value; }
			remove { _motionNotifyEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::enter-notify-event.</summary>
		public event EnterNotifyEventHandler EnterNotifyEvent {
			add { var unused = CompatMotionController; _enterNotifyEvent += value; }
			remove { _enterNotifyEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::leave-notify-event.</summary>
		public event LeaveNotifyEventHandler LeaveNotifyEvent {
			add { var unused = CompatMotionController; _leaveNotifyEvent += value; }
			remove { _leaveNotifyEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::scroll-event.</summary>
		public event ScrollEventHandler ScrollEvent {
			add { var unused = CompatScrollController; _scrollEvent += value; }
			remove { _scrollEvent -= value; }
		}

		void OnCompatPressed(object o, PressedArgs args)
		{
			if (_buttonPressEvent == null)
				return;

			var e = new ButtonPressEventArgs {
				Event = new Gdk.EventButton {
					Type = Gdk.EventType.ButtonPress,
					X = args.X,
					Y = args.Y,
					XRoot = args.X,
					YRoot = args.Y,
					NPress = args.NPress,
					Button = _compatClickGesture.CurrentButton,
					State = _compatClickGesture.CurrentEventState,
					Time = _compatClickGesture.CurrentEventTime,
					Event = _compatClickGesture.CurrentEvent
				}
			};

			_buttonPressEvent(this, e);
			ClaimIfHandled(_compatClickGesture, e);
		}

		void OnCompatReleased(object o, ReleasedArgs args)
		{
			if (_buttonReleaseEvent == null)
				return;

			var e = new ButtonReleaseEventArgs {
				Event = new Gdk.EventButton {
					Type = Gdk.EventType.ButtonRelease,
					X = args.X,
					Y = args.Y,
					XRoot = args.X,
					YRoot = args.Y,
					NPress = args.NPress,
					Button = _compatClickGesture.CurrentButton,
					State = _compatClickGesture.CurrentEventState,
					Time = _compatClickGesture.CurrentEventTime,
					Event = _compatClickGesture.CurrentEvent
				}
			};

			_buttonReleaseEvent(this, e);
			ClaimIfHandled(_compatClickGesture, e);
		}

		void OnCompatMotion(object o, MotionArgs args)
		{
			if (_motionNotifyEvent == null)
				return;

			_motionNotifyEvent(this, new MotionNotifyEventArgs {
				Event = new Gdk.EventMotion {
					Type = Gdk.EventType.MotionNotify,
					X = args.X,
					Y = args.Y,
					XRoot = args.X,
					YRoot = args.Y,
					State = _compatMotionController.CurrentEventState,
					Time = _compatMotionController.CurrentEventTime,
					Event = _compatMotionController.CurrentEvent
				}
			});
		}

		void OnCompatEnter(object o, EnterArgs args)
		{
			if (_enterNotifyEvent == null)
				return;

			_enterNotifyEvent(this, new EnterNotifyEventArgs {
				Event = new Gdk.EventCrossing {
					Type = Gdk.EventType.EnterNotify,
					X = args.X,
					Y = args.Y,
					State = _compatMotionController.CurrentEventState,
					Time = _compatMotionController.CurrentEventTime,
					Event = _compatMotionController.CurrentEvent
				}
			});
		}

		void OnCompatLeave(object o, System.EventArgs args)
		{
			if (_leaveNotifyEvent == null)
				return;

			// GtkEventControllerMotion::leave carries no coordinates - the pointer is, by
			// definition, no longer over the widget. Gtk 3's leave-notify-event did carry the
			// exit point; there is nothing to derive it from here.
			_leaveNotifyEvent(this, new LeaveNotifyEventArgs {
				Event = new Gdk.EventCrossing {
					Type = Gdk.EventType.LeaveNotify,
					State = _compatMotionController.CurrentEventState,
					Time = _compatMotionController.CurrentEventTime,
					Event = _compatMotionController.CurrentEvent
				}
			});
		}

		void OnCompatScroll(object o, ScrollArgs args)
		{
			if (_scrollEvent == null)
				return;

			var e = new ScrollEventArgs {
				Event = new Gdk.EventScroll {
					Type = Gdk.EventType.Scroll,
					DeltaX = args.Dx,
					DeltaY = args.Dy,
					Direction = DirectionOf(args.Dx, args.Dy),
					State = _compatScrollController.CurrentEventState,
					Time = _compatScrollController.CurrentEventTime,
					Event = _compatScrollController.CurrentEvent
				}
			};

			_scrollEvent(this, e);

			if (e.Handled)
				args.RetVal = true;
		}

		/// <summary>
		/// Reports the discrete direction a Gtk 3 handler would have seen for an axis-aligned
		/// delta, and Smooth otherwise.
		/// </summary>
		/// <remarks>
		/// Gtk 4 has only smooth scrolling, but Gtk 3 code overwhelmingly switches on Up/Down.
		/// Reporting Smooth unconditionally would make every such switch fall through its
		/// default; reporting a direction for the dominant axis keeps the common case working
		/// while leaving the deltas available for anything that wants precision.
		/// </remarks>
		static Gdk.ScrollDirection DirectionOf(double dx, double dy)
		{
			if (dx == 0 && dy == 0)
				return Gdk.ScrollDirection.Smooth;

			if (Math.Abs(dy) >= Math.Abs(dx))
				return dy > 0 ? Gdk.ScrollDirection.Down : Gdk.ScrollDirection.Up;

			return dx > 0 ? Gdk.ScrollDirection.Right : Gdk.ScrollDirection.Left;
		}

		static void ClaimIfHandled(Gesture gesture, CompatSignalArgs args)
		{
			// Gtk 3's "return true, stop propagating" is Gtk 4's "claim the sequence": every
			// other controller watching the same sequence is cancelled.
			if (args.Handled)
				gesture.SetState(EventSequenceState.Claimed);
		}

		// ------------------------------------------------------------ key events

		KeyPressEventHandler _keyPressEvent;
		KeyReleaseEventHandler _keyReleaseEvent;

		/// <summary>Stands in for GtkWidget::key-press-event.</summary>
		public event KeyPressEventHandler KeyPressEvent {
			add { var unused = CompatKeyController; _keyPressEvent += value; }
			remove { _keyPressEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::key-release-event.</summary>
		public event KeyReleaseEventHandler KeyReleaseEvent {
			add { var unused = CompatKeyController; _keyReleaseEvent += value; }
			remove { _keyReleaseEvent -= value; }
		}

		void OnCompatKeyPressed(object o, KeyPressedArgs args)
		{
			if (_keyPressEvent == null)
				return;

			var e = new KeyPressEventArgs { Event = KeyEventFor(args.Keyval, args.Keycode, args.State, Gdk.EventType.KeyPress) };

			_keyPressEvent(this, e);

			// key-pressed is one of the few Gtk 4 signals that still returns a boolean, and it
			// means what the Gtk 3 return value meant.
			if (e.Handled)
				args.RetVal = true;
		}

		void OnCompatKeyReleased(object o, KeyReleasedArgs args)
		{
			if (_keyReleaseEvent == null)
				return;

			_keyReleaseEvent(this, new KeyReleaseEventArgs {
				Event = KeyEventFor(args.Keyval, args.Keycode, args.State, Gdk.EventType.KeyRelease)
			});
		}

		Gdk.EventKey KeyEventFor(uint keyval, uint keycode, Gdk.ModifierType state, Gdk.EventType type)
		{
			return new Gdk.EventKey {
				Type = type,
				Key = (Gdk.Key)keyval,
				KeyValue = keyval,
				HardwareKeycode = (ushort)keycode,
				State = state,
				Time = _compatKeyController.CurrentEventTime,
				Event = _compatKeyController.CurrentEvent
			};
		}

		// ------------------------------------------------------------ focus events

		FocusInEventHandler _focusInEvent;
		FocusOutEventHandler _focusOutEvent;
		FocusedHandler _focused;

		/// <summary>Stands in for GtkWidget::focus-in-event, over GtkEventControllerFocus::enter.</summary>
		public event FocusInEventHandler FocusInEvent {
			add { var unused = CompatFocusController; _focusInEvent += value; }
			remove { _focusInEvent -= value; }
		}

		/// <summary>Stands in for GtkWidget::focus-out-event, over GtkEventControllerFocus::leave.</summary>
		public event FocusOutEventHandler FocusOutEvent {
			add { var unused = CompatFocusController; _focusOutEvent += value; }
			remove { _focusOutEvent -= value; }
		}

		/// <summary>
		/// Stands in for GtkWidget::focus. Raised when the widget takes focus.
		/// </summary>
		/// <remarks>
		/// Gtk 3's ::focus was a keyboard-navigation hook that reported the DIRECTION focus was
		/// moving in and could veto the move. Gtk 4 has no equivalent an application can hook, so
		/// this fires on focus-in with <c>Direction</c> left at its default. Code that only used
		/// it to learn "I have focus now" - which is what nearly all of it did - is unaffected;
		/// code that vetoed navigation needs a GtkEventControllerKey instead.
		/// </remarks>
		public event FocusedHandler Focused {
			add { var unused = CompatFocusController; _focused += value; }
			remove { _focused -= value; }
		}

		void OnCompatFocusEnter(object o, System.EventArgs args)
		{
			if (_focusInEvent != null)
				_focusInEvent(this, new FocusInEventArgs { Event = new Gdk.EventFocus { Type = Gdk.EventType.FocusChange, In = true } });

			if (_focused != null)
				_focused(this, new FocusedArgs());
		}

		void OnCompatFocusLeave(object o, System.EventArgs args)
		{
			if (_focusOutEvent == null)
				return;

			_focusOutEvent(this, new FocusOutEventArgs { Event = new Gdk.EventFocus { Type = Gdk.EventType.FocusChange, In = false } });
		}

		// ------------------------------------------------------------ lifecycle

		/// <summary>
		/// Stands in for gtk_widget_show_all.
		/// </summary>
		/// <remarks>
		/// Gtk 4 removed it because it no longer does anything: a widget is visible when it is
		/// created, and hiding a child is a deliberate act that show_all would have undone. This
		/// shows the widget itself and nothing else, deliberately - recursing and forcing every
		/// descendant visible would resurrect children that were hidden on purpose, which is
		/// exactly the bug that got show_all deleted.
		/// </remarks>
		public void ShowAll()
		{
			// Visible = true, not Show(): Gtk 4.10 deprecated gtk_widget_show in favour of the
			// property, which is the same operation without the "show" verb's Gtk 3 connotation of
			// mapping a toplevel.
			Visible = true;
		}

		// Destroy() is NOT here. Widget.cs already has it - virtual, and already routing a
		// toplevel to gtk_window_destroy and everything else to Unparent.
		//
		// Neither is a Destroyed event, and that is deliberate rather than an omission: see the
		// note in Widget.cs above CreateNativeObject. Gtk 4 removed the ::destroy signal, and an
		// event raised only from the managed Destroy() would fire for a widget torn down by an
		// explicit call and stay silent for one torn down by its parent going away - which is the
		// "handler that silently never runs" that note refuses to ship.

		// ------------------------------------------------------------ sizing

		/// <summary>
		/// Stands in for gtk_widget_get_preferred_width, over gtk_widget_measure.
		/// </summary>
		public void GetPreferredWidth(out int minimumWidth, out int naturalWidth)
		{
			int ignoredMinimumBaseline, ignoredNaturalBaseline;
			Measure(Orientation.Horizontal, -1, out minimumWidth, out naturalWidth,
				out ignoredMinimumBaseline, out ignoredNaturalBaseline);
		}

		/// <summary>
		/// Stands in for gtk_widget_get_preferred_height, over gtk_widget_measure.
		/// </summary>
		public void GetPreferredHeight(out int minimumHeight, out int naturalHeight)
		{
			int ignoredMinimumBaseline, ignoredNaturalBaseline;
			Measure(Orientation.Vertical, -1, out minimumHeight, out naturalHeight,
				out ignoredMinimumBaseline, out ignoredNaturalBaseline);
		}

		/// <summary>
		/// Stands in for gtk_widget_size_allocate's Gtk 3 signature.
		/// </summary>
		/// <remarks>
		/// <para>Gtk 3 allocated a rectangle in the PARENT's coordinates; Gtk 4 allocates a size in
		/// the widget's own, with the position carried by a transform the parent supplies. X and Y
		/// are therefore honoured as a translation, so that a caller positioning a child by
		/// allocation still gets it positioned - but a parent that lays out its children properly
		/// should be calling <c>Allocate</c> itself.</para>
		/// <para>MEASURED: the widget is measured here first, and that is load-bearing rather than
		/// tidiness. Gtk 4 requires gtk_widget_measure() before gtk_widget_allocate(); when it is
		/// skipped, Gtk logs "Allocating size to &lt;widget&gt; without calling gtk_widget_measure().
		/// How does the code know the size to allocate?" and <b>the allocation silently does not
		/// propagate</b> - the subtree keeps whatever geometry it had, so it lays out as if the call
		/// had never happened. Gtk 3's gtk_widget_size_allocate carried no such precondition; it
		/// performed the size request itself. Callers written against Gtk 3 therefore do not
		/// measure, and cannot be expected to, so this shim has to satisfy the precondition on their
		/// behalf or it is not standing in for the function it claims to.</para>
		/// <para>Measuring the child that is about to fill this widget is NOT a substitute: the
		/// precondition is on the widget being allocated. The height pass is given the allocated
		/// width, because height-for-width is the direction Gtk measures in, matching what
		/// <c>Container.AllocateChildren</c> already does.</para>
		/// </remarks>
		public void SizeAllocate(Gdk.Rectangle allocation)
		{
			Gsk.Transform transform = null;

			if (allocation.X != 0 || allocation.Y != 0) {
				var offset = new Graphene.Point();
				offset.Init(allocation.X, allocation.Y);
				transform = new Gsk.Transform().Translate(offset);
			}

			int ignored;
			Measure(Orientation.Horizontal, -1, out ignored, out ignored, out ignored, out ignored);
			Measure(Orientation.Vertical, allocation.Width, out ignored, out ignored, out ignored, out ignored);

			Allocate(allocation.Width, allocation.Height, -1, transform);
		}

		/// <summary>
		/// Stands in for gtk_widget_add_events. Accepted and ignored: Gtk 4 has no event masks,
		/// and a widget receives whatever its controllers ask for.
		/// </summary>
		public void AddEvents(int events)
		{
		}

		/// <summary>
		/// Stands in for GtkWidget:no-show-all. Accepted and ignored, because
		/// <see cref="ShowAll"/> no longer recurses and so has nothing to opt out of.
		/// </summary>
		public bool NoShowAll { get; set; }
	}
}
