// label.cs - Gtk# Tutorial example
//
// Author: Alejandro Sánchez Acosta <raciel@es.gnu.org>
// 	   Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//
// (c) 2002 Alejandro Sánchez Acosta

namespace GtkSharpTutorial {

	using Gtk;
	using GtkSharp;
	using System;
	using System.Drawing;

	public class label
	{

		static void delete_event (object obj, DeleteEventArgs args)
		{
			Application.Quit();
		}

		static void exitbutton_event (object obj, EventArgs args)
		{
			Application.Quit();
		}

		public static void Main (string[] args)
		{
		
			Widget widget;
			Window window;
			HBox hbox;
			VBox vbox;
			Frame frame;
			Label label;
			
			Application.Init ();   
      
   
			window = new Window ("Label sample");
			  
			window.DeleteEvent += new DeleteEventHandler (delete_event);

			window.Title = "Label";
			
			hbox = new HBox (false, 5);
			vbox = new VBox (false, 5);

			window.Add (hbox);
			hbox.PackStart (vbox, false, false, 0);

			window.BorderWidth = 5;

			frame = new Frame ("Normal Label");
			label = new Label ("This is a normal label");
			
			frame.Add (label);
			vbox.PackStart (frame, false, false, 0);
			
		        frame = new Frame ("Multiline Label");
			label = new Label ("This is a Multi-line label\nSecond Line\nThird Line");
	
			frame.Add (label);
			vbox.PackStart (frame, false, false, 0);
			
			frame = new Frame ("Left Justified Label");
			label = new Label ("This is a Left-Justified\n" + "Multi-line label.\n" + "Third      line");
			label.Justify = Justification.Left;

			frame.Add (label);
			vbox.PackStart (frame, false, false, 0);
	
			frame = new Frame ("Right Justified Label");			
			label = new Label ("This is a Right Justified\nMulti-line label.\n" + "Fourth Line, (j/k)");
			label.Justify = Justification.Right;
			frame.Add (label);
			vbox.PackStart (frame, false, false, 0);

			vbox = new VBox (false, 5);
			hbox.PackStart(vbox, false, false, 0);
			
			frame = new Frame ("Line wrapped Label");
			label = new Label ("This is an example of a line-wrapped label.  It " +
			 "should not be taking up the entire             " /* big space to test spacing */ +
			 "width allocated to it, but automatically " +
			 "wraps the words to fit.  " +
			 "The time has come, for all good men, to come to " +
			 "the aid of their party.  " +
			 "The sixth sheik's six sheep's sick.\n" +
			 "     It supports multiple paragraphs correctly, " +
			 "and  correctly   adds " +
			 "many          extra  spaces. ");
			
			label.LineWrap = true;
		
			frame.Add (label);
			vbox.PackStart(frame, false, false, 0);
			
			frame = new Frame ("Filled wrapped label");
			label = new Label ("This is an example of a line-wrapped, filled label.  " +
			 "It should be taking " +
			 "up the entire              width allocated to it.  " +
			 "Here is a sentence to prove " +
			 "my point.  Here is another sentence. " +
			 "Here comes the sun, do de do de do.\n" +
			 "    This is a new paragraph.\n" +
			 "    This is another newer, longer, better " +
			 "paragraph.  It is coming to an end, "+
					   "unfortunately.");
			
			label.Justify = Justification.Fill;
			label.Wrap = true;
			frame.Add (label);
			vbox.PackStart (frame, false, false, 0);


			frame = new Frame ("Underlined label");
			
			label = new Label ("This label is underlined!\n" + "This one is underlined in quite a funky fastion");

			label.Justify = Justification.Left;
			label.Pattern = "_________________________ _ _________ _ ______     __ _______ ___";

			frame.Add (label);
			vbox.PackStart (frame, false, false, 0);

			window.ShowAll ();

			Application.Run ();
		}
	}
}
