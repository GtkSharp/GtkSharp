/* Spinner
 *
 * GtkSpinner allows to show that background activity is on-going.
 *
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Spinner", "DemoSpinner.cs")]
	public class DemoSpinner : Dialog
	{
		Spinner spinner_sensitive;
		Spinner spinner_unsensitive;

		public DemoSpinner () : base ("Spinner", null, DialogFlags.DestroyWithParent)
		{
			Resizable = false;

			VBox vbox = new VBox (false, 5);
			vbox.BorderWidth = 5;
			ContentArea.PackStart (vbox, true, true, 0);

			/* Sensitive */
			HBox hbox = new HBox (false, 5);
			spinner_sensitive = new Spinner ();
			hbox.Add (spinner_sensitive);
			hbox.Add (new Entry ());
			vbox.Add (hbox);

			/* Disabled */
			hbox = new HBox (false, 5);
			spinner_unsensitive = new Spinner ();
			spinner_unsensitive.Sensitive = false;
			hbox.Add (spinner_unsensitive);
			hbox.Add (new Entry ());
			vbox.Add (hbox);

			Button btn_play = new Button ();
			btn_play.Label = "Play";
			btn_play.Clicked += OnPlayClicked;
			vbox.Add (btn_play);

			Button btn_stop = new Button ();
			btn_stop.Label = "Stop";
			btn_stop.Clicked += OnStopClicked;
			vbox.Add (btn_stop);

			AddButton (Stock.Close, ResponseType.Close);

			OnPlayClicked (null, null);

			ShowAll ();
			Run ();
			Destroy ();
		}

		private void OnPlayClicked (object sender, EventArgs e)
		{
			spinner_sensitive.Start ();
			spinner_unsensitive.Start ();
		}

		private void OnStopClicked (object sender, EventArgs e)
		{
			spinner_sensitive.Stop ();
			spinner_unsensitive.Stop ();
		}
	}
}

