using Gtk;
using Gdk;
using GLib;
using System;

public class TestDnd {

	private static readonly string [] drag_icon_xpm = new string [] {
		"36 48 9 1",
		" 	c None",
		".	c #020204",
		"+	c #8F8F90",
		"@	c #D3D3D2",
		"#	c #AEAEAC",
		"$	c #ECECEC",
		"%	c #A2A2A4",
		"&	c #FEFEFC",
		"*	c #BEBEBC",
		"               .....................",
		"              ..&&&&&&&&&&&&&&&&&&&.",
		"             ...&&&&&&&&&&&&&&&&&&&.",
		"            ..&.&&&&&&&&&&&&&&&&&&&.",
		"           ..&&.&&&&&&&&&&&&&&&&&&&.",
		"          ..&&&.&&&&&&&&&&&&&&&&&&&.",
		"         ..&&&&.&&&&&&&&&&&&&&&&&&&.",
		"        ..&&&&&.&&&@&&&&&&&&&&&&&&&.",
		"       ..&&&&&&.*$%$+$&&&&&&&&&&&&&.",
		"      ..&&&&&&&.%$%$+&&&&&&&&&&&&&&.",
		"     ..&&&&&&&&.#&#@$&&&&&&&&&&&&&&.",
		"    ..&&&&&&&&&.#$**#$&&&&&&&&&&&&&.",
		"   ..&&&&&&&&&&.&@%&%$&&&&&&&&&&&&&.",
		"  ..&&&&&&&&&&&.&&&&&&&&&&&&&&&&&&&.",
		" ..&&&&&&&&&&&&.&&&&&&&&&&&&&&&&&&&.",
		"................&$@&&&@&&&&&&&&&&&&.",
		".&&&&&&&+&&#@%#+@#@*$%$+$&&&&&&&&&&.",
		".&&&&&&&+&&#@#@&&@*%$%$+&&&&&&&&&&&.",
		".&&&&&&&+&$%&#@&#@@#&#@$&&&&&&&&&&&.",
		".&&&&&&@#@@$&*@&@#@#$**#$&&&&&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&@%&%$&&&&&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&.",
		".&&&&&&&&$#@@$&&&&&&&&&&&&&&&&&&&&&.",
		".&&&&&&&&&+&$+&$&@&$@&&$@&&&&&&&&&&.",
		".&&&&&&&&&+&&#@%#+@#@*$%&+$&&&&&&&&.",
		".&&&&&&&&&+&&#@#@&&@*%$%$+&&&&&&&&&.",
		".&&&&&&&&&+&$%&#@&#@@#&#@$&&&&&&&&&.",
		".&&&&&&&&@#@@$&*@&@#@#$#*#$&&&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&&&$%&%$&&&&&&&&.",
		".&&&&&&&&&&$#@@$&&&&&&&&&&&&&&&&&&&.",
		".&&&&&&&&&&&+&$%&$$@&$@&&$@&&&&&&&&.",
		".&&&&&&&&&&&+&&#@%#+@#@*$%$+$&&&&&&.",
		".&&&&&&&&&&&+&&#@#@&&@*#$%$+&&&&&&&.",
		".&&&&&&&&&&&+&$+&*@&#@@#&#@$&&&&&&&.",
		".&&&&&&&&&&$%@@&&*@&@#@#$#*#&&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&$%&%$&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&.",
		".&&&&&&&&&&&&&&$#@@$&&&&&&&&&&&&&&&.",
		".&&&&&&&&&&&&&&&+&$%&$$@&$@&&$@&&&&.",
		".&&&&&&&&&&&&&&&+&&#@%#+@#@*$%$+$&&.",
		".&&&&&&&&&&&&&&&+&&#@#@&&@*#$%$+&&&.",
		".&&&&&&&&&&&&&&&+&$+&*@&#@@#&#@$&&&.",
		".&&&&&&&&&&&&&&$%@@&&*@&@#@#$#*#&&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&&&&&$%&%$&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&.",
		".&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&.",
		"...................................."
	};

