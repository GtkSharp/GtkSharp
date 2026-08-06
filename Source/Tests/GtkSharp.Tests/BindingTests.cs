using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Behavioural tests for the bindings, each pinning something the Gtk 4
    /// migration changed or fixed.
    /// </summary>
    /// <remarks>
    /// These matter more than they might look. A native export that no longer
    /// exists does not fail to link: FuncLoader returns a null delegate, so the
    /// binding compiles and throws only when the call is reached. Whole families
    /// of removed Gtk 3 functions survived the port that way. Only a test that
    /// actually calls something and checks the answer will catch the next one.
    /// </remarks>
    public class BindingTests : GtkTestBase
    {
        public BindingTests(GtkFixture gtk) : base(gtk) { }

        [Fact]
        public void Button_constructed_with_a_string_carries_it_as_the_label()
        {
            // Gtk 3 read this string as a stock id, via gtk_button_new_from_stock.
            // Gtk 4 removed that symbol, so the ctor threw NullReferenceException
            // on a null delegate for every caller.
            var label = Run(() => new Button("Click me").Label);

            Assert.Equal("Click me", label);
        }

        [Fact]
        public void Box_Append_makes_the_child_reachable_and_parented()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var child = new Label("child");

                box.Append(child);

                Assert.Equal(child.Handle, box.FirstChild.Handle);
                Assert.Equal(box.Handle, child.Parent.Handle);
            });
        }

        [Fact]
        public void Box_ReorderChildAfter_moves_the_child()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Horizontal, 0);
                var first = new Label("first");
                var second = new Label("second");
                box.Append(first);
                box.Append(second);

                Assert.Equal(first.Handle, box.FirstChild.Handle);

                box.ReorderChildAfter(first, second);

                Assert.Equal(second.Handle, box.FirstChild.Handle);
            });
        }

        [Fact]
        public void Widget_Unparent_clears_the_parent()
        {
            Run(() =>
            {
                var box = new Box(Orientation.Vertical, 0);
                var child = new Label("child");
                box.Append(child);

                child.Unparent();

                Assert.Null(child.Parent);
                Assert.Null(box.FirstChild);
            });
        }

        [Fact]
        public void ScrolledWindow_Child_round_trips()
        {
            // Gtk 4 replaced Container.Add with a single Child property.
            Run(() =>
            {
                var scroller = new ScrolledWindow();
                var view = new TextView();

                scroller.Child = view;

                Assert.Equal(view.Handle, scroller.Child.Handle);
            });
        }

        [Fact]
        public void Grid_QueryChild_returns_what_was_attached()
        {
            // Gtk 4 removed child properties; attach data is queried from the grid.
            Run(() =>
            {
                var grid = new Grid();
                var child = new Label("cell");

                grid.Attach(child, 2, 3, 4, 5);
                grid.QueryChild(child, out int column, out int row, out int width, out int height);

                Assert.Equal(2, column);
                Assert.Equal(3, row);
                Assert.Equal(4, width);
                Assert.Equal(5, height);
            });
        }

        [Fact]
        public void StackPage_title_round_trips()
        {
            // What used to be a stack child property is a real GObject in Gtk 4.
            Run(() =>
            {
                var stack = new Stack();
                var child = new Label("page");

                StackPage page = stack.AddTitled(child, "one", "One");
                Assert.Equal("One", page.Title);

                page.Title = "Renamed";

                Assert.Equal("Renamed", stack.GetPage(child).Title);
            });
        }

        [Fact]
        public void Entry_text_round_trips()
        {
            Run(() =>
            {
                var entry = new Entry();

                entry.Text = "typed";

                Assert.Equal("typed", entry.Text);
            });
        }

        [Fact]
        public void TextBuffer_insert_round_trips()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null);
                var end = buffer.EndIter;

                buffer.Insert(ref end, "hello");

                Assert.Equal("hello", buffer.Text);
            });
        }

        [Fact]
        public void Adjustment_clamps_value_to_its_range()
        {
            Run(() =>
            {
                var adjustment = new Adjustment(0, 0, 10, 1, 1, 0);

                adjustment.Value = 25;

                Assert.Equal(10, adjustment.Value);
            });
        }

        [Fact]
        public void RGBA_Parse_fills_in_the_components()
        {
            // The colour button sample parsed into a copy and discarded it; this
            // pins what Parse actually does. Components are 0..1 floats in Gtk 4.
            Run(() =>
            {
                var rgba = new Gdk.RGBA();

                Assert.True(rgba.Parse("#729FCF"));

                Assert.Equal(0x72 / 255f, rgba.Red, 3);
                Assert.Equal(0x9F / 255f, rgba.Green, 3);
                Assert.Equal(0xCF / 255f, rgba.Blue, 3);
                Assert.Equal(1f, rgba.Alpha, 3);
            });
        }

        [Fact]
        public void Snapshot_produces_a_render_node()
        {
            // GskRenderNode is a GLib fundamental type, bound on GLib.Opaque
            // rather than GLib.Object. This is the end-to-end check that the
            // hierarchy works: build a snapshot, get a node back, and have it
            // survive being wrapped.
            Run(() =>
            {
                var snapshot = new Snapshot();
                var bounds = Graphene.Rect.Alloc();
                bounds.Init(0, 0, 10, 10);

                snapshot.AppendColor(new Gdk.RGBA { Red = 1f, Green = 0f, Blue = 0f, Alpha = 1f }, bounds);

                Gsk.RenderNode node = snapshot.ToNode();

                Assert.NotNull(node);
                Assert.NotEqual(IntPtr.Zero, node.Handle);
            });
        }

        [Fact]
        public void ConstantExpression_reports_the_type_it_holds()
        {
            // GtkExpression is the other fundamental hierarchy. Constructing one
            // and asking it a question exercises the ctor's ownership handling
            // as well as the binding.
            Run(() =>
            {
                var expression = new ConstantExpression(new GLib.Value(42));

                Assert.Equal(GLib.GType.Int, expression.ValueType);
            });
        }

        [Fact]
        public void Application_Run_returns_once_Quit_is_called()
        {
            // Gtk 4 removed gtk_main and gtk_main_quit, so Application.Run was a
            // null delegate and no application could start. The idle is queued
            // before Run so it cannot fire ahead of the loop it has to stop.
            Run(() =>
            {
                GLib.Idle.Add(() =>
                {
                    Application.Quit();
                    return false;
                });

                Application.Run();
            });
        }
    }
}
