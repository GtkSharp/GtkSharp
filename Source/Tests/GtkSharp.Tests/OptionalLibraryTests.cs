using System;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// WebKit and JavaScriptCore, which are not present everywhere.
    /// </summary>
    /// <remarks>
    /// The gvsbuild bundle GtkSharp.targets installs on Windows contains
    /// neither library, so these skip there with a reason and run on CI, which
    /// installs them from apt. They are written as skippable rather than
    /// omitted because a binding nobody ever calls is exactly the situation
    /// that hid a dozen dead symbols during this migration: build-verified
    /// proves very little when a missing export is a null delegate rather than
    /// a link error.
    /// </remarks>
    public class OptionalLibraryTests : GtkTestBase
    {
        public OptionalLibraryTests(GtkFixture fixture) : base(fixture) { }

        [SkippableFact]
        public void Javascript_values_round_trip_through_JSCValue()
        {
            Skip.IfNot(JavaScriptCore.Global.IsSupported,
                       "JavaScriptCore is not installed (gvsbuild ships no jsc).");

            Run(() =>
            {
                var context = new JavaScriptCore.Context();

                var number = context.Evaluate("21 * 2");
                Assert.True(number.IsNumber);
                Assert.Equal(42, number.ToInt32());

                var text = context.Evaluate("'hello ' + 'world'");
                Assert.True(text.IsString);
                Assert.Equal("hello world", text.ToString());
            });
        }

        [SkippableFact]
        public void A_WebView_can_be_constructed()
        {
            Skip.IfNot(WebKit.Global.IsSupported,
                       "WebKitGTK is not installed (gvsbuild ships no webkit).");

            Run(() =>
            {
                var view = new WebKit.WebView();

                Assert.NotEqual(IntPtr.Zero, view.Handle);
                Assert.IsAssignableFrom<Gtk.Widget>(view);
            });
        }
    }
}
