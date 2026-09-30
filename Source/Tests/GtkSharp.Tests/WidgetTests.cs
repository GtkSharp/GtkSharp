using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// State round-trips across the widgets an application actually uses.
    /// </summary>
    /// <remarks>
    /// Each of these sets something and reads it back through a separate native
    /// call, so a broken property pair fails rather than passing quietly. They
    /// are cheap individually; the value is in the breadth, since a codegen
    /// change touches every property at once.
    /// </remarks>
    public class WidgetTests : GtkTestBase
    {
        public WidgetTests(GtkFixture fixture) : base(fixture) { }

        [Theory]
        [InlineData("")]
        [InlineData("plain text")]
        [InlineData("ünïcödé ☺")]
        public void Label_text_round_trips(string text)
        {
            Run(() =>
            {
                var label = new Label(null) { Text = text };
                Assert.Equal(text, label.Text);
            });
        }

        [Fact]
        public void Label_markup_sets_the_text_without_the_tags()
        {
            Run(() =>
            {
                var label = new Label(null);

                label.Markup = "<b>bold</b>";

                Assert.True(label.UseMarkup);
                Assert.Equal("bold", label.Text);
            });
        }

        [Fact]
        public void CheckButton_active_round_trips_and_reports_toggles()
        {
            Run(() =>
            {
                var check = new CheckButton();
                int toggles = 0;
                check.Toggled += (o, e) => toggles++;

                check.Active = true;

                Assert.True(check.Active);
                Assert.Equal(1, toggles);
            });
        }

        [Fact]
        public void ToggleButton_and_CheckButton_can_be_grouped()
        {
            // Grouping makes them mutually exclusive: activating the second
            // must clear the first.
            Run(() =>
            {
                var first = new CheckButton();
                var second = new CheckButton();
                second.Group = first;

                first.Active = true;
                second.Active = true;

                Assert.False(first.Active);
                Assert.True(second.Active);
            });
        }

        [Fact]
        public void ProgressBar_clamps_its_fraction_to_the_unit_range()
        {
            Run(() =>
            {
                var bar = new ProgressBar();

                bar.Fraction = 0.25;
                Assert.Equal(0.25, bar.Fraction, 3);

                bar.Fraction = 2.0;
                Assert.Equal(1.0, bar.Fraction, 3);
            });
        }

        [Fact]
        public void LevelBar_reports_the_value_within_its_range()
        {
            Run(() =>
            {
                var level = new LevelBar { MinValue = 0, MaxValue = 10, Value = 4 };

                Assert.Equal(4, level.Value, 3);
                Assert.Equal(10, level.MaxValue, 3);
            });
        }

        [Fact]
        public void Scale_reads_its_value_from_the_adjustment_it_shares()
        {
            Run(() =>
            {
                var adjustment = new Adjustment(0, 0, 100, 1, 10, 0);
                var scale = new Scale(Orientation.Horizontal, adjustment);

                adjustment.Value = 42;

                Assert.Equal(42, scale.Value, 3);
            });
        }

        [Fact]
        public void SpinButton_rounds_to_its_configured_digits()
        {
            Run(() =>
            {
                var spin = new SpinButton(0, 10, 1) { Digits = 0 };

                spin.Value = 3.7;
                spin.Update();

                Assert.Equal(4, spin.ValueAsInt);
            });
        }

        [Fact]
        public void Expander_expanded_round_trips()
        {
            Run(() =>
            {
                var expander = new Expander("more") { Child = new Label("inside") };

                expander.Expanded = true;

                Assert.True(expander.Expanded);
                Assert.NotNull(expander.Child);
            });
        }

        [Fact]
        public void Notebook_reports_the_pages_appended_to_it()
        {
            Run(() =>
            {
                var book = new Notebook();

                book.AppendPage(new Label("one"), new Label("First"));
                book.AppendPage(new Label("two"), new Label("Second"));

                Assert.Equal(2, book.NPages);

                book.CurrentPage = 1;
                Assert.Equal(1, book.CurrentPage);
            });
        }

        [Fact]
        public void Stack_switches_between_named_children()
        {
            Run(() =>
            {
                var stack = new Stack();
                var first = new Label("one");
                var second = new Label("two");

                stack.AddNamed(first, "first");
                stack.AddNamed(second, "second");

                stack.VisibleChildName = "second";

                Assert.Equal("second", stack.VisibleChildName);
                Assert.Equal(second.Handle, stack.VisibleChild.Handle);
            });
        }

        [Fact]
        public void Paned_holds_both_children_and_its_position()
        {
            Run(() =>
            {
                var paned = new Paned(Orientation.Horizontal)
                {
                    StartChild = new Label("left"),
                    EndChild = new Label("right"),
                    Position = 120,
                };

                Assert.NotNull(paned.StartChild);
                Assert.NotNull(paned.EndChild);
                Assert.Equal(120, paned.Position);
            });
        }

        [Fact]
        public void Frame_label_round_trips()
        {
            Run(() =>
            {
                var frame = new Frame("titled") { Child = new Label("inside") };
                Assert.Equal("titled", frame.Label);
            });
        }

        [Fact]
        public void Revealer_reports_the_reveal_it_was_asked_for()
        {
            Run(() =>
            {
                var revealer = new Revealer { Child = new Label("hidden"), TransitionDuration = 0 };

                revealer.RevealChild = true;

                Assert.True(revealer.RevealChild);
            });
        }

        [Fact]
        public void Widget_sensitivity_and_visibility_round_trip()
        {
            Run(() =>
            {
                var button = new Button("press");

                button.Sensitive = false;
                button.Visible = false;

                Assert.False(button.Sensitive);
                Assert.False(button.Visible);
            });
        }

        [Fact]
        public void Widget_size_request_is_reported_back()
        {
            Run(() =>
            {
                var button = new Button("press");

                button.SetSizeRequest(200, 40);
                button.GetSizeRequest(out int width, out int height);

                Assert.Equal(200, width);
                Assert.Equal(40, height);
            });
        }

        [Fact]
        public void Css_classes_can_be_added_queried_and_removed()
        {
            Run(() =>
            {
                var button = new Button("press");

                button.AddCssClass("suggested-action");
                Assert.True(button.HasCssClass("suggested-action"));

                button.RemoveCssClass("suggested-action");
                Assert.False(button.HasCssClass("suggested-action"));
            });
        }

        [Fact]
        public void A_css_provider_parses_what_it_is_given()
        {
            Run(() =>
            {
                var provider = new CssProvider();

                // Malformed CSS reports through the parsing-error signal rather
                // than throwing, so a bad rule would otherwise pass unnoticed.
                bool failed = false;
                provider.ParsingError += (o, args) => failed = true;

                provider.LoadFromData("button { color: red; }");

                Assert.False(failed);
            });
        }
    }
}
