using Mono.Addins;
using Mono.Addins.Description;

[assembly: AddinName("GtkSharp Addin")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Provides modern Gtk project and file templates, as well as glade file integration.")]
[assembly: AddinAuthor("GtkSharp Contributors")]
[assembly: AddinUrl("https://github.com/GtkSharp/GtkSharp")]

[assembly: AddinDependency("MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("MonoDevelop.DotNetCore", MonoDevelop.BuildInfo.Version)]