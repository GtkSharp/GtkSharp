/* Dialog and Message Boxes
 *
 * Dialog widgets are used to pop up a transient window for user feedback.
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Dialog and Message Boxes", "DemoDialog.cs")]
	public class DemoDialog : Gtk.Window
	{
		private Entry entry1;
		private Entry entry2;

		public DemoDialog () : base ("Dialogs")
		{
			BorderWidth = 8;

			Frame frame = new Frame ("Dialogs");
			Add (frame);

			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			frame.Add (vbox);

			// Standard message dialog
			HBox hbox = new HBox (false,8);
			vbox.PackStart (hbox, false, false, 0);
			Button button = new Button ("_Message Dialog");
			button.Clicked += new EventHandler (MessageDialogClicked);
			hbox.PackStart (button, false, false, 0);
			vbox.PackStart (new HSeparator(), false, false, 0);

			// Interactive dialog
			hbox = new HBox (false, 8);
			vbox.PackStart (hbox, false, false, 0);
			VBox vbox2 = new VBox (false, 0);

			button = new Button ("_Interactive Dialog");
			button.Clicked += new EventHandler (InteractiveDialogClicked);
			hbox.PackStart (vbox2, false, false, 0);
			vbox2.PackStart (button, false, false, 0);

			Table table = new Table (2, 2, false);
			table.RowSpacing = 4;
			table.ColumnSpacing = 4;
			hbox.PackStart (table, false, false, 0);

			Label label = new Label ("_Entry1");
			table.Attach (label, 0, 1, 0, 1);
			entry1 = new Entry ();
			table.Attach (entry1, 1, 2, 0, 1);
			label.MnemonicWidget = entry1;

			label = new Label ("E_ntry2");
			table.Attach (label,0,1,1,2);
			entry2 = new Entry ();
			table.Attach (entry2, 1, 2, 1, 2);
			label.MnemonicWidget = entry2;

			ShowAll ();
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		private int i = 1;
		private void MessageDialogClicked (object o, EventArgs args)
		{
			using (Dialog dialog = new MessageDialog (this,
								  DialogFlags.Modal | DialogFlags.DestroyWithParent,
								  MessageType.Info,
								  ButtonsType.Ok,
								  "This message box has been popped up the following\nnumber of times:\n\n {0}",
								  i)) {
				dialog.Run ();
				dialog.Hide ();
			}

			i++;
		}

		private void InteractiveDialogClicked (object o, EventArgs args)
		{
			Dialog dialog = new Dialog ("Interactive Dialog", this,
						    DialogFlags.Modal | DialogFlags.DestroyWithParent,
						    Gtk.Stock.Ok, ResponseType.Ok,
						    "_Non-stock Button", ResponseType.Cancel);

			HBox hbox = new HBox (false, 8);
			hbox.BorderWidth = 8;
			dialog.VBox.PackStart (hbox, false, false, 0);

			Image stock = new Image (Stock.DialogQuestion, IconSize.Dialog);
			hbox.PackStart (stock, false, false, 0);

			Table table = new Table (2, 2, false);
			table.RowSpacing = 4;
			table.ColumnSpacing = 4;
			hbox.PackStart (table, true, true, 0);

			Label label = new Label ("_Entry1");
			table.Attach (label, 0, 1, 0, 1);
			Entry localEntry1 = new Entry ();
			localEntry1.Text = entry1.Text;
			table.Attach (localEntry1, 1, 2, 0, 1);
			label.MnemonicWidget = localEntry1;

			label = new Label ("E_ntry2");
			table.Attach (label, 0, 1, 1, 2);
			Entry localEntry2 = new Entry ();
			localEntry2.Text = entry2.Text;
			table.Attach (localEntry2, 1, 2, 1, 2);
			label.MnemonicWidget = localEntry2;

			hbox.ShowAll ();

			ResponseType response = (ResponseType) dialog.Run ();

			if (response == ResponseType.Ok) {
				entry1.Text = localEntry1.Text;
				entry2.Text = localEntry2.Text;
			}

			dialog.Destroy ();
		}
	}
}
