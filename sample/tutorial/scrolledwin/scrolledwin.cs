// scrolledwin.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class scrolledwin
	{

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void quitbutton_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		public static void Main(string[] args)
		{


			string buffer;
			uint i, j;
			    
			Application.Init();
			    
			/* Create a new dialog window for the scrolled window to be
			 * packed into.  */
			Dialog window = new Dialog();
			window.Title = "GtkScrolledWindow example";
			window.DeleteEvent += new DeleteEventHandler (delete_event);
			
			window.BorderWidth = 0;
			window.SetSizeRequest(300, 300);
			    
			/* create a new scrolled window. */
			ScrolledWindow scrolled_window = new ScrolledWindow (null, null);
			    
			scrolled_window.BorderWidth= 10;
			    
			/* the policy is one of GTK_POLICY AUTOMATIC, or GTK_POLICY_ALWAYS.
			 * GTK_POLICY_AUTOMATIC will automatically decide whether you need
			 * scrollbars, whereas GTK_POLICY_ALWAYS will always leave the scrollbars
			 * there.  The first one is the horizontal scrollbar, the second, 
			 * the vertical. */

			scrolled_window.SetPolicy (PolicyType.Automatic, PolicyType.Always);

			/* The dialog window is created with a vbox packed into it. */	

			window.VBox.PackStart(scrolled_window, true, true, 0);
			scrolled_window.Show();
  
			/* create a table of 10 by 10 squares. */
			Table table = new Table(10, 10, false);
			    
			/* set the spacing to 10 on x and 10 on y */
			table.RowSpacings = 10;
			table.ColSpacings = 10;
			
			   
			/* pack the table into the scrolled window */
			scrolled_window.AddWithViewport(table);
			table.Show();
			    
			/* this simply creates a grid of toggle buttons on the table
			 * to demonstrate the scrolled window. */
			
			for (i = 0; i < 10; i++)
				for (j = 0; j < 10; j++) {
				buffer = "button (" + i + "," + j + ")\n";
				ToggleButton button = new ToggleButton (buffer);
				table.Attach(button,  i, i+1, j, j+1);
				button.Show();
			       }
			   
			/* Add a "close" button to the bottom of the dialog */
			Button button = new Button("close");
			button.Clicked += new EventHandler (quitbutton_event);
			    
			/* this makes it so the button is the default. */
			    
			button.CanDefault = true;
			window.ActionArea.PackStart(button, true, true, 0);
			    
			/* This grabs this button to be the default button. Simply hitting
			 * the "Enter" key will cause this button to activate. */
			button.GrabDefault();
			button.Show();
			    
			window.Show();
			    
			Application.Run();    
		}
	}
}