using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Samples;
using Xunit;

// The sample's own tree is a GtkTreeView, deprecated in Gtk 4.10 but still what
// MainWindow uses.
#pragma warning disable CS0612, CS0618

namespace GtkSharp.Tests
{
    /// <summary>
    /// Selects every entry in the sample's section tree, the way someone
    /// clicking through the application would.
    /// </summary>
    /// <remarks>
    /// <see cref="SampleSectionTests"/> constructs each section in isolation.
    /// That misses everything the application does around it: resolving the
    /// row's label back to a type, instantiating it lazily on first selection,
    /// clearing the previous section out of the content pane, mounting the new
    /// one, and loading the section's own source into the code view. This drives
    /// that path, which is the one a manual tester actually exercises.
    /// </remarks>
    public class SectionBrowsingTests : GtkTestBase
    {
        public SectionBrowsingTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Every row the tree offers, with the category it sits under.</summary>
        public static IEnumerable<object[]> SectionRows()
        {
            return typeof(SectionAttribute).Assembly
                .GetTypes()
                .SelectMany(t => t.GetCustomAttributes(typeof(SectionAttribute), true)
                                  .Cast<SectionAttribute>()
                                  .Select(a => new object[] { a.Category.ToString(), a.ContentType.Name }))
                .OrderBy(row => (string) row[0]).ThenBy(row => (string) row[1]);
        }

        [Fact]
        public void The_tree_offers_a_row_for_every_section()
        {
            // Guards the theory: if the tree were empty, or stopped listing
            // sections, every case below would pass while browsing nothing.
            var declared = SectionRows().Count();
            Assert.True(declared > 20, $"expected the sample to declare many sections, found {declared}");

            Run(() =>
            {
                var app = Browser.Open();
                Assert.Equal(declared, app.SectionLabels().Count);
            });
        }

        [Theory]
        [MemberData(nameof(SectionRows))]
        public void Selecting_a_section_mounts_it_and_shows_its_source(string category, string label)
        {
            Run(() =>
            {
                var app = Browser.Open();

                app.Select(label);

                // The section was instantiated and mounted into the content pane.
                Assert.NotNull(app.Content.FirstChild);

                // The notebook reveals its tabs only for a real section.
                Assert.True(app.Notebook.ShowTabs, $"'{label}' should show the notebook tabs");

                // And the code view holds that section's own source.
                var source = app.Code.Buffer.Text;
                Assert.False(string.IsNullOrWhiteSpace(source), $"no source loaded for '{label}'");
                Assert.Contains("class", source);
            });
        }

        [Fact]
        public void Selecting_a_category_row_clears_the_content_pane()
        {
            // Categories are the parent rows. They map to no section, so the
            // application must empty the pane rather than leave the last one on
            // screen.
            Run(() =>
            {
                var app = Browser.Open();

                app.Select(SectionRows().First()[1] as string);
                Assert.NotNull(app.Content.FirstChild);

                app.SelectCategoryRow();

                Assert.Null(app.Content.FirstChild);
                Assert.False(app.Notebook.ShowTabs);
            });
        }

        [Fact]
        public void Browsing_from_one_section_to_the_next_replaces_the_content()
        {
            // The pane must hold exactly one section at a time; a missed removal
            // would stack them up and only show as a layout oddity.
            Run(() =>
            {
                var app = Browser.Open();
                var labels = app.SectionLabels().Take(5).ToList();

                foreach (var label in labels)
                {
                    app.Select(label);

                    var children = 0;
                    for (var child = app.Content.FirstChild; child != null; child = child.NextSibling)
                        children++;

                    Assert.Equal(1, children);
                }
            });
        }

        /// <summary>
        /// The sample's main window, reached through its own widget tree rather
        /// than through added test hooks — so the structure the port produced is
        /// asserted here too.
        /// </summary>
        private sealed class Browser
        {
            public TreeView Tree { get; private set; }
            public Notebook Notebook { get; private set; }
            public Box Content { get; private set; }
            public GtkSource.SourceView Code { get; private set; }

            public static Browser Open()
            {
                // Several sections add actions to the application, exactly as
                // they do when the real program starts.
                Program.EnsureApplication();

                var window = new MainWindow();

                var split = Assert.IsType<Paned>(window.Child);
                var notebook = Assert.IsType<Notebook>(split.EndChild);
                var dataPage = Assert.IsType<ScrolledWindow>(notebook.GetNthPage(0));
                var codePage = Assert.IsType<ScrolledWindow>(notebook.GetNthPage(1));
                var dataSplit = Assert.IsType<Paned>(Unwrap(dataPage.Child));

                return new Browser
                {
                    Tree = Assert.IsType<TreeView>(split.StartChild),
                    Notebook = notebook,
                    Content = Assert.IsType<Box>(dataSplit.StartChild),
                    Code = Assert.IsType<GtkSource.SourceView>(Unwrap(codePage.Child)),
                };
            }

            /// <summary>
            /// A ScrolledWindow puts a GtkViewport around a child that cannot
            /// scroll itself, so its Child is not always the widget that was
            /// handed to it.
            /// </summary>
            private static Widget Unwrap(Widget child)
            {
                return child is Viewport viewport ? viewport.Child : child;
            }

            public List<string> SectionLabels()
            {
                var labels = new List<string>();
                var model = Tree.Model;

                if (!model.GetIterFirst(out TreeIter category))
                    return labels;

                do
                {
                    if (!model.IterChildren(out TreeIter row, category))
                        continue;

                    do
                    {
                        labels.Add(model.GetValue(row, 0).ToString());
                    } while (model.IterNext(ref row));
                } while (model.IterNext(ref category));

                return labels;
            }

            public void Select(string label)
            {
                var model = Tree.Model;
                Assert.True(model.GetIterFirst(out TreeIter category), "the tree has no rows");

                do
                {
                    if (!model.IterChildren(out TreeIter row, category))
                        continue;

                    do
                    {
                        if (model.GetValue(row, 0).ToString() != label)
                            continue;

                        Tree.ExpandRow(model.GetPath(category), false);
                        Tree.Selection.SelectIter(row);
                        Drain();
                        return;
                    } while (model.IterNext(ref row));
                } while (model.IterNext(ref category));

                Assert.Fail($"no row labelled '{label}' in the section tree");
            }

            public void SelectCategoryRow()
            {
                var model = Tree.Model;
                Assert.True(model.GetIterFirst(out TreeIter category), "the tree has no rows");

                Tree.Selection.SelectIter(category);
                Drain();
            }

            private static void Drain()
            {
                // Selection work and anything the new section queues.
                for (int i = 0; i < 200 && Application.EventsPending(); i++)
                    Application.RunIteration(false);
            }
        }
    }
}
