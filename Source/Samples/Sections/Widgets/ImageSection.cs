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
			Stream GetResourceStream(Assembly assembly, string name)
			{
				var resources = assembly.GetManifestResourceNames();
				var resourceName = resources.SingleOrDefault(str => str == name);

				// try harder:
				if (resourceName == default) {
					resourceName = resources.SingleOrDefault(str => str.EndsWith(name));
				}

				if (resourceName == default)
					return default;
				var stream = assembly.GetManifestResourceStream(resourceName);
				return stream;
			}
			Pixbuf image = default;
			using (var stream = GetResourceStream(typeof(ImageSection).Assembly, "Testpic.png")) {
				image = new Pixbuf(stream);
			}

			var container = new ImageBox(image);


			return ($"{nameof(ImageBox)}:", container);
		}
	}
}