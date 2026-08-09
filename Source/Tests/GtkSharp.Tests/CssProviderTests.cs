using System;
using System.Reflection;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gtk.CssProvider</c>'s hand-written half — loading CSS from a manifest
    /// resource — and the parse-error reporting Gtk 4 replaced its return value
    /// with.
    /// </summary>
    /// <remarks>
    /// <c>Source/Libs/GtkSharp/CssProvider.cs</c> reported no coverage at all, and
    /// it is the entry point the templates and the getting-started guide reach
    /// for. It can only be exercised against a real manifest resource, so the
    /// test project embeds one.
    ///
    /// The oracle is <c>gtk_css_provider_to_string</c>: a provider hands back the
    /// CSS it is holding, so "did the load work" is a question with an answer
    /// rather than an absence of exceptions.
    ///
    /// The second half matters more than it looks. Gtk 3's
    /// <c>gtk_css_provider_load_from_data</c> returned a boolean and filled a
    /// <c>GError</c>; **Gtk 4's returns void and reports through the
    /// ::parsing-error signal instead**. So the hand-written wrapper's `return
    /// true` is unconditional, and a caller who trusts it will not notice broken
    /// CSS. These tests pin both halves of that: the return value is not a
    /// verdict, and the signal is.
    /// </remarks>
    public class CssProviderTests : GtkTestBase
    {
        public CssProviderTests(GtkFixture fixture) : base(fixture) { }

        const string Resource = "GtkSharp.Tests.embedded-style.css";

        // ------------------------------------------------------- from resource

        [Fact]
        public void CSS_loaded_from_a_resource_is_the_CSS_in_the_resource()
        {
            Run(() =>
            {
                var provider = new Gtk.CssProvider();

                Assert.True(provider.LoadFromResource(typeof(CssProviderTests).Assembly, Resource));

                // The provider hands back what it parsed, so this is the load
                // actually having happened rather than not having thrown.
                var loaded = provider.ToString();

                Assert.Contains("gtksharp-test-marker", loaded);
                Assert.Contains("rgb(12,34,56)", loaded.Replace(" ", ""));

                // What comes back is the *parsed* CSS re-serialised, not the
                // source text: Gtk normalises keyword values, so the resource's
                // `font-weight: bold` reads back as its numeric weight. Asserting
                // "bold" here would be asserting that ToString echoes the input,
                // which is the thing this method does not do.
                Assert.Contains("font-weight", loaded);
                Assert.Contains("700", loaded);
                Assert.DoesNotContain("bold", loaded);
            });
        }

        [Fact]
        public void The_overload_without_an_assembly_looks_in_its_caller()
        {
            // LoadFromResource(string) resolves Assembly.GetCallingAssembly(),
            // which is why both it and the overload it delegates to are marked
            // NoInlining. The test *is* the calling assembly, so a provider
            // loaded this way has to hold the same CSS as one told explicitly.
            Run(() =>
            {
                var implicitly_ = new Gtk.CssProvider();
                Assert.True(implicitly_.LoadFromResource(Resource));

                var explicitly = new Gtk.CssProvider();
                explicitly.LoadFromResource(typeof(CssProviderTests).Assembly, Resource);

                Assert.Equal(explicitly.ToString(), implicitly_.ToString());
                Assert.Contains("gtksharp-test-marker", implicitly_.ToString());
            });
        }

        [Fact]
        public void A_resource_name_that_does_not_exist_says_so()
        {
            Run(() =>
            {
                var provider = new Gtk.CssProvider();

                var error = Assert.Throws<ArgumentException>(
                    () => provider.LoadFromResource(typeof(CssProviderTests).Assembly, "no.such.resource"));

                Assert.Contains("no.such.resource", error.Message);
                Assert.Equal("resource", error.ParamName);
            });
        }

        [Fact]
        public void A_null_assembly_falls_back_to_the_caller_rather_than_throwing()
        {
            // The documented fallback in the wrapper. Passing null is not the
            // same as passing nothing, and it has its own branch.
            Run(() =>
            {
                var provider = new Gtk.CssProvider();

                Assert.True(provider.LoadFromResource(null, Resource));
                Assert.Contains("gtksharp-test-marker", provider.ToString());
            });
        }

        // --------------------------------------------------- what the CSS does

        [Fact]
        public void A_loaded_provider_changes_what_a_widget_looks_like()
        {
            // Holding the text is not the same as the text having an effect.
            // Adding the provider to the display and giving a label the class
            // the rule selects changes the label's colour, which the widget can
            // be asked for.
            Run(() =>
            {
                var provider = new Gtk.CssProvider();
                provider.LoadFromResource(typeof(CssProviderTests).Assembly, Resource);

                var label = new Gtk.Label("styled");
                var window = new Gtk.Window();
                window.Child = label;

                var display = Gdk.Display.Default;
                Assert.NotNull(display);

                Gtk.StyleContext.AddProviderForDisplay(
                    display, provider, Gtk.StyleProviderPriority.Application);
                try
                {
                    label.AddCssClass("gtksharp-test-marker");
                    Assert.Contains("gtksharp-test-marker", label.CssClasses);

                    var styled = label.Color;

                    // rgb(12, 34, 56) as Gdk.RGBA's 0..1 floats.
                    Assert.Equal(12 / 255.0, styled.Red, 2);
                    Assert.Equal(34 / 255.0, styled.Green, 2);
                    Assert.Equal(56 / 255.0, styled.Blue, 2);
                }
                finally
                {
                    Gtk.StyleContext.RemoveProviderForDisplay(display, provider);
                    window.Destroy();
                }
            });
        }

        // ------------------------------------------------ errors, or the lack

        [Fact]
        public void Valid_CSS_reports_no_parsing_error()
        {
            // The control for the test below. Without it, a ::parsing-error
            // handler that never fires would look like success.
            Run(() =>
            {
                var provider = new Gtk.CssProvider();
                int errors = 0;
                provider.ParsingError += (o, a) => errors++;

                provider.LoadFromData("label { color: rgb(1, 2, 3); }");

                Assert.Equal(0, errors);
            });
        }

        [Fact]
        public void Broken_CSS_reports_through_the_signal_and_not_through_the_return_value()
        {
            // The Gtk 3 to Gtk 4 change, pinned. gtk_css_provider_load_from_data
            // no longer returns a verdict, so LoadFromResource's `return true`
            // is unconditional -- it means "the resource was found and handed
            // over", never "the CSS was good".
            //
            // A caller who wants to know has to listen to ::parsing-error.
            Run(() =>
            {
                var provider = new Gtk.CssProvider();
                int errors = 0;
                provider.ParsingError += (o, a) => errors++;

                provider.LoadFromData("label { this-is-not-a-property: @@@ }");

                Assert.True(errors > 0, "broken CSS should have raised ::parsing-error");
            });
        }

        [Fact]
        public void Loading_again_replaces_what_the_provider_held()
        {
            // A provider is not an accumulator: each load discards the last.
            // Worth pinning, because "load twice and both apply" is the
            // reasonable other guess.
            Run(() =>
            {
                var provider = new Gtk.CssProvider();

                provider.LoadFromData("label { color: rgb(1, 2, 3); }");
                Assert.Contains("label", provider.ToString());

                provider.LoadFromData("button { color: rgb(4, 5, 6); }");

                var loaded = provider.ToString();
                Assert.Contains("button", loaded);
                Assert.DoesNotContain("label", loaded);
            });
        }
    }
}
