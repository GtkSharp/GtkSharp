// checkbuttons.cs - GTK# Tutorial example
//
// Authors: Alejandro Sanchez Acosta <raciel@es.gnu.org>
//          Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//
// (C) 2002 Alejandro Sanchez Acosta <raciel@es.gnu.org>
//          Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>

namespace GtkSharpTutorial {


        using Gtk;
        using GtkSharp;
        using System;
        using System.Drawing;


	public class rangeWidgetsSamples
	{
		static HScale hscale;
		static VScale vscale;
		
		static void cb_pos_menu_select (object obj, PositionType pos)
		{
			/* Set the value position on both scale widgets */
			((Scale) hscale).ValuePos = pos;
			((Scale) vscale).ValuePos = pos;
		}

		
		static void cb_update_menu_select (object obj, UpdateType policy)
		{
			/* Set the update policy for both scale widgets */
			((Range) hscale).UpdatePolicy = policy;
			((Range) vscale).UpdatePolicy = policy;
		}


		static void cb_digits_scale (Adjustment adj)
		{
			/* Set the number of decimal places to which adj->value is rounded */
			
			/* FIXME: this might be wrong */
			// ((Scale) hscale) = adj.Value;
			// ((Scale) vscale) = adj.Value;
		}

		
		// FIXME
		static void cb_page_size (Adjustment get, Adjustment set)
		{
			/* Set the page size and page increment size of the sample
			 * adjustment to the value specified by the "Page Size" scale */
			//set.PageSize = get.Value;
			//set->page_increment = = get.Value;

			/* This sets the adjustment and makes it emit the "changed" signal to 
			   reconfigure all the widgets that are attached to this signal.  */
			//set.ClampPage (			
		}
		
		// FIXME
		static void cb_draw_value (ToggleButton button)
		{
			/* Turn the value display on the scale widgets off or on depending
			 *  on the state of the checkbutton */
			//((Scale) hscale).DrawValue  button.Active
			//((Scale) vscale).DrawValue  button.Active			
		}


		/* Convenience functions */

		// FIXME:
		static Widget make_menu_item (string name /*, callback , gpointer */)
		{
			Widget w = null;
			return w;
		}


		static void scale_set_default_values (Scale s)
		{
			s.UpdatePolicy = UpdateType.Continuous;
			s.Digits = 1;
			s.ValuePos = PositionType.Top;
			s.DrawValue = true;
		}

		static void create_range_controls ()
		{
			Window window;
			VBox box1, box3;
			Box box2;
			Button button;
			HScrollbar scrollbar;
			HSeparator separator;
			Widget item; /* FIXME: find out the exactly widgets */
			OptionMenu opt;
			Menu menu;
			Label label;
			Scale scale;
			Adjustment adj1, adj2;

			window = new Window (WindowType.Toplevel);
			// window.DeleteEvent += new DeleteEventHandler
			window.Title = "range controls";
			
			box1 = new VBox (false, 0);
			window.Add (box1);
			box1.ShowAll ();

			box2 = new HBox (false, 0);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);
			box2.ShowAll ();

			/* value, lower, upper, step_increment, page_increment, page_size */
			/* Note that the page_size value only makes a difference for
			 * scrollbar widgets, and the highest value you'll get is actually
			 * (upper - page_size). */
			adj1 = new Adjustment (0.0, 0.0, 101.0, 0.1, 1.0, 1.0);
			
			vscale = new VScale ((Adjustment) adj1);
			scale_set_default_values (vscale);
			
			box2.PackStart (vscale, true, true, 0);
			vscale.ShowAll ();

			box3 = new VBox (false, 10);
			box2.PackStart (box3, true, true, 0);
			box3.ShowAll ();

			/* Reuse the same adjustment */
			hscale = new HScale ((Adjustment) adj1);
			hscale.SetSizeRequest (200, -1);
			scale_set_default_values (hscale);

			box3.PackStart (hscale, true, true, 0);
			hscale.ShowAll ();

			/* reuse the same adjustment again */
			scrollbar = new HScrollbar ((Adjustment) adj1);

