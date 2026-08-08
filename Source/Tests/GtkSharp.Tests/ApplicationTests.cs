using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The application object and the global state around it: the code every
    /// program runs before it does anything else.
    /// </summary>
    /// <remarks>
    /// Gtk.Application is one of the most heavily hand-written classes in this
    /// binding - the whole gtk_main family it used to be built on was deleted in
    /// Gtk 4 - and nothing had ever called it. Neither had anything called
    /// GLib.Application, Gtk.Settings, Gtk.IconTheme, Gtk.WindowGroup,
    /// Gtk.Accelerator or Gtk.Global.
    ///
    /// The oracles here are states the test arranged itself: how many times a
    /// signal fired, what a list contained after an add and a remove, what came
    /// back out of a setter. Nothing asserts a fact about the desktop, because
    /// there is not one: the container has no window manager, so which window
    /// has the pointer focus, what the theme is called and which icons are
    /// installed are all properties of the host.
    /// </remarks>
    public class ApplicationTests : GtkTestBase
    {
        public ApplicationTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ helpers

        /// <summary>Every application this file creates, kept alive for the life of the process.</summary>
        /// <remarks>
        /// g_application_get_default() is a bare static pointer - GLib stores it
        /// without taking a reference and never clears it - so a GApplication
        /// that is collected while it is the process default leaves the next
        /// caller holding freed memory. These tests make dozens of applications
        /// and one of them has to be the default, so none of them is ever let go.
        /// </remarks>
        private static readonly List<GLib.Application> Live = new List<GLib.Application>();

        private static int _serial;

        /// <summary>A registered, non-unique application with an id no other test uses.</summary>
        /// <remarks>
        /// NON_UNIQUE keeps the application off the bus name, so nothing here can
        /// meet - or become - a second instance of itself. Registration is what
        /// emits ::startup, and gtk_application_add_window refuses to do anything
        /// before it ("New application windows must be added after the
        /// GApplication::startup signal has been emitted"), so almost every test
        /// below needs it.
        /// </remarks>
        private static Gtk.Application NewApplication(string name, GLib.ApplicationFlags extra = GLib.ApplicationFlags.None)
        {
            var app = new Gtk.Application("org.gtksharp.tests." + name + (++_serial),
                                          GLib.ApplicationFlags.NonUnique | extra);
            Live.Add(app);
            return app;
        }

        private static Gtk.Application NewRegisteredApplication(string name, GLib.ApplicationFlags extra = GLib.ApplicationFlags.None)
        {
            var app = NewApplication(name, extra);
            Assert.True(app.Register(null), "registration failed");
            return app;
        }

        private static void Pump(int iterations = 400)
        {
            for (int i = 0; i < iterations && Gtk.Application.EventsPending(); i++)
                Gtk.Application.RunIteration(false);
        }

        /// <summary>Writes a one-icon hicolor theme into a directory and returns it.</summary>
        /// <remarks>
        /// Which icons the host has installed is not this binding's business -
        /// gvsbuild ships Adwaita, a bare container may ship nothing - so the
        /// icon the lookup tests ask for is one written here, under the icon
        /// name no theme will ever contain.
        /// </remarks>
        private const string TestThemeName = "gtksharp-test-theme";

        private static string BuildIconTheme()
        {
            string root = Path.Combine(Path.GetTempPath(), "gtksharp-tests-icons");
            string dir = Path.Combine(root, TestThemeName, "48x48", "apps");
            Directory.CreateDirectory(dir);

            // The theme must not be called "hicolor": Gtk appends hicolor to
            // every inheritance chain, and a theme directory of that name with
            // no Inherits key of its own ends up inheriting itself. Looking up
            // a *missing* icon then walks the chain until the stack runs out -
            // an uncatchable crash, so this is arranged so it cannot happen
            // rather than pinned by a test.
            File.WriteAllText(Path.Combine(root, TestThemeName, "index.theme"),
                "[Icon Theme]\nName=GtkSharp Test\nInherits=hicolor\nDirectories=48x48/apps\n\n" +
                "[48x48/apps]\nSize=48\nContext=Applications\nType=Fixed\n");

            using (var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 48, 48))
            {
                pixbuf.Fill(0xff0000ffu);
                File.WriteAllBytes(Path.Combine(dir, "gtksharp-test-icon.png"), pixbuf.SaveToBuffer("png"));
            }

            return root;
        }

        /// <summary>A private icon theme with the test icon appended to the default search path.</summary>
        /// <remarks>
        /// Appended, not assigned. A lookup that finds nothing falls back to
        /// image-missing, and image-missing is itself an icon that has to be
        /// found somewhere - so a theme whose search path has been *replaced*
        /// with a directory that has no fallback icons never terminates.
        /// Measured on Gtk 4.22: a display-less GtkIconTheme with its search
        /// path set to one directory blows the stack on the first missing icon.
        /// Appending is what an application shipping its own icons does anyway.
        /// </remarks>
        private static Gtk.IconTheme TestIconTheme()
        {
            var theme = new Gtk.IconTheme();
            theme.AddSearchPath(BuildIconTheme());
            theme.ThemeName = TestThemeName;
            return theme;
        }

        /// <summary>A GApplicationCommandLine built here rather than handed over by ::command-line.</summary>
        /// <remarks>
        /// "arguments" is construct-only and write-only and holds an "aay" - an
        /// array of NUL-terminated byte strings, which is what a POSIX argv is
        /// and what a C# string is not. Constructing one is the only way to test
        /// the reader without a second process: ::command-line is emitted by the
        /// primary instance over D-Bus, and what argv it is given is decided by
        /// the platform (Windows ignores the argv passed to g_application_run
        /// entirely and re-reads the real process command line).
        /// </remarks>
        private sealed class MadeCommandLine : GLib.ApplicationCommandLine
        {
            public MadeCommandLine(string[] argv) : base(IntPtr.Zero)
            {
                var words = new GLib.Variant[argv.Length];
                for (int i = 0; i < argv.Length; i++)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(argv[i] + "\0");
                    var octets = new GLib.Variant[bytes.Length];
                    for (int j = 0; j < bytes.Length; j++)
                        octets[j] = new GLib.Variant(bytes[j]);
                    words[i] = GLib.Variant.NewArray(new GLib.VariantType("y"), octets);
                }

                CreateNativeObject(new string[] { "arguments" },
                                   new GLib.Value[] { new GLib.Value(GLib.Variant.NewArray(new GLib.VariantType("ay"), words)) });
            }
        }

        // ------------------------------------------------- registration and life cycle

        // Registration is the whole of an application's startup: it is what emits
        // ::startup, and it is idempotent. A second Register must not run startup
        // again, or every application that registers defensively would set itself
        // up twice.
        [Fact]
        public void Registering_an_application_runs_startup_exactly_once()
        {
            Run(() =>
            {
                var app = NewApplication("Startup");
                int startups = 0;
                app.Startup += (o, a) => startups++;

                Assert.False(app.IsRegistered);
                Assert.Equal(0, startups);

                Assert.True(app.Register(null));
                Assert.True(app.IsRegistered);
                Assert.Equal(1, startups);

                Assert.True(app.Register(null));
                Assert.Equal(1, startups);

                // NON_UNIQUE never owns the bus name, so this process is always
                // the primary instance and never a proxy for another one.
                Assert.False(app.IsRemote);
            });
        }

        // ::activate, by contrast, is not once-only: it is the "the user asked
        // for you again" signal, and a second launch of a running application
        // arrives as a second activation.
        [Fact]
        public void Activating_an_application_emits_activate_every_time()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("Activate");
                int activations = 0;
                app.Activated += (o, a) => activations++;

                app.Activate();
                app.Activate();
                app.Activate();

                Assert.Equal(3, activations);
            });
        }

        // g_application_id_is_valid is what decides whether the constructor
        // works at all, and its rules are not obvious: a dot is required, a
        // trailing dot is not a name, a leading digit is not, and a hyphen is
        // allowed where an empty element is not.
        [Theory]
        [InlineData("org.gtk.Test", true)]
        [InlineData("org.gtk.Test-1", true)]
        [InlineData("org.gtk.Test_1", true)]
        [InlineData("nodot", false)]
        [InlineData("org.gtk.", false)]
        [InlineData("1org.gtk.Test", false)]
        [InlineData("org..gtk", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void An_application_id_is_a_dotted_name_that_starts_with_a_letter(string id, bool valid)
        {
            Run(() => Assert.Equal(valid, GLib.Application.IdIsValid(id)));
        }

        // An application's resource base path is derived from its id by turning
        // the dots into slashes - which is how GtkApplication finds an
        // application's menus and icons in its own GResource without being told
        // where they are.
        [Fact]
        public void The_resource_base_path_is_the_application_id_as_a_path()
        {
            Run(() =>
            {
                var app = NewApplication("Resource");
                Assert.Equal("/" + app.ApplicationId.Replace('.', '/'), app.ResourceBasePath);

                app.ResourceBasePath = "/somewhere/else";
                Assert.Equal("/somewhere/else", app.ResourceBasePath);
            });
        }

        // g_application_set_default installs a process-wide pointer that
        // GtkApplication, GtkBuilder and every "app." action lookup fall back on.
        [Fact]
        public void Setting_an_application_as_the_default_replaces_the_process_default()
        {
            Run(() =>
            {
                GLib.Application previous = GLib.Application.Default;

                var app = NewRegisteredApplication("Default");
                app.SetDefault();
                Assert.Same(app, GLib.Application.Default);

                var other = NewRegisteredApplication("Default");
                other.SetDefault();
                Assert.Same(other, GLib.Application.Default);

                // Put it back, so a later test reads what it would have read.
                // There is no way to restore a *null* default - the setter is an
                // instance method - which is why nothing here may be collected.
                previous?.SetDefault();
            });
        }

        // "Busy" and "held" are two different counters with two different jobs:
        // busy is advisory ("show a spinner"), a hold keeps g_application_run
        // from returning. Marking busy twice needs unmarking twice, and a hold
        // does not make the application busy at all.
        [Fact]
        public void Busy_is_a_counter_and_a_hold_is_not_part_of_it()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("Busy");
                Assert.False(app.IsBusy);

                app.MarkBusy();
                app.MarkBusy();
                Assert.True(app.IsBusy);

                app.UnmarkBusy();
                Assert.True(app.IsBusy);

                app.UnmarkBusy();
                Assert.False(app.IsBusy);

                app.Hold();
                Assert.False(app.IsBusy);
                app.Release();
            });
        }

        // g_application_bind_busy_property keeps the busy count in step with a
        // boolean property on another object, so a long operation only has to
        // set its own flag.
        [Fact]
        public void A_bound_property_drives_the_busy_state()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("BusyBind");
                var spinner = new Gtk.Spinner();

                app.BindBusyProperty(spinner.Handle, "spinning");
                Assert.False(app.IsBusy);

                spinner.Spinning = true;
                Assert.True(app.IsBusy);

                spinner.Spinning = false;
                Assert.False(app.IsBusy);

                app.UnbindBusyProperty(spinner.Handle, "spinning");
                spinner.Spinning = true;
                Assert.False(app.IsBusy);
            });
        }

        [Fact]
        public void An_inactivity_timeout_round_trips()
        {
            Run(() =>
            {
                var app = NewApplication("Inactivity");
                Assert.Equal(0u, app.InactivityTimeout);
                app.InactivityTimeout = 1234;
                Assert.Equal(1234u, app.InactivityTimeout);
            });
        }

        // ------------------------------------------------------------ open

        // g_application_open takes "GFile **files, gint n_files", and the api.xml
        // has no way to tie the two together: the binding took a single GFile and
        // handed Gio the object's own address as the base of the array, so the
        // first "file" it read was that object's class pointer. This is the entry
        // point HANDLES_OPEN exists for, and it could not be called.
        [Fact]
        public void Opening_files_hands_every_one_of_them_to_the_handler()
        {
            Run(() =>
            {
                var app = NewApplication("Open", GLib.ApplicationFlags.HandlesOpen);

                var seen = new List<string>();
                string hint = null;
                int count = -1;
                app.Opened += (o, a) =>
                {
                    count = a.NFiles;
                    hint = a.Hint;
                    for (int i = 0; i < a.NFiles; i++)
                        seen.Add(GLib.FileAdapter.GetObject(
                            System.Runtime.InteropServices.Marshal.ReadIntPtr(a.Files, i * IntPtr.Size), false).Basename);
                };

                Assert.True(app.Register(null));

                var files = new GLib.IFile[]
                {
                    GLib.FileFactory.NewForPath(Path.Combine(Path.GetTempPath(), "alpha.txt")),
                    GLib.FileFactory.NewForPath(Path.Combine(Path.GetTempPath(), "beta.txt")),
                    GLib.FileFactory.NewForPath(Path.Combine(Path.GetTempPath(), "gamma.txt")),
                };

                app.Open(files, "a-hint");

                Assert.Equal(3, count);
                Assert.Equal(new[] { "alpha.txt", "beta.txt", "gamma.txt" }, seen);
                Assert.Equal("a-hint", hint);
            });
        }

        // ------------------------------------------------------ command line

        // g_application_command_line_get_arguments returns "gchar **" with its
        // length beside it and, says the gir, without a terminating NULL - the
        // one array shape codegen has no rule for. It came out as a single
        // string, which is the program name and nothing else.
        [Fact]
        public void A_command_line_reports_every_argument_it_holds()
        {
            Run(() =>
            {
                var cmdline = new MadeCommandLine(new string[] { "prog", "--flag", "value", "" });

                int argc;
                string[] args = cmdline.GetArguments(out argc);

                Assert.Equal(4, argc);
                Assert.Equal(new[] { "prog", "--flag", "value", "" }, args);
                Assert.Equal(args, cmdline.Arguments);
            });
        }

        // An argv is bytes, not text: the round trip has to survive a word that
        // is longer in UTF-8 than it is in characters.
        [Fact]
        public void A_command_line_argument_survives_being_bytes()
        {
            Run(() =>
            {
                var cmdline = new MadeCommandLine(new string[] { "prog", "café-日本" });

                int argc;
                string[] args = cmdline.GetArguments(out argc);

                Assert.Equal(2, argc);
                Assert.Equal("café-日本", args[1]);
            });
        }

        // ------------------------------------------------------------ windows

        // gtk_application_get_windows prepends, so the list runs newest first -
        // and gtk_application_get_active_window is defined as its head. Reading
        // Windows[0] as "the first window I added" is the natural mistake.
        [Fact]
        public void The_window_list_runs_newest_first_and_the_active_window_is_its_head()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("Windows");
                Assert.Empty(app.Windows);
                Assert.Null(app.ActiveWindow);

                var first = new Gtk.Window { Title = "first" };
                var second = new Gtk.Window { Title = "second" };
                app.AddWindow(first);
                app.AddWindow(second);

                Assert.Equal(new[] { "second", "first" }, Array.ConvertAll(app.Windows, w => w.Title));
                Assert.Same(app.Windows[0], app.ActiveWindow);

                app.RemoveWindow(second);
                Assert.Equal(new[] { "first" }, Array.ConvertAll(app.Windows, w => w.Title));
                Assert.Same(first, app.ActiveWindow);

                app.RemoveWindow(first);
                Assert.Empty(app.Windows);
                Assert.Null(app.ActiveWindow);
            });
        }

        // The window's own Application property is the other half of the same
        // list: assigning it adds, clearing it removes.
        [Fact]
        public void A_windows_application_property_and_the_applications_window_list_are_one_thing()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("WindowApp");
                var window = new Gtk.Window();

                Assert.Null(window.Application);

                window.Application = app;
                Assert.Same(app, window.Application);
                Assert.Single(app.Windows);

                window.Application = null;
                Assert.Null(window.Application);
                Assert.Empty(app.Windows);
            });
        }

        [Fact]
        public void Adding_and_removing_a_window_is_announced()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("WindowSignals");
                Gtk.Window added = null, removed = null;
                app.WindowAdded += (o, a) => added = a.Window;
                app.WindowRemoved += (o, a) => removed = a.Window;

                var window = new Gtk.Window();
                app.AddWindow(window);
                Assert.Same(window, added);
                Assert.Null(removed);

                app.RemoveWindow(window);
                Assert.Same(window, removed);
            });
        }

        // A GtkApplicationWindow numbers itself so that the application can
        // address a window by id - the ids start at one and are handed back out
        // when a window is removed, which is why two live windows never share one.
        [Fact]
        public void An_application_window_has_an_id_that_finds_it_again()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("WindowId");
                var one = new Gtk.ApplicationWindow(app);
                var two = new Gtk.ApplicationWindow(app);

                Assert.NotEqual(0u, one.Id);
                Assert.NotEqual(one.Id, two.Id);
                Assert.Same(one, app.GetWindowById(one.Id));
                Assert.Same(two, app.GetWindowById(two.Id));
                Assert.Null(app.GetWindowById(9999));

                // A plain GtkWindow is not numbered at all.
                var plain = new Gtk.Window();
                app.AddWindow(plain);
                Assert.Equal(2, app.Windows.Length - 1);
            });
        }

        // ------------------------------------------------------------ actions

        [Fact]
        public void An_applications_actions_are_reachable_from_the_widgets_inside_it()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("WidgetAction");
                int fired = 0;
                var action = new GLib.SimpleAction("boom", null);
                action.Activated += (o, a) => fired++;
                app.AddAction(action);

                var window = new Gtk.ApplicationWindow(app);
                var button = new Gtk.Button();
                window.Child = button;

                // A widget resolves "app." against the application its window
                // belongs to, so a button never needs a reference to it.
                Assert.True(button.ActivateActionVariant("app.boom", null));
                Assert.Equal(1, fired);

                Assert.False(button.ActivateActionVariant("app.nosuchthing", null));
                Assert.Equal(1, fired);
            });
        }

        // The window is itself a GActionGroup, published under the "win" prefix.
        [Fact]
        public void An_application_window_is_an_action_group_of_its_own()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("WinAction");
                var window = new Gtk.ApplicationWindow(app);

                var action = new GLib.SimpleAction("toggle", null, new GLib.Variant(false));
                window.AddAction(action);

                Assert.Equal(new[] { "toggle" }, window.ListActions());
                Assert.Equal("false", window.GetActionState("toggle").Print(false));

                window.ChangeActionState("toggle", new GLib.Variant(true));
                Assert.Equal("true", window.GetActionState("toggle").Print(false));

                Assert.True(window.GetActionEnabled("toggle"));
                action.Enabled = false;
                Assert.False(window.GetActionEnabled("toggle"));

                Assert.True(window.ActivateActionVariant("win.toggle", null));

                window.RemoveAction("toggle");
                Assert.False(window.HasAction("toggle"));
            });
        }

        [Fact]
        public void An_action_receives_the_parameter_it_was_activated_with()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("ActionParam");
                string got = null;
                var action = new GLib.SimpleAction("greet", new GLib.VariantType("s"));
                action.Activated += (o, a) => got = (string)a.Parameter;
                app.AddAction(action);

                app.ActivateAction("greet", new GLib.Variant("hello"));

                Assert.Equal("hello", got);
                Assert.Equal("s", app.GetActionParameterType("greet").ToString());
            });
        }

        // ------------------------------------------------------- accelerators

        [Fact]
        public void Accelerators_round_trip_through_the_application()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("Accels");

                Assert.Empty(app.GetAccelsForAction("win.save"));

                app.SetAccelsForAction("win.save", new[] { "<Control>s", "F2" });
                app.SetAccelsForAction("app.quit", new[] { "<Control>q" });

                Assert.Equal(new[] { "<Control>s", "F2" }, app.GetAccelsForAction("win.save"));
                Assert.Equal(new[] { "win.save" }, app.GetActionsForAccel("F2"));
                Assert.Equal(new[] { "app.quit" }, app.GetActionsForAccel("<Control>q"));
                Assert.Empty(app.GetActionsForAccel("<Control>z"));

                Assert.Equal(new[] { "win.save", "app.quit" }, app.ListActionDescriptions());

                app.SetAccelsForAction("win.save", new string[0]);
                Assert.Empty(app.GetAccelsForAction("win.save"));
                Assert.Empty(app.GetActionsForAccel("F2"));
                Assert.Equal(new[] { "app.quit" }, app.ListActionDescriptions());
            });
        }

        // A detailed action name carries its target, and Gtk stores it in the
        // canonical g_action_parse_detailed_name form: what goes in as
        // "app.open('x')" comes back out as "app.open::x". Comparing the string
        // you passed against the string you get is a bug waiting to happen.
        [Fact]
        public void An_accelerator_for_a_targeted_action_comes_back_normalised()
        {
            Run(() =>
            {
                var app = NewRegisteredApplication("AccelTarget");
                app.SetAccelsForAction("app.open('x')", new[] { "<Control>1" });

                Assert.Equal(new[] { "<Control>1" }, app.GetAccelsForAction("app.open('x')"));
                Assert.Equal(new[] { "app.open::x" }, app.GetActionsForAccel("<Control>1"));

                // The untargeted action is a different action entirely.
                Assert.Empty(app.GetAccelsForAction("app.open"));
            });
        }

        [Fact]
        public void An_accelerator_string_survives_being_parsed_and_named_again()
        {
            Run(() =>
            {
                uint key;
                Gdk.ModifierType mods;

                Assert.True(Gtk.Accelerator.Parse("<Shift><Control>x", out key, out mods));
                Assert.Equal((uint)'x', key);
                Assert.Equal(Gdk.ModifierType.ShiftMask | Gdk.ModifierType.ControlMask, mods);
                Assert.True(Gtk.Accelerator.Valid(key, mods));
                Assert.Equal("<Shift><Control>x", Gtk.Accelerator.Name(key, mods));

                // The label is for a menu and the name is for a file: they are
                // deliberately not the same string.
                Assert.NotEqual(Gtk.Accelerator.Name(key, mods), Gtk.Accelerator.GetLabel(key, mods));

                Assert.False(Gtk.Accelerator.Parse("<Nonsense>zzz", out key, out mods));
                Assert.Equal(0u, key);
                Assert.Equal(Gdk.ModifierType.NoModifierMask, mods);

                // "Valid" is about the key, not about whether it is a sensible
                // shortcut: a bare letter passes, and only a key that cannot be
                // an accelerator at all - a modifier - is refused.
                Assert.True(Gtk.Accelerator.Valid((uint)'x', Gdk.ModifierType.NoModifierMask));
                Assert.False(Gtk.Accelerator.Valid((uint)Gdk.Key.Shift_L, Gdk.ModifierType.ControlMask));
            });
        }

        // gtk_accelerator_parse_with_keycode's accelerator_codes is a "guint **"
        // out-parameter holding a zero-terminated array that the caller must
        // free. Bound as "out uint" it gave Gtk four bytes to write an
        // eight-byte pointer into and reported the low half of an address as a
        // keycode. The oracle is Gdk: every keycode it hands back has to
        // translate back to the keyval that was asked for.
        [Fact]
        public void Parsing_with_keycodes_returns_keycodes_that_produce_the_key()
        {
            Run(() =>
            {
                uint key;
                uint[] codes;
                Gdk.ModifierType mods;

                Assert.True(Gtk.Accelerator.ParseWithKeycode("<Control>a", Gdk.Display.Default,
                                                             out key, out codes, out mods));
                Assert.Equal((uint)'a', key);
                Assert.Equal(Gdk.ModifierType.ControlMask, mods);
                Assert.NotEmpty(codes);

                foreach (uint code in codes)
                {
                    Assert.NotEqual(0u, code);

                    uint keyval;
                    int group, level;
                    Gdk.ModifierType consumed;
                    Assert.True(Gdk.Display.Default.TranslateKey(code, Gdk.ModifierType.NoModifierMask, 0,
                                                                 out keyval, out group, out level, out consumed));
                    Assert.Equal((uint)'a', keyval);
                }

                // A null display means the default one, not "no keymap".
                uint key2;
                uint[] codes2;
                Gdk.ModifierType mods2;
                Assert.True(Gtk.Accelerator.ParseWithKeycode("<Control>a", null, out key2, out codes2, out mods2));
                Assert.Equal(key, key2);
                Assert.Equal(codes, codes2);
            });
        }

        // ----------------------------------------------------------- settings

        // Gtk.Settings is where an application reads what the desktop decided
        // and, occasionally, overrides it. What matters is that an override is
        // an override: gtk_settings_reset_property has to give back the value
        // the theme and the platform agreed on, not a hard-coded default.
        [Fact]
        public void Overriding_a_setting_and_resetting_it_restores_the_original()
        {
            Run(() =>
            {
                var settings = Gtk.Settings.Default;
                Assert.NotNull(settings);

                string original = settings.GtkFontName;
                try
                {
                    settings.GtkFontName = "GtkSharp Test 42";
                    Assert.Equal("GtkSharp Test 42", settings.GtkFontName);
                }
                finally
                {
                    settings.ResetProperty("gtk-font-name");
                }

                Assert.Equal(original, settings.GtkFontName);
            });
        }

        [Fact]
        public void A_settings_change_notifies_until_the_handler_is_removed()
        {
            Run(() =>
            {
                var settings = Gtk.Settings.Default;
                int notifications = 0;
                var handler = new GLib.NotifyHandler((o, a) => notifications++);

                bool original = settings.GtkEnableAnimations;
                try
                {
                    settings.AddNotification("gtk-enable-animations", handler);

                    settings.GtkEnableAnimations = !original;
                    Assert.Equal(!original, settings.GtkEnableAnimations);
                    Assert.Equal(1, notifications);

                    settings.GtkEnableAnimations = original;
                    Assert.Equal(2, notifications);

                    settings.RemoveNotification("gtk-enable-animations", handler);
                    settings.GtkEnableAnimations = !original;
                    Assert.Equal(2, notifications);
                }
                finally
                {
                    settings.GtkEnableAnimations = original;
                    settings.ResetProperty("gtk-enable-animations");
                }
            });
        }

        // There is one settings object per display, and a widget reads that one -
        // not a copy, and not a global.
        [Fact]
        public void A_widget_reads_the_settings_of_its_display()
        {
            Run(() =>
            {
                var settings = Gtk.Settings.GetForDisplay(Gdk.Display.Default);
                Assert.Same(Gtk.Settings.Default, settings);
                Assert.Same(settings, new Gtk.Button().Settings);
            });
        }

        // --------------------------------------------------------- icon theme

        // GetSearchPath/SetSearchPath were hidden by a mono-era metadata rule, so
        // the property fell back on the "search-path" GObject property - which
        // holds a G_TYPE_STRV and was emitted as a *string*. Reading it gave
        // null and writing it asked GObject to turn a string into a strv, which
        // it refuses. The path an application adds its own icons to was
        // unreachable in both directions.
        [Fact]
        public void An_icon_themes_search_path_round_trips()
        {
            Run(() =>
            {
                var theme = new Gtk.IconTheme();

                Assert.NotEmpty(theme.SearchPath);

                theme.SearchPath = new[] { "/one", "/two" };
                Assert.Equal(new[] { "/one", "/two" }, theme.SearchPath);

                theme.AddSearchPath("/three");
                Assert.Equal(new[] { "/one", "/two", "/three" }, theme.SearchPath);

                theme.SearchPath = new string[0];
                Assert.Empty(theme.SearchPath);
            });
        }

        [Fact]
        public void An_icon_themes_resource_path_round_trips()
        {
            Run(() =>
            {
                var theme = new Gtk.IconTheme();

                // A fresh theme already looks inside Gtk's own resources.
                Assert.Contains("/org/gtk/libgtk/icons/", theme.ResourcePath);

                theme.ResourcePath = new[] { "/org/example/icons" };
                Assert.Equal(new[] { "/org/example/icons" }, theme.ResourcePath);

                theme.AddResourcePath("/org/example/more");
                Assert.Equal(new[] { "/org/example/icons", "/org/example/more" }, theme.ResourcePath);
            });
        }

        // Starting a GtkApplication registers its own resource directory with the
        // display's icon theme, which is how an application's icons are found by
        // name without any code. This is the visible half of gtk_application's
        // ::startup, and the only part of it observable from managed code.
        [Fact]
        public void Starting_an_application_adds_its_icon_resources_to_the_display_theme()
        {
            Run(() =>
            {
                var theme = Gtk.IconTheme.GetForDisplay(Gdk.Display.Default);

                var app = NewApplication("IconResource");
                string expected = app.ResourceBasePath + "/icons/";

                Assert.DoesNotContain(expected, theme.ResourcePath);
                Assert.True(app.Register(null));
                Assert.Contains(expected, theme.ResourcePath);
            });
        }

        // The whole point of a search path: an icon dropped into a directory is
        // found by name, at the size the theme declares, and looked up to the
        // file it came from.
        [Fact]
        public void An_icon_on_the_search_path_is_found_at_the_size_the_theme_declares()
        {
            Run(() =>
            {
                var theme = TestIconTheme();

                Assert.True(theme.HasIcon("gtksharp-test-icon"));
                Assert.False(theme.HasIcon("gtksharp-icon-that-is-not-there"));
                Assert.Contains("gtksharp-test-icon", theme.IconNames);

                // index.theme said 48 and nothing else, so that is the one size
                // available without scaling.
                Assert.Equal(new[] { 48 }, theme.GetIconSizes("gtksharp-test-icon"));

                var paintable = theme.LookupIcon("gtksharp-test-icon", null, 48, 1, TextDirection.Ltr, 0);
                Assert.NotNull(paintable);
                Assert.Equal("gtksharp-test-icon", paintable.IconName);
                Assert.False(paintable.IsSymbolic);
                Assert.Equal("gtksharp-test-icon.png", paintable.File.Basename);
                Assert.Equal(48, paintable.IntrinsicWidth);
                Assert.Equal(48, paintable.IntrinsicHeight);
            });
        }

        // A lookup never fails: it falls through to image-missing, which is a
        // real paintable backed by a real file. Testing the result for null
        // concludes that a missing icon was found.
        [Fact]
        public void Looking_up_an_icon_that_is_not_there_yields_image_missing()
        {
            Run(() =>
            {
                var theme = TestIconTheme();

                var paintable = theme.LookupIcon("gtksharp-icon-that-is-not-there", null, 32, 1, TextDirection.Ltr, 0);
                Assert.NotNull(paintable);
                Assert.Equal("image-missing", paintable.IconName);

                // A fallback list is tried in order before that happens.
                var found = theme.LookupIcon("gtksharp-icon-that-is-not-there",
                                             new[] { "gtksharp-test-icon" }, 48, 1, TextDirection.Ltr, 0);
                Assert.Equal("gtksharp-test-icon", found.IconName);
            });
        }

        // ------------------------------------------------------------ windows

        // DestroyWithParent is the difference between a dialog that goes away
        // with the document it belongs to and one that outlives it. The oracle
        // is the global toplevel list rather than the window objects, because
        // after gtk_window_destroy the widget is gone and asking it anything is
        // a use-after-free - the reason this test is written the long way round.
        [Fact]
        public void A_transient_child_goes_away_with_its_parent_only_if_it_was_told_to()
        {
            Run(() =>
            {
                bool Listed(string title) =>
                    Array.Exists(Gtk.Window.ListToplevels(), w => w.Title == title);

                var parent = new Gtk.Window { Title = "gtksharp-parent" };
                var bound = new Gtk.Window { Title = "gtksharp-bound-child" };
                var free = new Gtk.Window { Title = "gtksharp-free-child" };

                bound.TransientFor = parent;
                bound.Modal = true;
                bound.DestroyWithParent = true;
                free.TransientFor = parent;
                free.DestroyWithParent = false;

                Assert.Same(parent, bound.TransientFor);
                Assert.True(bound.Modal);
                Assert.True(bound.DestroyWithParent);
                Assert.False(free.DestroyWithParent);

                parent.Present();
                bound.Present();
                free.Present();
                Pump();

                // Modality is not insensitivity: the parent is still a live,
                // sensitive widget, it just cannot be reached with a pointer.
                Assert.True(parent.Sensitive);
                Assert.True(Listed("gtksharp-parent"));
                Assert.True(Listed("gtksharp-bound-child"));

                parent.Destroy();
                Pump();

                Assert.False(Listed("gtksharp-parent"));
                Assert.False(Listed("gtksharp-bound-child"));
                Assert.True(Listed("gtksharp-free-child"));

                free.Destroy();
                Pump();
            });
        }

        // ::close-request is a veto, not a notification: returning true stops the
        // close. Gtk.Window.Close is the Gtk 4 replacement for gtk_widget_destroy
        // and goes through it, so a handler that refuses keeps the window up.
        [Fact]
        public void A_close_request_handler_can_refuse_the_close()
        {
            Run(() =>
            {
                var window = new Gtk.Window { Title = "gtksharp-veto" };
                int requests = 0;
                window.CloseRequest += (o, a) =>
                {
                    requests++;
                    a.RetVal = requests == 1;      // refuse the first, allow the second
                };

                window.Present();
                Pump();
                Assert.True(window.Visible);

                window.Close();
                Pump();
                Assert.Equal(1, requests);
                Assert.True(window.Visible);

                window.Close();
                Pump();
                Assert.Equal(2, requests);

                // A close that is not refused runs gtk_window_destroy, so the
                // widget is gone: asking *it* anything afterwards reads freed
                // memory. The toplevel list is the thing left to ask.
                Assert.DoesNotContain(Gtk.Window.ListToplevels(), w => w.Title == "gtksharp-veto");
            });
        }

        [Fact]
        public void A_default_size_is_what_the_window_asks_for_and_keeps()
        {
            Run(() =>
            {
                var window = new Gtk.Window();

                int width, height;
                window.GetDefaultSize(out width, out height);
                Assert.Equal(0, width);
                Assert.Equal(0, height);

                window.SetDefaultSize(437, 311);
                window.GetDefaultSize(out width, out height);
                Assert.Equal(437, width);
                Assert.Equal(311, height);

                window.Present();
                Pump();
                window.GetDefaultSize(out width, out height);
                Assert.Equal(437, width);
                Assert.Equal(311, height);

                window.Close();
                Pump();
            });
        }

        // A window group scopes a modal grab. A window is always in one - the
        // display's default group if nothing else - so removing it from a group
        // does not leave it in none.
        [Fact]
        public void A_window_group_owns_the_windows_added_to_it()
        {
            Run(() =>
            {
                var first = new Gtk.Window();
                var second = new Gtk.Window();

                var defaultGroup = first.Group;
                Assert.NotNull(defaultGroup);

                var group = new Gtk.WindowGroup();
                group.AddWindow(first);
                group.AddWindow(second);

                Assert.Equal(2, group.ListWindows().Length);
                Assert.Same(group, first.Group);
                Assert.Same(first.Group, second.Group);

                group.RemoveWindow(first);
                Assert.Single(group.ListWindows());
                Assert.NotSame(group, first.Group);
                Assert.Same(group, second.Group);
            });
        }

        [Fact]
        public void A_window_appears_in_the_toplevel_list_and_the_toplevel_model()
        {
            Run(() =>
            {
                var window = new Gtk.Window { Title = "gtksharp-toplevel-probe" };

                Assert.Contains(Gtk.Window.ListToplevels(), w => w.Title == "gtksharp-toplevel-probe");

                var model = Gtk.Window.Toplevels;
                bool found = false;
                for (uint i = 0; i < model.NItems; i++)
                    found |= (GLib.Object.GetObject(model.GetItem(i)) as Gtk.Window)?.Title == "gtksharp-toplevel-probe";
                Assert.True(found, "the toplevel list model disagrees with gtk_window_list_toplevels");

                Assert.Equal(Gtk.Window.ListToplevels().Length, (int)model.NItems);
            });
        }

        // --------------------------------------------- header bar and controls

        // A GtkHeaderBar's packed children are not its children: it wraps
        // everything in a GtkWindowHandle so that a drag on the bar moves the
        // window. Walking FirstChild/NextSibling to find a packed button finds
        // the handle instead.
        [Fact]
        public void A_header_bar_puts_its_children_behind_a_window_handle()
        {
            Run(() =>
            {
                var bar = new HeaderBar();
                var title = new Label("Title");
                bar.TitleWidget = title;
                Assert.Same(title, bar.TitleWidget);

                var button = new Gtk.Button();
                bar.PackStart(button);

                var children = new List<Gtk.Widget>();
                for (var child = ((Gtk.Widget)bar).FirstChild; child != null; child = child.NextSibling)
                    children.Add(child);

                Assert.Single(children);
                Assert.IsType<WindowHandle>(children[0]);

                // The button is in there, just further down.
                Assert.Same(bar, button.GetAncestor(HeaderBar.GType));

                Assert.True(bar.ShowTitleButtons);
                bar.ShowTitleButtons = false;
                Assert.False(bar.ShowTitleButtons);

                bar.DecorationLayout = "icon:minimize,close";
                Assert.Equal("icon:minimize,close", bar.DecorationLayout);
            });
        }

        // GtkWindowControls shows the half of the decoration layout on its own
        // side of the colon, so the same layout string makes exactly one of a
        // start/end pair empty.
        [Fact]
        public void Window_controls_show_only_the_side_they_are_packed_on()
        {
            Run(() =>
            {
                var window = new Gtk.Window();
                var bar = new HeaderBar();
                window.Titlebar = bar;

                var start = new WindowControls(PackType.Start);
                var end = new WindowControls(PackType.End);
                Assert.Equal(PackType.Start, start.Side);
                Assert.Equal(PackType.End, end.Side);

                bar.PackStart(start);
                bar.PackEnd(end);
                window.Present();
                Pump();

                start.DecorationLayout = "close:";
                end.DecorationLayout = "close:";
                Pump();
                Assert.False(start.Empty);
                Assert.True(end.Empty);

                start.DecorationLayout = ":close";
                end.DecorationLayout = ":close";
                Pump();
                Assert.True(start.Empty);
                Assert.False(end.Empty);

                window.Close();
                Pump();
            });
        }

        // ---------------------------------------------------------- Gtk.Global

        // gtk_check_version answers with NULL when the running Gtk is usable,
        // and its message is written from the *caller's* point of view: asking
        // for an older major version reports the library as too new.
        [Fact]
        public void The_running_gtk_is_the_one_the_bindings_were_generated_from()
        {
            Run(() =>
            {
                Assert.True(Gtk.Global.IsInitialized);
                Assert.Equal(4u, Gtk.Global.MajorVersion);

                Assert.Null(Gtk.Global.CheckVersion(4, 0, 0));
                Assert.Null(Gtk.Global.CheckVersion(Gtk.Global.MajorVersion, Gtk.Global.MinorVersion, Gtk.Global.MicroVersion));

                Assert.NotNull(Gtk.Global.CheckVersion(Gtk.Global.MajorVersion, Gtk.Global.MinorVersion + 1, 0));
                Assert.Contains("too old", Gtk.Global.CheckVersion(99, 0, 0));
                Assert.Contains("too new", Gtk.Global.CheckVersion(3, 0, 0));
            });
        }

        // Pure arithmetic, so the oracle is colour theory: hue 0 is red, hue 120
        // is green at whatever value it is given, and the two conversions invert.
        // The tolerance is one part in ten thousand because both functions take
        // and return single-precision floats.
        [Fact]
        public void Hsv_and_rgb_are_inverses()
        {
            Run(() =>
            {
                float r, g, b, h, s, v;

                Gtk.Global.HsvToRgb(0f, 1f, 1f, out r, out g, out b);
                Assert.Equal(1f, r, 4);
                Assert.Equal(0f, g, 4);
                Assert.Equal(0f, b, 4);

                Gtk.Global.HsvToRgb(120f / 360f, 1f, 0.5f, out r, out g, out b);
                Assert.Equal(0f, r, 4);
                Assert.Equal(0.5f, g, 4);
                Assert.Equal(0f, b, 4);

                Gtk.Global.RgbToHsv(0.2f, 0.6f, 0.9f, out h, out s, out v);
                Assert.Equal(0.9f, v, 4);                       // value is the largest channel
                Assert.Equal((0.9f - 0.2f) / 0.9f, s, 4);       // saturation is its relative spread
                Gtk.Global.HsvToRgb(h, s, v, out r, out g, out b);
                Assert.Equal(0.2f, r, 4);
                Assert.Equal(0.6f, g, 4);
                Assert.Equal(0.9f, b, 4);
            });
        }

        // gtk_distribute_natural_allocation reads n_requested_sizes structs and
        // writes each one's allocation back into MinimumSize. Codegen marshalled
        // one struct by value into memory it freed on return, so with more than
        // one size Gtk wrote past a 24-byte block and with exactly one the answer
        // was thrown away. The algorithm is "give to the poorest first", which is
        // arithmetic this test can do itself.
        [Fact]
        public void Extra_space_goes_to_the_children_with_the_smallest_gap_first()
        {
            Run(() =>
            {
                var sizes = new Gtk.RequestedSize[3];
                sizes[0].MinimumSize = 10; sizes[0].NaturalSize = 20;   // gap 10
                sizes[1].MinimumSize = 10; sizes[1].NaturalSize = 100;  // gap 90
                sizes[2].MinimumSize = 10; sizes[2].NaturalSize = 12;   // gap 2

                // 15 to share out. Smallest gap first: the third child takes the
                // 2 it wants, leaving 13 for two children -> 7 and then 6.
                int left = Gtk.Global.DistributeNaturalAllocation(15, sizes);

                Assert.Equal(0, left);
                Assert.Equal(17, sizes[0].MinimumSize);
                Assert.Equal(16, sizes[1].MinimumSize);
                Assert.Equal(12, sizes[2].MinimumSize);

                // Natural sizes are never exceeded, and what is left over is
                // reported rather than forced on anyone.
                var plenty = new Gtk.RequestedSize[2];
                plenty[0].MinimumSize = 5; plenty[0].NaturalSize = 8;
                plenty[1].MinimumSize = 5; plenty[1].NaturalSize = 9;

                Assert.Equal(93, Gtk.Global.DistributeNaturalAllocation(100, plenty));
                Assert.Equal(8, plenty[0].MinimumSize);
                Assert.Equal(9, plenty[1].MinimumSize);
            });
        }

        [Fact]
        public void The_debug_flags_round_trip()
        {
            Run(() =>
            {
                var original = Gtk.Global.DebugFlags;
                try
                {
                    Gtk.Global.DebugFlags = Gtk.DebugFlags.Geometry | Gtk.DebugFlags.Actions;
                    Assert.Equal(Gtk.DebugFlags.Geometry | Gtk.DebugFlags.Actions, Gtk.Global.DebugFlags);
                }
                finally
                {
                    Gtk.Global.DebugFlags = original;
                }

                Assert.Equal(original, Gtk.Global.DebugFlags);
            });
        }

        [Fact]
        public void The_default_window_icon_name_round_trips()
        {
            Run(() =>
            {
                string original = Gtk.Window.DefaultIconName;
                try
                {
                    Gtk.Window.DefaultIconName = "gtksharp-test-icon";
                    Assert.Equal("gtksharp-test-icon", Gtk.Window.DefaultIconName);
                }
                finally
                {
                    Gtk.Window.DefaultIconName = original;
                }

                Assert.Equal(original, Gtk.Window.DefaultIconName);
            });
        }

        // -------------------------------------------- the main loop Gtk 4 removed

        // Gtk 4 deleted gtk_main and everything around it, so Application.Run,
        // Quit, EventsPending and RunIteration are all hand-written here over a
        // GLib.MainLoop. Before that they were bound to symbols that no longer
        // exist, which meant a null delegate and no Gtk 4 application could
        // start at all.
        [Fact]
        public void Run_returns_when_something_in_the_loop_calls_Quit()
        {
            Run(() =>
            {
                int ticks = 0;
                GLib.Timeout.Add(10, () =>
                {
                    ticks++;
                    Gtk.Application.Quit();
                    return false;
                });

                var clock = System.Diagnostics.Stopwatch.StartNew();
                Gtk.Application.Run();
                clock.Stop();

                Assert.Equal(1, ticks);
                Assert.True(clock.ElapsedMilliseconds < 10000, "Run did not return promptly after Quit");

                // Quitting a loop that is not running is not an error - the
                // static loop is shared, and a second Quit must not throw.
                Gtk.Application.Quit();
                Gtk.Application.Quit();
            });
        }

        // Application.Invoke is the "do this on the Gtk thread" helper, built on
        // a zero-length timeout. It has two overloads, and the one that carries a
        // sender and arguments has to hand back the very objects it was given.
        [Fact]
        public void Invoke_runs_a_handler_on_the_main_loop_with_the_sender_it_was_given()
        {
            Run(() =>
            {
                object sender = null;
                System.EventArgs args = null;
                var marker = new object();
                var payload = new System.EventArgs();

                Gtk.Application.Invoke(marker, payload, (o, e) => { sender = o; args = e; });
                Assert.Null(sender);                       // nothing runs until the loop turns

                for (int i = 0; i < 1000 && sender == null; i++)
                    Gtk.Application.RunIteration(false);

                Assert.Same(marker, sender);
                Assert.Same(payload, args);
            });
        }
    }
}
