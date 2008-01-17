// Assistant.cs - Gtk.Assistant Sample App
//
// Author:  Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2008 Novell, Inc.
//
// Adapted to C# with modifications from a C tutorial sample at
// http://www.linuxquestions.org/linux/articles/Technical/New_GTK_Widgets_GtkAssistant


namespace GtkSharpSamples {

	using System;
	using Gtk;

	public class SampleAssistant : Assistant {

		public static int Main (string[] argv)
		{
			Application.Init ();
			new SampleAssistant ().ShowAll ();
			Application.Run ();
			return 0;
		}

		ProgressBar progress_bar;
   
		public SampleAssistant () 
		{
			SetSizeRequest (450, 300);
			Title = "Gtk.Assistant Sample";
  
			Label lbl = new Label ("Click the forward button to continue.");
			AppendPage (lbl);
			SetPageTitle (lbl, "Introduction");
			SetPageType (lbl, AssistantPageType.Intro);
			SetPageComplete (lbl, true);

			HBox box = new HBox (false, 6);
			box.PackStart (new Label ("Enter some text: "), false, false, 6);
			Entry entry = new Entry ();
			entry.Changed += new EventHandler (EntryChanged);
			box.PackStart (entry, false, false, 6);
			AppendPage (box);
			SetPageTitle (box, "Getting Some Input");
			SetPageType (box, AssistantPageType.Content);

			CheckButton chk = new CheckButton ("I think Gtk# is awesome.");
			chk.Toggled += new EventHandler (ButtonToggled);
			AppendPage (chk);
			SetPageTitle (chk, "Provide Feedback");
			SetPageType (chk, AssistantPageType.Content);

			Alignment al = new Alignment (0.5f, 0.5f, 0.0f, 0.0f);
			box = new HBox (false, 6);
			progress_bar = new ProgressBar ();
			box.PackStart (progress_bar, true, true, 6);
			Button btn = new Button ("Make progress");
			btn.Clicked += new EventHandler (ButtonClicked);
			box.PackStart (btn, false, false, 6);
			al.Add (box);
			AppendPage (al);
			SetPageTitle (al, "Show Some Progress");
			SetPageType (al, AssistantPageType.Progress);

			lbl = new Label ("In addition to being able to type,\nYou obviously have great taste in software.");
			AppendPage (lbl);
			SetPageTitle (lbl, "Congratulations");
			SetPageType (lbl, AssistantPageType.Confirm);
			SetPageComplete (lbl, true);
  
			Cancel += new EventHandler (AssistantCancel);
			Close += new EventHandler (AssistantClose);
		}

		protected override bool OnDeleteEvent (Gdk.Event ev)
		{
			Console.WriteLine ("Assistant Destroyed prematurely");
			Application.Quit ();
			return true;
		}

		// If there is text in the GtkEntry, set the page as complete.
		void EntryChanged (object o, EventArgs args)
		{
			string text = (o as Gtk.Entry).Text;
			SetPageComplete (GetNthPage (CurrentPage), text.Length > 0);
		}

		// If check button is checked, set the page as complete.
		void ButtonToggled (object o, EventArgs args)
		{
			bool active = (o as ToggleButton).Active;
			SetPageComplete (o as Widget, active);
		}

		// Progress 10% per second after button clicked.
		void ButtonClicked (object o, EventArgs args)
		{
			(o as Widget).Sensitive = false;
			GLib.Timeout.Add (500, new GLib.TimeoutHandler (TimeoutCallback));
		}

		double fraction = 0.0;

		bool TimeoutCallback ()
		{
			fraction += 5.0;
			progress_bar.Fraction = fraction / 100.0;
			progress_bar.Text = String.Format ("{0}% Complete", fraction);
			if (fraction == 100.0) {
				SetPageComplete (progress_bar.Parent.Parent, true);
				return false;
			}
			return true;
		}

		void AssistantCancel (object o, EventArgs args)
		{
			Console.WriteLine ("Assistant cancelled.");
			Destroy ();
			Application.Quit ();
		}

		void AssistantClose (object o, EventArgs args)
		{
			Console.WriteLine ("Assistant ran to completion.");
			Destroy ();
			Application.Quit ();
		}
	}
}

