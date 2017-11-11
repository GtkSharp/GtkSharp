using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin("MonoDevelop.GtkSharp.Addin", Version="1.0")]

[assembly: AddinName("Gtk# Addin")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Provides modern Gtk# file / project templates and glade file integration.")]
[assembly: AddinAuthor("cra0zy")]
[assembly: AddinUrl("https://github.com/GtkSharp/GtkSharp")]

[assembly: AddinDependency("MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("MonoDevelop.DotNetCore", MonoDevelop.BuildInfo.Version)]