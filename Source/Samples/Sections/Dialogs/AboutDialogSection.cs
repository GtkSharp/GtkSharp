// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using Gtk;

namespace Samples
{

	[Section (ContentType = typeof(AboutDialog), Category = Category.Dialogs)]
	class AboutDialogSection : ListSection
	{
		public AboutDialogSection ()
		{
			AddItem (CreateAboutDialog ());
		}

		public (string, Widget) CreateAboutDialog ()
		{
			var btn = new Button ("Show");
			btn.Clicked += (sender, e) => {
				var dlg = new AboutDialog();
				dlg.Shown += (s1, e1) => ApplicationOutput.WriteLine (sender, "Shown");
				var result = dlg.Run ();
				ApplicationOutput.WriteLine (sender, $"Run result: {result}");
				dlg.Dispose ();
			};
			return ("Dialog:", btn);
		}

	}

}