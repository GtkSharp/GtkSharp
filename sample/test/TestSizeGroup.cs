//
// TestSizeGroup.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
//

using System;

using Gtk;
using GtkSharp;

namespace WidgetViewer {
	public class TestSizeGroup {

		static Dialog window = null;
		static SizeGroup size_group = null;

		public static Gtk.Window Create ()
		{
			window = new Dialog ();
			window.Title = "Sized groups";
			window.Resizable = false;
			
			VBox vbox = new VBox (false, 5);
			window.VBox.PackStart (vbox, true, true, 0);
			vbox.BorderWidth = 5;

			size_group = new SizeGroup (SizeGroupMode.Horizontal);

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

			Add_Row (table, 0, size_group, "_Foreground", colors);
			Add_Row (table, 1, size_group, "_Background", colors);

			frame = new Frame ("Line Options");
			vbox.PackStart (frame, false, false, 0);

			table = new Table (2, 2, false);
			table.BorderWidth = 5;
			table.RowSpacing = 5;
			table.ColSpacings = 10;
			frame.Add (table);

			Add_Row (table, 0, size_group, "_Dashing", dashes);
			Add_Row (table, 1, size_group, "_Line ends", ends);

			CheckButton check_button = new CheckButton ("_Enable grouping");
			vbox.PackStart (check_button, false, false, 0);
			check_button.Active = true;
			check_button.Toggled += new EventHandler (Button_Toggle_Cb);

			Button close_button = new Button ("_Close");
			close_button.Clicked += new EventHandler (Close_Button);
			window.ActionArea.PackStart (close_button, false, false, 0);
			
			window.ShowAll ();
			return window;
		}

		static OptionMenu Create_OptionMenu (string [] strings)
		{
			Menu menu = new Menu ();

			MenuItem menu_item = null;

			foreach (string str in strings) {
				menu_item = new MenuItem (str);
				menu_item.Show ();
				menu.Append (menu_item);
			}

			OptionMenu option_menu = new OptionMenu ();
			option_menu.Menu = menu;

			return option_menu;
		}

		static void Add_Row (Table table, uint row, SizeGroup size_group,
				     string label_text, string [] options)
		{
			Label label = Label.NewWithMnemonic (label_text);
			label.SetAlignment (0, 1);

			table.Attach (label,
				      0, 1, row, row + 1,
				      AttachOptions.Expand, AttachOptions.Fill,
				      0, 0);

			OptionMenu option_menu = Create_OptionMenu (options);

			size_group.AddWidget (option_menu);
			table.Attach (option_menu,
				      1, 2, row, row + 1,
				      AttachOptions.Expand, AttachOptions.Expand,
				      0, 0);
		}

		static void Button_Toggle_Cb (object o, EventArgs args)
		{
			Toggle_Grouping ((ToggleButton) o, size_group);
		}

		static void Toggle_Grouping (ToggleButton check_button,
					     SizeGroup size_group)
		{
			SizeGroupMode mode;

			if (check_button.Active)
				mode = SizeGroupMode.Horizontal;
			else
				mode = SizeGroupMode.None;

			size_group.Mode = mode;
		}

		static void Close_Button (object o, EventArgs args)
		{
			window.Destroy ();
		}
	}
}
