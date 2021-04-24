// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Atk;
using Gdk;
using Gtk;

namespace Samples
{
	[Section(ContentType = typeof(ImageBox), Category = Category.Widgets)]
	class ImageSection : ListSection
	{
		public ImageSection()
		{
			AddItem(CreateContainer());
		}

		public (string, Widget) CreateContainer()
		{
			var image = new Pixbuf(typeof(ImageSection).Assembly, "Testpic");
			var container = new ImageBox(image);

			return ($"{nameof(ImageBox)}:", container);
		}
	}
}