using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(FileChooserDialog), Category = Category.Dialogs)]
    class FileChooserDialogSection : ListSection
	{
		public FileChooserDialogSection ()
		{
			AddItem ($"Press button to open {nameof(FileChooserDialog)} :", new FileChooserDialogDemo ("Press me"));
		}
	}

	class FileChooserDialogDemo : Button
	{
		public FileChooserDialogDemo (string text) : base (text) { }

		protected override void OnClicked ()
		{
			var fcd = new FileChooserDialog ("Open File", null, FileChooserAction.Open);
			// Gtk 4 removed the stock item registry: button labels are plain
			// strings, with the underscore marking the mnemonic.
			fcd.AddButton ("_Cancel", ResponseType.Cancel);
			fcd.AddButton ("_Open", ResponseType.Ok);
			fcd.DefaultResponse = ResponseType.Ok;
			fcd.SelectMultiple = false;

			// gtk_dialog_run is gone -- it spun a nested main loop, which Gtk 4
			// does not allow. The result arrives on the Response signal instead,
			// so everything after the dialog opens has to move into the handler.
			fcd.Response += (o, args) => {
				if (args.ResponseId == (int) ResponseType.Ok) {
					// The chooser answers with a GFile now, not a bare path;
					// a file need not be local, so Path can be null.
					var file = fcd.File;
					ApplicationOutput.WriteLine (file?.Path ?? file?.Uri?.ToString () ?? "(no file)");
				}

				fcd.Destroy ();
			};

			fcd.Present ();
		}
    }
}