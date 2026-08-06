using System;
using Gtk;
using Samples;
using Xunit;

// These exercise APIs Gtk 4.10 deprecated on purpose: they are still
// bound, so they still need to work.
#pragma warning disable CS0612, CS0618

namespace GtkSharp.Tests
{
    /// <summary>
    /// Builds the sample application's main window, which is where most of the
    /// Gtk 4 port landed: the header bar, the paned split, the notebook, the
    /// source view and the section tree.
    /// </summary>
    /// <remarks>
    /// The section tests construct sections in isolation; nothing else here
    /// assembles them into a window, wires a close handler, or drives
    /// <c>GtkSourceView</c> and <c>TreeStore</c> together. This closes that gap.
    /// </remarks>
    public class MainWindowTests : GtkTestBase
    {
        public MainWindowTests(GtkFixture gtk) : base(gtk) { }

        [Fact]
        public void MainWindow_constructs_with_its_Gtk4_layout()
        {
            Run(() =>
            {
                var window = new MainWindow();

                Assert.NotEqual(IntPtr.Zero, window.Handle);

                // Gtk 4 moved the title off GtkHeaderBar, which no longer has
                // one, onto the window itself.
                Assert.Equal("GtkSharp Sample Application", window.Title);

                // The header bar is installed as the titlebar, and the paned
                // split is the window's single child -- Gtk 4 windows take one.
                Assert.IsType<HeaderBar>(window.Titlebar);
                var paned = Assert.IsType<Paned>(window.Child);

                // Pack1/Pack2 became StartChild/EndChild.
                var treeView = Assert.IsType<TreeView>(paned.StartChild);
                Assert.NotNull(paned.EndChild);

                // FillUpTreeView ran: the section tree has a model with rows.
                Assert.NotNull(treeView.Model);
                Assert.True(treeView.Model.GetIterFirst(out TreeIter _),
                            "the section tree should have at least one row");
            });
        }

        [Fact]
        public void MainWindow_presents_and_becomes_visible()
        {
            // Presenting realises the widget tree, so a failure in layout or in
            // a draw function surfaces here rather than in whichever test runs
            // next -- GtkFixture drains the main loop after each body, which is
            // what gives that its chance to happen.
            //
            // Visible is asserted rather than Application.Windows, because
            // GApplication window tracking only works once the application has
            // registered, and registration needs a session bus that is not
            // present everywhere. See Program.EnsureApplication.
            Run(() =>
            {
                var window = new MainWindow();

                Assert.False(window.Visible);

                window.Present();

                Assert.True(window.Visible);
            });
        }

    }
}
#pragma warning restore CS0612, CS0618
