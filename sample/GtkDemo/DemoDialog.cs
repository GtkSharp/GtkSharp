//
// DemoDialog.cs, port of dialog.c from gtk-demo
//
// Author Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) 2003, Ximian Inc.

/* Dialog and Message Boxes
 *
 * Dialog widgets are used to pop up a transient window for user feedback.
 */

// TODO: - Couldn't find a good equivalent to gtk_dialog_new_with_buttons
//         in InteractiveDialogClicked
//       - Check how to handle response type. Can we make the button to have
//         the binding to the cancel signal automagicly like in 
//         gtk_dialog_new_with_buttons or should we just use the if ?

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
			this.DeleteEvent += new DeleteEventHandler(WindowDelete);
			this.BorderWidth = 8;

			Frame frame = new Frame ("Dialogs");
			this.Add (frame);

			VBox vbox = new VBox (false, 8);
			vbox.BorderWidth = 8;
			frame.Add (vbox);

			// Standard message dialog		
			HBox hbox = new HBox (false,8);
			vbox.PackStart (hbox, false, false, 0);
			Button button = new Button ("_Message Dialog");
			button.Clicked += new EventHandler (MessageDialogClicked);
			hbox.PackStart (button, false, false, 0);
			vbox.PackStart (new HSeparator());

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
			hbox.PackStart (table);

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
			
			this.ShowAll ();
		}

		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}

		private int i = 1;
		private void MessageDialogClicked (object o, EventArgs args)
		{
			using (Dialog dialog = new MessageDialog (this, 
					DialogFlags.Modal | DialogFlags.DestroyWithParent,
					MessageType.Info,
					ButtonsType.Ok,
					"This message box has been popped up the following\n number of times:\n\n {0:D} ",
					i)) {
				dialog.Run ();
				dialog.Hide ();
			}

			i++;
		}

		private void InteractiveDialogClicked (object o, EventArgs args)
		{
			using (MessageDialog dialog = new MessageDialog (this,
					DialogFlags.Modal | DialogFlags.DestroyWithParent,
					MessageType.Question,
					ButtonsType.Ok,
					null)) {
			
				dialog.AddButton ("_Non-stock Button", (int) ResponseType.Cancel);

				HBox hbox = new HBox (false, 8);
				hbox.BorderWidth = 8;
				dialog.VBox.PackStart (hbox, false, false, 0);

				Table table = new Table (2, 2, false);
				table.RowSpacing = 4;
				table.ColumnSpacing = 4;
				hbox.PackStart (table, false, false, 0);

				Label label = new Label ("_Entry1");
				table.Attach (label, 0, 1, 0, 1);
				Entry localEntry1 = new Entry();
				localEntry1.Text = entry1.Text;
				table.Attach (localEntry1, 1, 2, 0, 1);
				label.MnemonicWidget = localEntry1;

				label = new Label ("E_ntry2");
				table.Attach (label, 0, 1, 1, 2);
				Entry localEntry2 = new Entry();
				localEntry2.Text = entry2.Text;
				table.Attach (localEntry2, 1, 2, 1, 2);
				label.MnemonicWidget = localEntry2;
			
				hbox.ShowAll ();
	
				ResponseType response = (ResponseType) dialog.Run ();

				if (response == ResponseType.Ok)
				{
					entry1.Text = localEntry1.Text;
					entry2.Text = localEntry2.Text;
				}			
			
				dialog.Hide ();
			}
		}
	}		     
}

