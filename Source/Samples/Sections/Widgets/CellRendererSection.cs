using Gtk;
using System;
namespace Samples
{

	[Section (ContentType = typeof(CustomCellRenderer), Category = Category.Widgets)]
	class CellRendererSection : ListSection
	{
		public CellRendererSection ()
		{
			AddItem (CreateCellRenderer ());
         
		}
		ListStore liststore;

		public (string, Widget) CreateCellRenderer ()
		{
			liststore = new ListStore (typeof (float), typeof (string));
			liststore.AppendValues (0.5f, "50%");

			TreeView view = new TreeView (liststore);

			view.AppendColumn ("Progress", new CellRendererText (), "text", 1);
			view.AppendColumn ("Progress", new CustomCellRenderer (), "percent", 0);
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (update_percent));

			return (nameof(CustomCellRenderer), view);
		}
		
		bool increasing = true;
		bool update_percent ()
		{
			TreeIter iter;
			liststore.GetIterFirst (out iter);
			float perc = (float) liststore.GetValue (iter, 0);

			if ((perc > 0.99) || (perc < 0.01 && perc > 0)) {
				increasing = !increasing;
			}

			if (increasing)
				perc += 0.01f;
			else
				perc -= 0.01f;

			liststore.SetValue (iter, 0, perc);
			liststore.SetValue (iter, 1, Convert.ToInt32 (perc * 100) + "%");

			return true;
		}
	}

}