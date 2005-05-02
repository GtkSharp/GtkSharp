/* Clipboard
 *
 * GtkClipboard is used for clipboard handling. This demo shows how to
 * copy and paste text to and from the clipboard.
 *
 * This is actually from gtk+ 2.6's gtk-demo, but doesn't use any 2.6
 * functionality
 */
using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Clipboard", "DemoClipboard.cs")]
	public class DemoClipboard : Gtk.Window
	{
		Entry pasteEntry, copyEntry;

		public DemoClipboard () : base ("Demo Clipboard")
		{
			VBox vbox = new VBox ();
			vbox.BorderWidth = 8;
			Label copyLabel = new Label ("\"Copy\" will copy the text\nin the entry to the clipboard");
			vbox.PackStart (copyLabel, false, true, 0);
			vbox.PackStart (CreateCopyBox (), false, true, 0);

			Label pasteLabel = new Label ("\"Paste\" will paste the text from the clipboard to the entry");
			vbox.PackStart (pasteLabel, false, false, 0);
			vbox.PackStart (CreatePasteBox (), false, false, 0);

			Add (vbox);
			ShowAll ();
		}

		HBox CreateCopyBox ()
		{
			HBox hbox = new HBox (false, 4);
			hbox.BorderWidth = 8;
			copyEntry = new Entry ();
			Button copyButton = new Button (Stock.Copy);
			copyButton.Clicked += new EventHandler (CopyClicked);
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
			pasteButton.Clicked += new EventHandler (PasteClicked);
			hbox.PackStart (pasteEntry, true, true, 0);
			hbox.PackStart (pasteButton, false, false, 0);
			return hbox;
		}

		void CopyClicked (object obj, EventArgs args)
		{
			Clipboard clipboard = copyEntry.GetClipboard (Gdk.Selection.Clipboard);
			clipboard.Text = copyEntry.Text;
		}

		void PasteClicked (object obj, EventArgs args)
		{
			Clipboard clipboard = pasteEntry.GetClipboard (Gdk.Selection.Clipboard);
			clipboard.RequestText (new ClipboardTextReceivedFunc (PasteReceived));
		}

		void PasteReceived (Clipboard clipboard, string text)
		{
			pasteEntry.Text = text;
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}
