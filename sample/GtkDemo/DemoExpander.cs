/* Expander
 *
 * GtkExpander allows to provide additional content that is initially hidden.
 * This is also known as "disclosure triangle".
 *
 */
using System;
using Gtk;

namespace GtkDemo 
{
	[Demo ("Expander", "DemoExpander.cs")]
	public class DemoExpander : Gtk.Dialog
	{
		public DemoExpander () : base ("Demo Expander", null, DialogFlags.DestroyWithParent)
		{
			this.BorderWidth = 10;
			this.VBox.PackStart (new Label ("Expander demo. Click on the triangle for details."), false, true, 3);

			Expander expander = new Expander ("Details");
			expander.Add (new Label ("Details can be shown or hidden."));
			this.VBox.PackStart (expander, false, true, 3);

			this.AddButton (Stock.Close, ResponseType.Close);

			this.ShowAll ();
			this.Run ();
			this.Hide ();
			this.Destroy ();
		}
	}
}