			/* Notice how this causes the scales to always be updated
			 * continuously when the scrollbar is moved */
			scrollbar.UpdatePolicy = UpdateType.Continuous;

			box3.PackStart (scrollbar, true, true, 0);
			scrollbar.ShowAll ();

			box2 = new HBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, true, true, 0);
			box2.ShowAll ();

			/* A checkbutton to control whether the value is displayed or not */
			button = new Button ("Display value on scale widgets");
			//FIXME
                        //((ToggleButton) button).Active = true;
			// FIXME: find out the handler signature
			//((ToggleButton) button).Toggled += 
			box2.PackStart (button, true, true, 0);
			button.ShowAll ();
			
			box2 = new HBox (false, 10);
			box2.BorderWidth = 10;

			/* An option menu to change the position of the value */
			label = new Label ("Scale Value Position:");
			box2.PackStart (label, false, false, 0);
			label.ShowAll ();

			opt = new OptionMenu ();
			menu = new Menu ();

			//FIXME:
			item = new MenuItem ();
			menu.Append (item);

			// item =
			menu.Append (item);
			
			// item = 
			menu.Append (item);
			
			// item = 
			menu.Append (item);

			((OptionMenu) opt).Menu = menu;
			box2.PackStart (opt, true, true, 0);
			opt.ShowAll ();

			box1.PackStart (box2, true, true, 0);
			box2.ShowAll ();
			
			box2 = new HBox (false, 10);
			box2.BorderWidth = 10;

			/* Yet another option menu, this time for the update policy of the
			 * scale widgets */
			label = new Label ("Scale Update Policy:");
			box2.PackStart (label, false, false, 0);
			label.ShowAll ();

			opt = new OptionMenu ();
			menu = new Menu ();

			// FIXME: continuous
			item = new MenuItem  ();
			menu.Append (item);

			// FIXME: discontinuous
			item = new MenuItem ();
			menu.Append (item);

			//FIXME: delayed
			item = new MenuItem ();
			menu.Append (item);

			opt.Menu = menu;
			
			box2.PackStart (opt, true, true, 0);
			opt.ShowAll ();

			box1.PackStart (box2, true, true, 0);
			box2.ShowAll ();

			box2 = new HBox (false, 10);
			box2.BorderWidth = 10;

			/* An HScale widget for adjusting the number of digits on the
			 * sample scales. */
			label = new Label ("Scale Digits:");
			box2.PackStart (label, false, false, 0);
			label.ShowAll ();

			adj2 = new Adjustment (1.0, 0.0, 5.0, 1.0, 1.0, 0.0);
			//FIXME: add a value_changed signal handler
			scale = new HScale (adj2);
			scale.Digits = 0;
			
			box2.PackStart (scale, true, true, 0);
			scale.ShowAll ();
			
			box1.PackStart (box2, true, true, 0);
			box2.ShowAll ();

			box2 = new HBox (false, 10);
			box2.BorderWidth = 10;

			/* And, one last HScale widget for adjusting the page size of the
			 * scrollbar. */
			label = new Label ("Scrollbar Page Size:");
			box2.PackStart (label, false, false, 0);
			label.ShowAll ();

			adj2 = new Adjustment (1.0, 1.0, 101.0, 1.0, 1.0, 0.0);
			// FIXME: write a value_changed signal handler
			scale = new HScale (adj2);
			scale.Digits = 0;
			box2.PackStart (scale, true, true, 0);
			scale.ShowAll ();

			box1.PackStart (box2, true, true, 0);
			box2.ShowAll ();

			separator = new HSeparator ();
			box1.PackStart (separator, false, true, 0);
			separator.ShowAll ();
			
			box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart (box2, false, true, 0);
			box2.ShowAll ();

			button = new Button ("Quit");
			// FIXME: write a clicked signal handler
			box2.PackStart (button, true, true, 0);
			//FIXME: set widget flags
			//FIXME: grab default
			button.ShowAll ();

			window.ShowAll ();			
		}
		
		
		public static void Main (string [] args)
		{
			Application.Init ();
			create_range_controls ();
			Application.Run ();
		}
	}
}
