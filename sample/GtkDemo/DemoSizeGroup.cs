/* Size Groups
 *
 * SizeGroup provides a mechanism for grouping a number of
 * widgets together so they all request the same amount of space.
 * This is typically useful when you want a column of widgets to
 * have the same size, but you can't use a Table widget.
 *
 * Note that size groups only affect the amount of space requested,
 * not the size that the widgets finally receive. If you want the
 * widgets in a SizeGroup to actually be the same size, you need
 * to pack them in such a way that they get the size they request
 * and not more. For example, if you are packing your widgets
 * into a table, you would not include the Gtk.Fill flag.
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Size Group", "DemoSizeGroup.cs")]
	public class DemoSizeGroup : Dialog
	{
		private SizeGroup sizeGroup;

		static string [] colors = { "Red", "Green", "Blue" };
		static string [] dashes = { "Solid", "Dashed", "Dotted" };
		static string [] ends = { "Square", "Round", "Arrow" };

		public DemoSizeGroup () : base ("SizeGroup", null, DialogFlags.DestroyWithParent,
						Gtk.Stock.Close, Gtk.ResponseType.Close)
		{
			Resizable = false;

 			VBox vbox = new VBox (false, 5);
 			this.VBox.PackStart (vbox, true, true, 0);
 			vbox.BorderWidth = 5;

 			sizeGroup = new SizeGroup (SizeGroupMode.Horizontal);

			// Create one frame holding color options
 			Frame frame = new Frame ("Color Options");
 			vbox.PackStart (frame, true, true, 0);

 			Table table = new Table (2, 2, false);
 			table.BorderWidth = 5;
 			table.RowSpacing = 5;
 			table.ColumnSpacing = 10;
 			frame.Add (table);

 			AddRow (table, 0, sizeGroup, "_Foreground", colors);
 			AddRow (table, 1, sizeGroup, "_Background", colors);

			// And another frame holding line style options
			frame = new Frame ("Line Options");
			vbox.PackStart (frame, false, false, 0);

			table = new Table (2, 2, false);
			table.BorderWidth = 5;
			table.RowSpacing = 5;
			table.ColumnSpacing = 10;
			frame.Add (table);

 			AddRow (table, 0, sizeGroup, "_Dashing", dashes);
 			AddRow (table, 1, sizeGroup, "_Line ends", ends);

			// And a check button to turn grouping on and off
  			CheckButton checkButton = new CheckButton ("_Enable grouping");
  			vbox.PackStart (checkButton, false, false, 0);
  			checkButton.Active = true;
 			checkButton.Toggled += new EventHandler (ToggleGrouping);

			ShowAll ();
		}

		// Convenience function to create a combo box holding a number of strings
		private ComboBox CreateComboBox (string [] strings)
		{
			ComboBox combo = ComboBox.NewText ();

			foreach (string str in strings)
				combo.AppendText (str);

			combo.Active = 0;
			return combo;
		}

 		private void AddRow (Table table, uint row, SizeGroup sizeGroup, string labelText, string [] options)
 		{
 			Label label = new Label (labelText);
 			label.SetAlignment (0, 1);

			table.Attach (label,
				      0, 1, row, row + 1,
				      AttachOptions.Expand | AttachOptions.Fill, 0,
				      0, 0);

			ComboBox combo = CreateComboBox (options);
			label.MnemonicWidget = combo;

			sizeGroup.AddWidget (combo);
			table.Attach (combo,
				      1, 2, row, row + 1,
				      0, 0,
				      0, 0);
		}

 		private void ToggleGrouping (object o, EventArgs args)
 		{
			ToggleButton checkButton = (ToggleButton)o;

			// SizeGroupMode.None is not generally useful, but is useful
			// here to show the effect of SizeGroupMode.Horizontal by
			// contrast
 			SizeGroupMode mode;

 			if (checkButton.Active)
 				mode = SizeGroupMode.Horizontal;
 			else
 				mode = SizeGroupMode.None;

 			sizeGroup.Mode = mode;
 		}

		protected override void OnResponse (Gtk.ResponseType responseId)
		{
			Destroy ();
		}
	}
}