	private static readonly string [] trashcan_closed_xpm = new string [] {
		"64 80 17 1",
		" 	c None",
		".	c #030304",
		"+	c #5A5A5C",
		"@	c #323231",
		"#	c #888888",
		"$	c #1E1E1F",
		"%	c #767677",
		"&	c #494949",
		"*	c #9E9E9C",
		"=	c #111111",
		"-	c #3C3C3D",
		";	c #6B6B6B",
		">	c #949494",
		",	c #282828",
		"'	c #808080",
		")	c #545454",
		"!	c #AEAEAC",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                       ==......=$$...===                        ",
		"                 ..$------)+++++++++++++@$$...                  ",
		"             ..=@@-------&+++++++++++++++++++-....              ",
		"          =.$$@@@-&&)++++)-,$$$$=@@&+++++++++++++,..$           ",
		"         .$$$$@@&+++++++&$$$@@@@-&,$,-++++++++++;;;&..          ",
		"        $$$$,@--&++++++&$$)++++++++-,$&++++++;%%'%%;;$@         ",
		"       .-@@-@-&++++++++-@++++++++++++,-++++++;''%;;;%*-$        ",
		"       +------++++++++++++++++++++++++++++++;;%%%;;##*!.        ",
		"        =+----+++++++++++++++++++++++;;;;;;;;;;;;%'>>).         ",
		"         .=)&+++++++++++++++++;;;;;;;;;;;;;;%''>>#>#@.          ",
		"          =..=&++++++++++++;;;;;;;;;;;;;%###>>###+%==           ",
		"           .&....=-+++++%;;####''''''''''##'%%%)..#.            ",
		"           .+-++@....=,+%#####'%%%%%%%%%;@$-@-@*++!.            ",
		"           .+-++-+++-&-@$$=$=......$,,,@;&)+!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           =+-++-+++-+++++++++!++++!++++!+++!++!+++=            ",
		"            $.++-+++-+++++++++!++++!++++!+++!++!+.$             ",
		"              =.++++++++++++++!++++!++++!+++!++.=               ",
		"                 $..+++++++++++++++!++++++...$                  ",
		"                      $$=.............=$$                       ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                "
	};

	private static readonly string [] trashcan_open_xpm = new string [] {
		"64 80 17 1",
		" 	c None",
		".	c #030304",
		"+	c #5A5A5C",
		"@	c #323231",
		"#	c #888888",
		"$	c #1E1E1F",
		"%	c #767677",
		"&	c #494949",
		"*	c #9E9E9C",
		"=	c #111111",
		"-	c #3C3C3D",
		";	c #6B6B6B",
		">	c #949494",
		",	c #282828",
		"'	c #808080",
		")	c #545454",
		"!	c #AEAEAC",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                      .=.==.,@                  ",
		"                                   ==.,@-&&&)-=                 ",
		"                                 .$@,&++;;;%>*-                 ",
		"                               $,-+)+++%%;;'#+.                 ",
		"                            =---+++++;%%%;%##@.                 ",
		"                           @)++++++++;%%%%'#%$                  ",
		"                         $&++++++++++;%%;%##@=                  ",
		"                       ,-++++)+++++++;;;'#%)                    ",
		"                      @+++&&--&)++++;;%'#'-.                    ",
		"                    ,&++-@@,,,,-)++;;;'>'+,                     ",
		"                  =-++&@$@&&&&-&+;;;%##%+@                      ",
		"                =,)+)-,@@&+++++;;;;%##%&@                       ",
		"               @--&&,,@&)++++++;;;;'#)@                         ",
		"              ---&)-,@)+++++++;;;%''+,                          ",
		"            $--&)+&$-+++++++;;;%%'';-                           ",
		"           .,-&+++-$&++++++;;;%''%&=                            ",
		"          $,-&)++)-@++++++;;%''%),                              ",
		"         =,@&)++++&&+++++;%'''+$@&++++++                        ",
		"        .$@-++++++++++++;'#';,........=$@&++++                  ",
		"       =$@@&)+++++++++++'##-.................=&++               ",
		"      .$$@-&)+++++++++;%#+$.....................=)+             ",
		"      $$,@-)+++++++++;%;@=........................,+            ",
		"     .$$@@-++++++++)-)@=............................            ",
		"     $,@---)++++&)@===............................,.            ",
		"    $-@---&)))-$$=..............................=)!.            ",
		"     --&-&&,,$=,==...........................=&+++!.            ",
		"      =,=$..=$+)+++++&@$=.............=$@&+++++!++!.            ",
		"           .)-++-+++++++++++++++++++++++++++!++!++!.            ",
		"           .+-++-+++++++++++++++++++++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!+++!!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           .+-++-+++-+++++++++!++++!++++!+++!++!++!.            ",
		"           =+-++-+++-+++++++++!++++!++++!+++!++!+++=            ",
		"            $.++-+++-+++++++++!++++!++++!+++!++!+.$             ",
		"              =.++++++++++++++!++++!++++!+++!++.=               ",
		"                 $..+++++++++++++++!++++++...$                  ",
		"                      $$==...........==$$                       ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                ",
		"                                                                "
	};

	private static Pixbuf trashcan_open_pixbuf;
	private static Pixbuf trashcan_closed_pixbuf;

