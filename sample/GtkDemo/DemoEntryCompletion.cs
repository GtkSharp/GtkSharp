/* Entry Completion
 *
 * GtkEntryCompletion provides a mechanism for adding support for
 * completion in GtkEntry.
 *
 */
using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Entry Completion", "DemoEntryCompletion.cs")]
	public class DemoEntryCompletion : Dialog
	{
		public DemoEntryCompletion () : base ("Demo Entry Completion", null, DialogFlags.DestroyWithParent)
		{
			Resizable = false;

			VBox vbox = new VBox (false, 5);
			vbox.BorderWidth = 5;
			this.VBox.PackStart (vbox, true, true, 0);

			Label label = new Label ("Completion demo, try writing <b>total</b> or <b>gnome</b> for example.");
			label.UseMarkup = true;
			vbox.PackStart (label, false, true, 0);

			Entry entry = new Entry ();
			vbox.PackStart (entry, false, true, 0);

			entry.Completion = new EntryCompletion ();
			entry.Completion.Model = CreateCompletionModel ();
			entry.Completion.TextColumn = 0;

			AddButton (Stock.Close, ResponseType.Close);

			ShowAll ();
			Run ();
			Destroy ();
		}

		TreeModel CreateCompletionModel ()
		{
			ListStore store = new ListStore (typeof (string));

			store.AppendValues ("GNOME");
			store.AppendValues ("total");
			store.AppendValues ("totally");

			return store;
		}
	}
}
