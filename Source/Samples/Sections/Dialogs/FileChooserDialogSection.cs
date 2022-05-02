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

		protected override void OnPressed ()
		{
			var fcd = new FileChooserDialog ("Open File", null, FileChooserAction.Open);
			fcd.AddButton (Stock.Cancel, ResponseType.Cancel);
			fcd.AddButton (Stock.Open, ResponseType.Ok);
			fcd.DefaultResponse = ResponseType.Ok;
			fcd.SelectMultiple = false;

			ResponseType response = (ResponseType) fcd.Run ();
			if (response == ResponseType.Ok)
			     ApplicationOutput.WriteLine (fcd.Filename);
			fcd.Destroy ();
		}
    }
}