	private static bool have_drag;

	enum TargetType {
		String,
		RootWindow
	};

	private static TargetEntry [] target_table = new TargetEntry [] {
		new TargetEntry ("STRING", 0, (uint) TargetType.String ),
		new TargetEntry ("text/plain", 0, (uint) TargetType.String),
		new TargetEntry ("application/x-rootwindow-drop", 0, (uint) TargetType.RootWindow)
	};

	private static void HandleTargetDragLeave (object sender, DragLeaveArgs args)
	{
		Console.WriteLine ("leave");
		have_drag = false;

		// FIXME?  Kinda wonky binding.
		(sender as Gtk.Image).FromPixbuf = trashcan_closed_pixbuf;
	}

	private static void HandleTargetDragMotion (object sender, DragMotionArgs args)
	{
		if (! have_drag) {
			have_drag = true;
			// FIXME?  Kinda wonky binding.
			(sender as Gtk.Image).FromPixbuf = trashcan_open_pixbuf;
		}

		Widget source_widget = Gtk.Drag.GetSourceWidget (args.Context);
		Console.WriteLine ("motion, source {0}", source_widget == null ? "null" : source_widget.ToString ());

		Atom [] targets = args.Context.Targets;
		foreach (Atom a in targets)
			Console.WriteLine (a.Name); 

		Gdk.Drag.Status (args.Context, args.Context.SuggestedAction, args.Time);
		args.RetVal = true;
	}

	private static void HandleTargetDragDrop (object sender, DragDropArgs args)
	{
		Console.WriteLine ("drop");
		have_drag = false;
		(sender as Gtk.Image).FromPixbuf = trashcan_closed_pixbuf;

#if BROKEN			// Context.Targets is not defined in the bindings
		if (Context.Targets.Length != 0) {
			Drag.GetData (sender, context, Context.Targets.Data as Gdk.Atom, args.Time);
			args.RetVal = true;
		}
#endif

		args.RetVal = false;
	}

	private static void HandleTargetDragDataReceived (object sender, DragDataReceivedArgs args)
	{
		if (args.SelectionData.Length >=0 && args.SelectionData.Format == 8) {
			Console.WriteLine ("Received {0} in trashcan", args.SelectionData);
			Gtk.Drag.Finish (args.Context, true, false, args.Time);
		}

		Gtk.Drag.Finish (args.Context, false, false, args.Time);
	}

	private static void HandleLabelDragDataReceived (object sender, DragDataReceivedArgs args)
	{
		if (args.SelectionData.Length >=0 && args.SelectionData.Format == 8) {
			Console.WriteLine ("Received {0} in label", args.SelectionData);
			Gtk.Drag.Finish (args.Context, true, false, args.Time);
		}

		Gtk.Drag.Finish (args.Context, false, false, args.Time);
	}

	private static void HandleSourceDragDataGet (object sender, DragDataGetArgs args)
	{
		if (args.Info == (uint) TargetType.RootWindow)
			Console.WriteLine ("I was dropped on the rootwin");
		else
			args.SelectionData.Text = "I'm data!";
	}


	// The following is a rather elaborate example demonstrating/testing
	// changing of the window heirarchy during a drag - in this case,
	// via a "spring-loaded" popup window.

	private static Gtk.Window popup_window = null;

	private static bool popped_up = false;
	private static bool in_popup = false;
	private static uint popdown_timer = 0;
	private static uint popup_timer = 0;

	private static bool HandlePopdownCallback ()
	{
		popdown_timer = 0;
		popup_window.Hide ();
		popped_up = false;

		return false;
	}

	private static void HandlePopupMotion (object sender, DragMotionArgs args)
	{
		if (! in_popup) {
			in_popup = true;
			if (popdown_timer != 0) {
				Console.WriteLine ("removed popdown");
				GLib.Source.Remove (popdown_timer);
				popdown_timer = 0;
			}
		}

		args.RetVal = true;
	}

	private static void HandlePopupLeave (object sender, DragLeaveArgs args)
	{
		if (in_popup) {
			in_popup = false;
			if (popdown_timer == 0) {
				Console.WriteLine ("added popdown");
				popdown_timer = GLib.Timeout.Add (500, new TimeoutHandler (HandlePopdownCallback));
			}
		}
	}

