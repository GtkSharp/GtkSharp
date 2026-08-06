using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Samples;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Opens the windows the samples can open, which constructing a section
    /// never does — dialogs appear on a button press.
    /// </summary>
    /// <remarks>
    /// Dialogs changed more than almost anything else in Gtk 4:
    /// <c>gtk_dialog_run</c> is gone because nested main loops are not allowed,
    /// <c>GtkAboutDialog</c> no longer derives from <c>GtkDialog</c> at all, and
    /// file choosers answer with a <c>GFile</c> rather than a path. None of that
    /// is reachable from a section constructor, so none of it was covered.
    /// </remarks>
    public class ChildWindowTests : GtkTestBase
    {
        public ChildWindowTests(GtkFixture gtk) : base(gtk) { }

        private static IEnumerable<Widget> Descendants(Widget root)
        {
            for (var child = root.FirstChild; child != null; child = child.NextSibling)
            {
                yield return child;

                foreach (var descendant in Descendants(child))
                    yield return descendant;
            }
        }

        public static IEnumerable<object[]> Sections() => SampleSectionTests.Sections();

        /// <summary>
        /// True for buttons a test must not press, with the reason for each.
        /// </summary>
        /// <remarks>
        /// Both are the sample behaving as written, not defects:
        ///
        /// PixbufDemo is a manual leak-stress toggle. The first press enters an
        /// unbounded allocation loop that exits only when a second press clears
        /// its running flag, so with no user it never returns. Skipping it by
        /// name beats a timeout, which would make the suite's duration a matter
        /// of luck.
        ///
        /// LinkButton asks the desktop to open a URI, which launches a browser.
        /// A test suite should not open browser windows on the machine running
        /// it, least of all in CI.
        /// </remarks>
        private static bool HasSideEffectsUnsuitableForTests(Widget button)
        {
            return button.GetType().Name == "PixbufDemo"
                || button is LinkButton;
        }

        /// <summary>
        /// Presses every button a section contains, and requires that whatever
        /// window that opens is a real, live toplevel that can be closed again.
        /// </summary>
        /// <remarks>
        /// This is the deepest reach into the samples available: it runs the
        /// sections' own handlers rather than a re-implementation of them, so a
        /// dialog the port broke fails here and not in a test that only happens
        /// to agree with it.
        /// </remarks>
        [Theory]
        [MemberData(nameof(Sections))]
        public void Section_buttons_can_be_pressed_and_any_window_they_open_is_live(string typeName)
        {
            var type = typeof(SectionAttribute).Assembly.GetType(typeName);
            Assert.NotNull(type);

            Run(() =>
            {
                Program.EnsureApplication();

                var before = Window.ListToplevels().Select(w => w.Handle).ToHashSet();
                var section = (Widget) Activator.CreateInstance(type);

                foreach (var button in Descendants(section).OfType<Button>())
                {
                    if (HasSideEffectsUnsuitableForTests(button))
                        continue;

                    // Widget.Activate does not reach the handler -- Gtk 4 routes
                    // a button press through a gesture -- and gtk_button_clicked
                    // is gone, so the signal is emitted directly.
                    GLib.Signal.Emit(button, "clicked");
                }

                // Let the dialogs actually map before they are inspected.
                for (int i = 0; i < 200 && Application.EventsPending(); i++)
                    Application.RunIteration(false);

                var opened = Window.ListToplevels()
                    .Where(w => !before.Contains(w.Handle))
                    .ToList();

                foreach (var window in opened)
                {
                    Assert.NotEqual(IntPtr.Zero, window.Handle);

                    // Gtk 4 tears a toplevel down with gtk_window_destroy;
                    // leaving them open would leak across the whole run.
                    window.Destroy();
                }
            });
        }

        [Fact]
        public void Pressing_the_file_chooser_button_opens_a_toplevel()
        {
            // Guards the theory above against passing vacuously. If emitting
            // "clicked" ever stops reaching handlers -- as Widget.Activate
            // silently did -- every section would still "pass" while pressing
            // nothing. This asserts the mechanism works by naming the one
            // sample that is supposed to open a window.
            Run(() =>
            {
                Program.EnsureApplication();

                var before = Window.ListToplevels().Select(w => w.Handle).ToHashSet();
                var section = new FileChooserDialogSection();

                var button = Descendants(section).OfType<Button>().Single();
                GLib.Signal.Emit(button, "clicked");

                for (int i = 0; i < 200 && Application.EventsPending(); i++)
                    Application.RunIteration(false);

                var opened = Window.ListToplevels().Where(w => !before.Contains(w.Handle)).ToList();

                Assert.Single(opened);
                Assert.IsType<FileChooserDialog>(opened[0]);

                opened[0].Destroy();
            });
        }

        [Fact]
        public void AboutDialog_presents_with_the_properties_the_sample_sets()
        {
            // GtkAboutDialog derives from GtkWindow in Gtk 4, not GtkDialog, so
            // it has no response to wait for. The sample sets these properties
            // and presents it; this pins that they survive the round trip.
            Run(() =>
            {
                var dialog = new AboutDialog
                {
                    ProgramName = "GtkSharp Sample Application",
                    Version = "1.0.0.0",
                    LogoIconName = "system-run-symbolic",
                    Website = "https://www.github.com/GtkSharp/GtkSharp",
                };

                Assert.IsAssignableFrom<Window>(dialog);

                dialog.Present();

                Assert.True(dialog.Visible);
                Assert.Equal("GtkSharp Sample Application", dialog.ProgramName);
                Assert.Equal("1.0.0.0", dialog.Version);

                dialog.Destroy();
            });
        }

        [Fact]
        public void FileChooserDialog_takes_buttons_and_answers_with_a_response()
        {
            // gtk_dialog_run is gone, so the answer arrives on the Response
            // signal. Emitting a response directly is what lets that path be
            // tested without a user: it proves the handler is wired and that the
            // response id survives the marshalling.
            Run(() =>
            {
                var dialog = new FileChooserDialog("Open File", null, FileChooserAction.Open);
                dialog.AddButton("_Cancel", ResponseType.Cancel);
                dialog.AddButton("_Open", ResponseType.Ok);

                int seen = int.MinValue;
                dialog.Response += (o, args) => seen = args.ResponseId;

                dialog.Present();
                Assert.True(dialog.Visible);

                // gtk_dialog_response is bound as Respond, renamed so it does
                // not collide with the Response signal it raises.
#pragma warning disable CS0618
                dialog.Respond((int) ResponseType.Cancel);
#pragma warning restore CS0618

                Assert.Equal((int) ResponseType.Cancel, seen);

                dialog.Destroy();
            });
        }
    }
}
