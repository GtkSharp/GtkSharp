/* CSS Theming/Style Classes
 *
 * GTK+ uses CSS for theming. Style classes can be associated
 * with widgets to inform the theme about intended rendering.
 *
 * This demo shows some common examples where theming features
 * of GTK+ are used for certain effects: primary toolbars,
 * inline toolbars and linked buttons.
 */

using System;
using Gtk;

namespace GtkDemo
{
	[Demo ("Style Classes", "DemoThemingStyleClasses.cs", "CSS Theming")]
	public class DemoThemingStyleClasses : Window
	{
		public DemoThemingStyleClasses () : base ("Style Classes")
		{
			BorderWidth = 12;
			var builder = new Builder ("theming.ui");

			var grid = (Widget)builder.GetObject ("grid");
			grid.ShowAll ();
			Add (grid);

			Show ();
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