	private static bool HandlePopupCallback ()
	{
		if (! popped_up) {
			if (popup_window == null) {
				Button button;
				Table table;

				popup_window = new Gtk.Window (Gtk.WindowType.Popup);
				popup_window.SetPosition (WindowPosition.Mouse);

				table = new Table (3, 3, false);

				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++) {
						string label = String.Format ("{0},{1}", i, j);
						button = Button.NewWithLabel (label);

						table.Attach (button, (uint) i, (uint) i + 1, (uint) j, (uint) j + 1,
							      AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill,
							      0, 0);

						Gtk.Drag.DestSet (button, DestDefaults.All,
								  target_table, DragAction.Copy | DragAction.Move);

						button.DragMotion += new DragMotionHandler (HandlePopupMotion);
						button.DragLeave += new DragLeaveHandler (HandlePopupLeave);
					}

				table.ShowAll ();
				popup_window.Add (table);
			}

			popup_window.Show ();
			popped_up = true;
		}

		popdown_timer = GLib.Timeout.Add (500, new TimeoutHandler (HandlePopdownCallback));
		popup_timer = 0;

		return false;
	}

	private static void HandlePopsiteMotion (object sender, DragMotionArgs args)
	{
		if (popup_timer == 0)
			popup_timer = GLib.Timeout.Add (500, new TimeoutHandler (HandlePopupCallback));

		args.RetVal = true;
	}

	private static void HandlePopsiteLeave (object sender, DragLeaveArgs args)
	{
		if (popup_timer != 0) {
			GLib.Source.Remove (popup_timer);
			popup_timer = 0;
		}
	}

	private static void HandleSourceDragDataDelete (object sender, DragDataDeleteArgs args)
	{
		Console.WriteLine ("Delete the data!");
	}

	public static void Main (string [] args)
	{
		Gtk.Window window;
		Table table;
		Label label;
		Gtk.Image pixmap;
		Button button;
		Pixbuf drag_icon_pixbuf;

		Application.Init ();

		window = new Gtk.Window (Gtk.WindowType.Toplevel);
		window.DeleteEvent += new DeleteEventHandler (OnDelete);

		table = new Table (2, 2, false);
		window.Add (table);

		// FIXME should get a string[], not a string.
		drag_icon_pixbuf = new Pixbuf (drag_icon_xpm);
		trashcan_open_pixbuf = new Pixbuf (trashcan_open_xpm);
		trashcan_closed_pixbuf = new Pixbuf (trashcan_closed_xpm);

		label = new Label ("Drop Here\n");

		Gtk.Drag.DestSet (label, DestDefaults.All, target_table, DragAction.Copy | DragAction.Move);

		label.DragDataReceived += new DragDataReceivedHandler (HandleLabelDragDataReceived);

		table.Attach (label, 0, 1, 0, 1, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill, 0, 0);

		label = new Label ("Popup\n");

		Gtk.Drag.DestSet (label, DestDefaults.All, target_table, DragAction.Copy | DragAction.Move);

		table.Attach (label, 1, 2, 1, 2,
			      AttachOptions.Expand | AttachOptions.Fill,
			      AttachOptions.Expand | AttachOptions.Fill, 0, 0);

		label.DragMotion += new DragMotionHandler (HandlePopsiteMotion);
		label.DragLeave += new DragLeaveHandler (HandlePopsiteLeave);

		pixmap = new Gtk.Image (trashcan_closed_pixbuf);
		Gtk.Drag.DestSet (pixmap, 0, null, 0);
		table.Attach (pixmap, 1, 2, 0, 1,
			      AttachOptions.Expand | AttachOptions.Fill,
			      AttachOptions.Expand | AttachOptions.Fill, 0, 0);

		pixmap.DragLeave += new DragLeaveHandler (HandleTargetDragLeave);
		pixmap.DragMotion += new DragMotionHandler (HandleTargetDragMotion);
		pixmap.DragDrop += new DragDropHandler (HandleTargetDragDrop);
		pixmap.DragDataReceived += new DragDataReceivedHandler (HandleTargetDragDataReceived);

		button = new Button ("Drag Here\n");

		Gtk.Drag.SourceSet (button, Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask,
				    target_table, DragAction.Copy | DragAction.Move);

		// FIXME can I pass a pixbuf here instead?
		// Gtk.Drag.SourceSetIcon (button, window.Colormap, drag_icon, drag_mask);

		table.Attach (button, 0, 1, 1, 2,
			      AttachOptions.Expand | AttachOptions.Fill,
			      AttachOptions.Expand | AttachOptions.Fill, 0, 0);

		button.DragDataGet += new DragDataGetHandler (HandleSourceDragDataGet);
		button.DragDataDelete += new DragDataDeleteHandler (HandleSourceDragDataDelete);

		window.ShowAll ();

		Application.Run ();
	}

	private static void OnDelete (object o, DeleteEventArgs e)
	{
		Application.Quit ();
	}
}
