// base.cs - Gtk# Tutorial example
//
// Author: Johannes Roith <johannes@jroith.de>
//
// (c) 2002 Johannes Roith

namespace GtkSharpTutorial {

	using Gtk;
	using GtkSharp;
	using System;


	public class baseclass {

		public static void Main(string[] args)
		{
   
			Application.Init ();
    
			Window window = new Window ("base");
			window.Show();
    
			Application.Run ();
    
 
		}

	}

}