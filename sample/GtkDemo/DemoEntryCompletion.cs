using System;
using Gtk;

namespace GtkDemo 
{
	[Demo ("Entry Completion", "DemoEntryCompletion.cs")]
	public class DemoEntryCompletion : Dialog
	{
		public DemoEntryCompletion () : base ("Demo Entry Completion", null, DialogFlags.DestroyWithParent)
		{
			this.BorderWidth = 10;
			this.Resizable = false;

			Label label = new Label ("Completion demo, try writing <b>total</b> or <b>gnome</b> for example.");
			label.UseMarkup = true;
			this.VBox.PackStart (label, false, true, 0);

			Entry entry = new Entry ();
			entry.Completion = new EntryCompletion ();
			entry.Completion.Model = CreateCompletionModel ();
			entry.Completion.TextColumn = 0;
			this.VBox.PackStart (entry, false, true, 0);

			this.AddButton (Stock.Close, ResponseType.Close);

			this.ShowAll ();
			this.Run ();
			this.Hide ();
			this.Destroy ();
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

