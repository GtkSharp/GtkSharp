using System;
using System.Collections.Generic;
using Gtk;
using GettingStarted.Pages;
using UI = Gtk.Builder.ObjectAttribute;

namespace GettingStarted
{
    /// <summary>
    /// The shell: a stack of pages, one per section of getting-started.md,
    /// built from MainWindow.ui through GtkBuilder.
    /// </summary>
    public class MainWindow : Window
    {
        // Bound by name off the .ui file. The field name IS the id -- BindFields
        // matches them exactly unless [UI("some-id")] says otherwise.
        [UI] private readonly Stack _stack = null;
        [UI] private readonly Label _status = null;

        public MainWindow() : this(new Builder("MainWindow.ui")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            // Binds the [UI] fields above. It would also connect <signal>
            // elements, but the document declares none -- see MainWindow.ui.
            builder.Autoconnect(this);

            BuildHeaderBar();
            AddPages();
            AddShortcuts();

            // delete-event is gone. CloseRequest's RetVal INVERTS the old
            // convention: true vetoes the close, so allowing it means false.
            CloseRequest += (o, args) =>
            {
                Application.Quit();
                args.RetVal = false;
            };
        }

        /// <summary>Pages call this to report what just happened.</summary>
        public void Report(string message) => _status.Text = message;

        private void BuildHeaderBar()
        {
            var header = new HeaderBar { ShowTitleButtons = true };

            // A menu in Gtk 4 is a MODEL, not a widget tree: GMenu describes it,
            // and the MenuButton renders it into a popover. Items name actions
            // rather than carrying callbacks.
            var menu = new GLib.Menu();
            menu.Append("About", "app.about");
            menu.Append("Quit", "app.quit");

            var menuButton = new MenuButton
            {
                IconName = "open-menu-symbolic",
                MenuModel = menu,
                TooltipText = "Application menu (app.* actions)"
            };
            header.PackEnd(menuButton);

            Titlebar = header;
        }

        private void AddPages()
        {
            var pages = new List<TourPage>
            {
                new LayoutPage(),
                new SignalsPage(),
                new BuilderPage(),
                new ControllersPage(),
                new ActionsPage(),
                new ListsPage(),
                new DrawingPage(),
                new CssPage(),
                new TextPage(),
                new TrapsPage()
            };

            foreach (var page in pages)
            {
                page.Report = Report;
                // AddTitled gives the StackSidebar the label it shows; the name
                // is the identity the stack itself uses.
                _stack.AddTitled(page, page.GetType().Name, page.Title);
            }
        }

        private void AddShortcuts()
        {
            // AccelGroup and gtk_widget_add_accelerator are gone; shortcuts are
            // a controller like every other input source in Gtk 4.
            var shortcuts = new ShortcutController();
            shortcuts.AddShortcut(new Shortcut(new ShortcutTrigger("<Control>q"),
                                               new ShortcutAction("action(app.quit)")));
            AddController(shortcuts);
        }
    }
}
