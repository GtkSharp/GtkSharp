//
// DemoSizeGroup.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

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
	public class DemoSizeGroup
	{
		private Dialog window;
		private SizeGroup sizeGroup;

		public DemoSizeGroup ()
		{
			window = new Dialog ();
			window.Title = "Sized groups";
			window.Resizable = false;
			
 			VBox vbox = new VBox (false, 5);
 			window.VBox.PackStart (vbox, true, true, 0);
 			vbox.BorderWidth = 5;

 			sizeGroup = new SizeGroup (SizeGroupMode.Horizontal);

			//Create one frame holding color options
 			Frame frame = new Frame ("Color Options");
 			vbox.PackStart (frame, true, true, 0);

 			Table table = new Table (2, 2, false);
 			table.BorderWidth = 5;
 			table.RowSpacing = 5;
 			table.ColumnSpacing = 10;
 			frame.Add (table);

 			string [] colors = {"Red", "Green", "Blue", };
 			string [] dashes = {"Solid", "Dashed", "Dotted", };
 			string [] ends = {"Square", "Round", "Arrow", };

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
 			checkButton.Toggled += new EventHandler (ButtonToggleCb);

			Button CloseButton = new Button (Stock.Close);
			window.AddActionWidget  (CloseButton, 5);
			window.Response += new ResponseHandler (ResponseCallback);
		
			window.ShowAll ();
		}

		// Convenience function to create an option menu holding a number of strings
		private OptionMenu CreateOptionMenu (string [] strings)
		{
			Menu menu = new Menu ();
			MenuItem menuItem;

			foreach (string str in strings)
			{
				menuItem = new MenuItem (str);
				menuItem.Show ();
				menu.Append (menuItem);
			}

			OptionMenu optionMenu = new OptionMenu ();
			optionMenu.Menu = menu;

			return optionMenu;
		}

 		private void AddRow (Table table, uint row, SizeGroup sizeGroup, string labelText, string [] options)
 		{
 			Label label = new Label (labelText);
 			label.SetAlignment (0, 1);

			table.Attach (label,
				      0, 1, row, row + 1,
				      AttachOptions.Expand, AttachOptions.Fill,
				      0, 0);

			OptionMenu optionMenu = CreateOptionMenu (options);

			sizeGroup.AddWidget (optionMenu);
			table.Attach (optionMenu,
				      1, 2, row, row + 1,
				      AttachOptions.Expand, AttachOptions.Expand,
				      0, 0);
		}

 		private void ButtonToggleCb (object o, EventArgs args)
 		{
 			ToggleGrouping ((ToggleButton) o, sizeGroup);
 		}

		// SizeGroupMode.None is not generally useful, but is useful
		// here to show the effect of SizeGroupMode.Horizontal by 
		// contrast

 		private void ToggleGrouping (ToggleButton checkButton, SizeGroup sizeGroup)
 		{
 			SizeGroupMode mode;

 			if (checkButton.Active)
 				mode = SizeGroupMode.Horizontal;
 			else
 				mode = SizeGroupMode.None;

 			sizeGroup.Mode = mode;
 		}

		private void ResponseCallback (object obj, ResponseArgs args)
        
               {
		       if (args.ResponseId == 5) {
			       window.Hide ();
			       window.Destroy ();}
	       }
	}
}
