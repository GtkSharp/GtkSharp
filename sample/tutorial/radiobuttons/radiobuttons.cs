// radiobuttons.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {


	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;


	public class radiobuttons
	{

		static GLib.SList group = null;

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void exitbutton_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		public static void Main(string[] args)
		{

			Application.Init();   
      
   
			Window window = new Window("radio buttons");
			  
			window.DeleteEvent += new DeleteEventHandler (delete_event);
			
			window.BorderWidth = 0;
			
			VBox box1 = new VBox (false, 0);
			window.Add(box1);
			box1.Show();
			
			VBox box2 = new VBox (false, 10);
			box2.BorderWidth = 10;
			box1.PackStart(box2, true, true, 0);
			box2.Show();
			
			RadioButton radiobutton = new RadioButton  (null, "button1");
			box2.PackStart(radiobutton, true, true, 0);
			radiobutton.Show();
			group = radiobutton.Group;
			RadioButton radiobutton2 = new RadioButton(group, "button2");
			radiobutton2.Active = true;
			box2.PackStart(radiobutton2, true, true, 0);
			radiobutton2.Show();
			
			RadioButton  radiobutton3 = RadioButton.NewWithLabelFromWidget(radiobutton, "button3");
			box2.PackStart(radiobutton3, true, true, 0);
			radiobutton3.Show();
			
			HSeparator separator = new HSeparator ();
			box1.PackStart (separator,false, true, 0);
			separator.Show();
			
			VBox box3 = new VBox(false, 10);
			box3.BorderWidth = 10;
			box1.PackStart(box3,false, true, 0);
			box3.Show();
			
			Button button = new Button ("close");
			button.Clicked += new EventHandler (exitbutton_event);
			
			box3.PackStart(button, true, true, 0);
			button.CanDefault = true;
			button.GrabDefault();
			button.Show();
			  
			window.ShowAll();
			     
			Application.Run();
			
		}
	}
}