using System;
using Gtk;
using Gnome;

class PrintSample
{
	TextView tv;
	
	static void Main ()
	{
		new PrintSample ();
	}
	
	PrintSample ()
	{
		Application.Init ();
		Gtk.Window win = new Gtk.Window ("Print sample");
		win.SetDefaultSize (400, 300);
		win.DeleteEvent += new DeleteEventHandler (OnWinDelete);
		
		VBox vbox = new VBox (false, 0);
		win.Add (vbox);
		
		tv = new TextView ();
		tv.Buffer.Text = "Hello World";
		vbox.PackStart (tv, true, true, 0);

		Button print = new Button (Gtk.Stock.Print);
		print.Clicked += new EventHandler (OnPrintClicked);
		vbox.PackStart (print, false, true, 0);	
		
		win.ShowAll ();
		Application.Run ();
	}
	
	void MyPrint (Gnome.PrintContext gpc)
	{
		gpc.BeginPage ("demo");
		gpc.MoveTo (1, 700);
		gpc.Show (tv.Buffer.Text);
		gpc.ShowPage ();
	}
	
	void OnPrintClicked (object o, EventArgs args)
	{
		Gnome.PrintJob pj = new Gnome.PrintJob (Gnome.PrintConfig.Default ());
		Gnome.PrintDialog dialog = new Gnome.PrintDialog (pj, "Print Test", 0);
		int response = dialog.Run ();
		Console.WriteLine ("response: " + response);
		
		if (response == (int) PrintButtons.Cancel) {
			Console.WriteLine ("Canceled");
			dialog.Hide ();
			dialog.Dispose ();
			return;
		}

		Gnome.PrintContext ctx = pj.Context;
		MyPrint (ctx); 

		pj.Close ();
		
		switch (response) {
		case (int) PrintButtons.Print: 
			pj.Print (); 
			break;
		case (int) PrintButtons.Preview:
			new PrintJobPreview (pj, "Print Test").Show ();
			break;
		}

		dialog.Hide ();
		dialog.Dispose ();
	}
	
	void OnWinDelete (object o, DeleteEventArgs args)
	{
		Application.Quit ();
	}
}
