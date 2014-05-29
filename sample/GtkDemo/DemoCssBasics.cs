/* CSS Theming/CSS Basics
 *
 * Gtk themes are written using CSS. Every widget is build of multiple items
 * that you can style very similarly to a regular website.
 *
 */

using System;
using System.IO;
using System.Reflection;
using Gtk;

namespace GtkDemo
{
	[Demo ("CSS Basics", "DemoCssBasics.cs", "CSS Theming")]
	public class DemoCssBasics : Window
	{
		TextBuffer buffer;
		CssProvider provider;
		CssProvider provider_reset;

		public DemoCssBasics () : base ("CSS Basics")
		{
			SetDefaultSize (600, 500);

			buffer = new TextBuffer (null);

			var warning = new TextTag ("warning");
			warning.Underline = Pango.Underline.Single;
			buffer.TagTable.Add (warning);

			var error = new TextTag ("error");
			error.Underline = Pango.Underline.Error;
			buffer.TagTable.Add (error);

			provider = new CssProvider ();
			provider_reset = new CssProvider ();

			var container = new ScrolledWindow ();
			Add (container);
			var view = new TextView (buffer);
			container.Add (view);
			buffer.Changed += OnCssTextChanged;

			using (Stream stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("reset.css"))
			using (StreamReader reader = new StreamReader(stream))
			{
				provider_reset.LoadFromData (reader.ReadToEnd());
			}

			using (Stream stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("css_basics.css"))
			using (StreamReader reader = new StreamReader(stream))
			{
				buffer.Text = reader.ReadToEnd();
			}

			// TODO: Connect to "parsing-error" signal in CssProvider, added in GTK+ 3.2

			ApplyCss (this, provider_reset, 800);
			ApplyCss (this, provider, UInt32.MaxValue);

			ShowAll ();
		}

		private void ApplyCss (Widget widget, CssProvider provider, uint priority)
		{
			widget.StyleContext.AddProvider (provider, priority);
			var container = widget as Container;
			if (container != null) {
				foreach (var child in container.Children) {
					ApplyCss (child, provider, priority);
				}
			}
		}

		private void OnCssTextChanged (object sender, EventArgs e)
		{
			var start = buffer.StartIter;
			var end = buffer.EndIter;
			buffer.RemoveAllTags (start, end);

			string text = buffer.Text;
			try {
				provider.LoadFromData (text);
			} catch (GLib.GException) {
				// Ignore parsing errors
			}

			StyleContext.ResetWidgets (Gdk.Screen.Default);
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}

