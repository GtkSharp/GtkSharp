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

			Label label = new Label ("Completion demo, try writing <b>total</b> or <b>gnome</b> for example.");
			label.UseMarkup = true;
			this.VBox.PackStart (label, false, true, 0);

			Entry entry = new Entry ();
			// FIXME: no way to set model
			//entry.Completion = new EntryCompletion ();
			//entry.SetModel (CreateCompletionModel ());
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

