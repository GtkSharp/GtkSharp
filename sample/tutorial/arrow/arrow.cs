// arrow.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class arrow
	{
	
		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}
	
		/* Create an Arrow widget with the specified parameters
		 * and pack it into a button */
	
		static Widget create_arrow_button(ArrowType arrow_type, ShadowType  shadow_type )
		{
	 
			Button button = new Button ();
			Arrow  arrow = new Arrow (arrow_type, shadow_type);
	 
			button.Add(arrow);
	
			button.Show();
			arrow.Show();
	
			return button;
		}
		
		public static void Main(string[] args)
		{
	
			/* Initialize the toolkit */
			 Application.Init ();

			/* Create a new window */
			Window window = new Window ("Arrow Buttons");

			/* It's a good idea to do this for all windows. */

			window.DeleteEvent += new DeleteEventHandler (delete_event);

			/* Sets the border width of the window. */
			window.BorderWidth = 10;

			/* Create a box to hold the arrows/buttons */
			HBox box = new HBox (false, 0);
			box.BorderWidth = 2;
			window.Add(box);

			/* Pack and show all our widgets */
			box.Show();

			Widget button1 = create_arrow_button(ArrowType.Up, ShadowType.In);
			box.PackStart (button1, false, false, 3);
  
			Widget button2 = create_arrow_button(ArrowType.Down, ShadowType.Out);
			box.PackStart (button2, false, false, 3);

			Widget button3 = create_arrow_button(ArrowType.Left, ShadowType.EtchedIn);
			box.PackStart (button3, false, false, 3);
	
			Widget button4 = create_arrow_button(ArrowType.Right, ShadowType.EtchedOut);
			box.PackStart (button4, false, false, 3);
	
	
			window.ShowAll ();
	  
			/* Rest in Application.Run() and wait for the fun to begin! */
			Application.Run();
		}
	
	}
}