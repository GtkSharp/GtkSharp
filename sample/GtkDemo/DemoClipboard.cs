using System;
using Gtk;

namespace GtkDemo 
{
	public class DemoClipboard : Gtk.Window
	{
		Entry pasteEntry, copyEntry;

		public DemoClipboard () : base ("Demo Clipboard")
		{
			this.DeleteEvent += new DeleteEventHandler (OnDelete);

			VBox vbox = new VBox ();
			vbox.BorderWidth = 8;
			Label copyLabel = new Label ("\"Copy\" will copy the text\nin the entry to the clipboard");
			vbox.PackStart (copyLabel, false, true, 0);
			vbox.PackStart (CreateCopyBox (), false, true, 0);

			Label pasteLabel = new Label ("\"Paste\" will paste the text from the clipboard to the entry");
			vbox.PackStart (pasteLabel, false, false, 0);
			vbox.PackStart (CreatePasteBox (), false, false, 0);
	
			this.Add (vbox);
			this.ShowAll ();
		}

		HBox CreateCopyBox ()
		{
			HBox hbox = new HBox (false, 4);
			hbox.BorderWidth = 8;
			copyEntry = new Entry ();
			Button copyButton = new Button (Stock.Copy);
			copyButton.Clicked += new EventHandler (OnCopyClicked);
			hbox.PackStart (copyEntry, true, true, 0);
			hbox.PackStart (copyButton, false, false, 0);
			return hbox;
		}

		HBox CreatePasteBox ()
		{
			HBox hbox = new HBox (false, 4);
			hbox.BorderWidth = 8;
			pasteEntry = new Entry ();
			Button pasteButton = new Button (Stock.Paste);
			pasteButton.Clicked += new EventHandler (OnPasteClicked);
			hbox.PackStart (pasteEntry, true, true, 0);
			hbox.PackStart (pasteButton, false, false, 0);
			return hbox;
		}

		void OnCopyClicked (object sender, EventArgs a)
		{
			Clipboard clipboard = copyEntry.GetClipboard (Gdk.Selection.Clipboard);
			clipboard.SetText (copyEntry.Text);
		}

		void OnPasteClicked (object sender, EventArgs a)
		{
			Clipboard clipboard = pasteEntry.GetClipboard (Gdk.Selection.Clipboard);
			pasteEntry.Text = clipboard.WaitForText ();
		}

		void OnDelete (object sender, DeleteEventArgs a)
		{
			this.Hide ();
			this.Destroy ();
			a.RetVal = true;
		}
	}
}

