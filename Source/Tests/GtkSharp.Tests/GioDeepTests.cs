using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The half of Gio that needs a filesystem and a main loop: GSettings over a
    /// schema this file compiles itself, GFileMonitor over a directory it builds,
    /// the async pattern end to end, GCancellable stopping an operation that is
    /// already running, and GFile's copy/move/delete with their error paths.
    /// </summary>
    /// <remarks>
    /// Every oracle here is outside the library. The settings tests read the
    /// keyfile GSettings wrote with <c>System.IO</c>, so what is asserted is what
    /// landed on disk rather than what GSettings remembers; the schema's own XML
    /// is the source of truth for the defaults. The file tests compare against
    /// bytes this test wrote, and the async tests against the managed thread id
    /// the caller was on.
    ///
    /// The async shape is the one most likely to be subtly wrong, because it
    /// crosses the main loop: a callback that never arrives, arrives on a pool
    /// thread, or arrives with a result the Finish call cannot read would all
    /// look like "the operation worked" from the outside.
    /// </remarks>
    public class GioDeepTests : GtkTestBase
    {
        public GioDeepTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ plumbing

        /// <summary>
        /// Runs the main loop until <paramref name="done"/> holds or the deadline
        /// passes, and reports which.
        /// </summary>
        /// <remarks>
        /// A blocking iteration parks forever when nothing is ready, and a Gio
        /// operation that never completes would hang the run rather than fail
        /// it. The ticking timeout is what makes the deadline reachable.
        /// </remarks>
        private static bool PumpUntil(Func<bool> done, int milliseconds = 10000)
        {
            var clock = Stopwatch.StartNew();
            var tick = GLib.Timeout.Add(10, () => true);
            try
            {
                while (!done() && clock.ElapsedMilliseconds < milliseconds)
                    GLib.MainContext.Iteration(true);
            }
            finally
            {
                GLib.Source.Remove(tick);
            }

            return done();
        }

        /// <summary>A directory that exists only while <paramref name="body"/> runs.</summary>
        private static void WithTempDir(Action<string> body)
        {
            var dir = Path.Combine(Path.GetTempPath(),
                                   "gtksharp-gio-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                body(dir);
            }
            finally
            {
                try { Directory.Delete(dir, true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        private static GLib.IFile FileAt(params string[] parts)
            => GLib.FileFactory.NewForPath(Path.Combine(parts));

        // ------------------------------------------------------------- GSettings

        private const string SchemaId = "org.gtksharp.tests.deep";
        private const string ChildSchemaId = "org.gtksharp.tests.deep.sub";
        private const string SchemaPath = "/org/gtksharp/tests/deep/";

        /// <summary>
        /// Everything the settings tests assert is declared here, so the schema
        /// is the oracle: 'hello' and 7 and 0.5 are facts about this text, not
        /// about GSettings.
        /// </summary>
        private const string SchemaXml = @"<?xml version='1.0' encoding='UTF-8'?>
<schemalist>
  <enum id='org.gtksharp.tests.deep.Fruit'>
    <value nick='apple' value='0'/>
    <value nick='banana' value='1'/>
    <value nick='cherry' value='2'/>
  </enum>
  <flags id='org.gtksharp.tests.deep.Topping'>
    <value nick='cheese' value='1'/>
    <value nick='ham' value='2'/>
    <value nick='olives' value='4'/>
  </flags>
  <schema id='org.gtksharp.tests.deep' path='/org/gtksharp/tests/deep/'>
    <key name='greeting' type='s'>
      <default>'hello'</default>
      <summary>The greeting</summary>
      <description>Words used to greet somebody.</description>
    </key>
    <key name='count' type='i'>
      <default>7</default>
      <range min='0' max='100'/>
    </key>
    <key name='ratio' type='d'>
      <default>0.5</default>
    </key>
    <key name='enabled' type='b'>
      <default>true</default>
    </key>
    <key name='words' type='as'>
      <default>['alpha','beta']</default>
    </key>
    <key name='fruit' enum='org.gtksharp.tests.deep.Fruit'>
      <default>'banana'</default>
    </key>
    <key name='toppings' flags='org.gtksharp.tests.deep.Topping'>
      <default>['cheese','olives']</default>
    </key>
    <key name='huge' type='x'>
      <default>9007199254740993</default>
    </key>
    <child name='sub' schema='org.gtksharp.tests.deep.sub'/>
  </schema>
  <schema id='org.gtksharp.tests.deep.sub' path='/org/gtksharp/tests/deep/sub/'>
    <key name='nested' type='s'>
      <default>'inner'</default>
    </key>
  </schema>
</schemalist>";

        /// <summary>
        /// The compiled schema directory, or null when glib-compile-schemas is
        /// not installed. Compiled once: the tests share the compiled schema and
        /// each takes its own keyfile, so none of them can see another's writes.
        /// </summary>
        private static readonly Lazy<string> SchemaDir = new Lazy<string>(CompileSchema);

        private static string FindSchemaCompiler()
        {
            var exe = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "glib-compile-schemas.exe"
                : "glib-compile-schemas";

            var candidates = new List<string>();

            // Debian ships it in the multiarch glib private directory; only
            // libglib2.0-dev-bin puts a copy on PATH, and that is not installed
            // just because Gtk is.
            foreach (var root in new[] { "/usr/lib", "/usr/lib64", "/usr/local/lib", "/usr/libexec" })
            {
                if (!Directory.Exists(root))
                    continue;
                candidates.Add(Path.Combine(root, "glib-2.0", exe));
                foreach (var arch in Directory.EnumerateDirectories(root))
                    candidates.Add(Path.Combine(arch, "glib-2.0", exe));
            }

            // The gvsbuild bundle GtkSharp.targets installs on Windows.
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            if (!string.IsNullOrEmpty(localAppData))
                candidates.Add(Path.Combine(localAppData, "Gtk", "4.22.4", "bin", exe));

            foreach (var entry in (Environment.GetEnvironmentVariable("PATH") ?? "")
                                  .Split(Path.PathSeparator))
                if (!string.IsNullOrWhiteSpace(entry))
                    candidates.Add(Path.Combine(entry.Trim(), exe));

            return candidates.FirstOrDefault(File.Exists);
        }

        private static string CompileSchema()
        {
            var compiler = FindSchemaCompiler();
            if (compiler == null)
                return null;

            var dir = Path.Combine(Path.GetTempPath(),
                                   "gtksharp-schemas-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "org.gtksharp.tests.deep.gschema.xml"),
                              SchemaXml, new UTF8Encoding(false));

            using var process = Process.Start(new ProcessStartInfo(compiler, "\"" + dir + "\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0 || !File.Exists(Path.Combine(dir, "gschemas.compiled")))
                throw new InvalidOperationException(
                    "glib-compile-schemas failed with " + process.ExitCode + ": " + stderr);

            return dir;
        }

        private static void SkipWithoutSchemas()
            => Skip.If(SchemaDir.Value == null,
                       "glib-compile-schemas is not installed; the settings tests compile their own schema.");

        /// <summary>
        /// Hands the body a GSettings over the test's own schema, backed by a
        /// keyfile at <c>keyfile</c> so that what it stored can be read back off
        /// disk without going through GSettings again.
        /// </summary>
        private static void WithSettings(Action<GLib.Settings, string> body)
            => WithTempDir(dir =>
            {
                var keyfile = Path.Combine(dir, "settings.ini");
                var source = new GLib.SettingsSchemaSource(SchemaDir.Value, null, true);
                var schema = source.Lookup(SchemaId, false);
                Assert.NotNull(schema);

                var backend = GLib.GioGlobal.KeyfileSettingsBackendNew(keyfile, SchemaPath, "Test");
                var settings = new GLib.Settings(schema, backend, null);

                body(settings, keyfile);
            });

        /// <summary>Reads the backend's keyfile once it contains <paramref name="expected"/>.</summary>
        private static string KeyfileText(string keyfile, string expected)
        {
            PumpUntil(() => File.Exists(keyfile) && File.ReadAllText(keyfile).Contains(expected), 3000);
            return File.Exists(keyfile) ? File.ReadAllText(keyfile) : "";
        }

        [SkippableFact]
        public void A_schema_source_lists_the_schema_ids_the_compiled_directory_holds()
        {
            // g_settings_schema_source_list_schemas fills two gchar*** -- two
            // NULL-terminated string arrays -- and codegen had no rule for a
            // triple pointer, so it read the array of pointers as one UTF-8
            // string and g_freed it. The call could not report anything.
            SkipWithoutSchemas();

            Run(() =>
            {
                var source = new GLib.SettingsSchemaSource(SchemaDir.Value, null, true);

                source.ListSchemas(false, out var nonRelocatable, out var relocatable);

                // Both schemas in the XML declare a path, so both are
                // non-relocatable and the other list is empty.
                Assert.Equal(new[] { SchemaId, ChildSchemaId },
                             nonRelocatable.OrderBy(s => s, StringComparer.Ordinal).ToArray());
                Assert.Empty(relocatable);
            });
        }

        [SkippableFact]
        public void Every_key_starts_at_the_default_the_schema_declared()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                Assert.Equal("hello", settings.GetString("greeting"));
                Assert.Equal(7, settings.GetInt("count"));
                Assert.Equal(0.5, settings.GetDouble("ratio"));
                Assert.True(settings.GetBoolean("enabled"));
                Assert.Equal(new[] { "alpha", "beta" }, settings.GetStrv("words"));

                // 2^53 + 1: a long that a double cannot represent, so this fails
                // if the value ever went through a float on the way here.
                Assert.Equal(9007199254740993L, settings.GetInt64("huge"));

                // Nothing has been written, so the backend has no file yet.
                Assert.False(File.Exists(keyfile));
            }));
        }

        [SkippableFact]
        public void A_written_setting_lands_in_the_keyfile_in_GVariant_text_form()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                Assert.True(settings.SetString("greeting", "written"));
                Assert.True(settings.SetInt("count", 42));

                var text = KeyfileText(keyfile, "count=42");

                // The root group is the one the backend was created with, and a
                // string is stored quoted because the file holds GVariant text
                // rather than raw values.
                Assert.Contains("[Test]", text);
                Assert.Contains("greeting='written'", text);
                Assert.Contains("count=42", text);

                Assert.Equal("written", settings.GetString("greeting"));
            }));
        }

        [SkippableFact]
        public void Resetting_a_key_removes_it_from_the_keyfile_and_restores_the_default()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                settings.SetString("greeting", "temporary");
                Assert.Contains("greeting='temporary'", KeyfileText(keyfile, "greeting='temporary'"));

                settings.Reset("greeting");

                Assert.Equal("hello", settings.GetString("greeting"));

                // A reset key is *deleted*, not written back as the default --
                // which is what lets a later change of default reach the user.
                PumpUntil(() => !File.ReadAllText(keyfile).Contains("greeting"), 3000);
                Assert.DoesNotContain("greeting", File.ReadAllText(keyfile));
            }));
        }

        [SkippableFact]
        public void A_user_value_is_reported_separately_from_the_default()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                Assert.Null(settings.GetUserValue("greeting"));
                Assert.Equal("hello", (string) settings.GetDefaultValue("greeting"));

                settings.SetString("greeting", "mine");

                Assert.Equal("mine", (string) settings.GetUserValue("greeting"));

                // The default does not move when the value does; this is the
                // pair a "reset to defaults" button is written against.
                Assert.Equal("hello", (string) settings.GetDefaultValue("greeting"));

                settings.Reset("greeting");
                Assert.Null(settings.GetUserValue("greeting"));
            }));
        }

        [SkippableFact]
        public void A_range_declared_in_the_schema_rejects_values_outside_it()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                var key = settings.SettingsSchema.GetKey("count");

                Assert.True(key.RangeCheck(new GLib.Variant(100)));
                Assert.True(key.RangeCheck(new GLib.Variant(0)));
                Assert.False(key.RangeCheck(new GLib.Variant(101)));
                Assert.False(key.RangeCheck(new GLib.Variant(-1)));

                // The range is reported as (sv): the word "range" and a boxed
                // variant, which for a range holds (min, max). The second child
                // therefore has to be unboxed before the bounds are reachable --
                // reading it as the tuple gives one child, not two.
                var range = key.Range.ToArray();
                Assert.Equal("range", (string) range[0]);

                var boxed = range[1].ToArray();
                Assert.Single(boxed);

                var bounds = boxed[0].ToArray();
                Assert.Equal(0, (int) bounds[0]);
                Assert.Equal(100, (int) bounds[1]);
            }));
        }

        [SkippableFact]
        public void The_changed_signal_names_the_key_that_changed_and_only_that_key()
        {
            // GSettings::changed and GFileMonitor::changed both wanted a class
            // called GLib.ChangedArgs. This is the half that kept the name, so
            // it is also the half that proves the split did not break it.
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                var seen = new List<string>();
                GLib.ChangedHandler handler = (o, args) => seen.Add(args.Key);
                settings.Changed += handler;

                settings.SetString("greeting", "signalled");
                PumpUntil(() => seen.Count > 0, 3000);

                Assert.Equal(new[] { "greeting" }, seen.ToArray());
                Assert.Same(settings, seen.Count > 0 ? settings : null);

                settings.Changed -= handler;
                settings.SetInt("count", 3);
                PumpUntil(() => seen.Count > 1, 500);

                // A disconnected handler is not called again, which is the only
                // thing that distinguishes a leak from a working disconnect.
                Assert.Single(seen);
            }));
        }

        [SkippableFact]
        public void A_delayed_settings_object_holds_its_writes_until_Apply()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                settings.Delay();

                settings.SetString("greeting", "deferred");

                // Readable through the settings object immediately...
                Assert.Equal("deferred", settings.GetString("greeting"));
                Assert.True(settings.HasUnapplied);

                // ...and nowhere near the backend.
                PumpUntil(() => false, 200);
                Assert.False(File.Exists(keyfile) && File.ReadAllText(keyfile).Contains("deferred"));

                settings.Apply();

                Assert.False(settings.HasUnapplied);
                Assert.Contains("greeting='deferred'", KeyfileText(keyfile, "greeting='deferred'"));
            }));
        }

        [SkippableFact]
        public void Reverting_a_delayed_settings_object_throws_the_writes_away()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                settings.SetString("greeting", "committed");
                KeyfileText(keyfile, "greeting='committed'");

                settings.Delay();
                settings.SetString("greeting", "discarded");
                Assert.Equal("discarded", settings.GetString("greeting"));

                settings.Revert();

                Assert.False(settings.HasUnapplied);
                Assert.Equal("committed", settings.GetString("greeting"));
                Assert.DoesNotContain("discarded", File.ReadAllText(keyfile));
            }));
        }

        [SkippableFact]
        public void An_enum_key_is_an_integer_in_C_and_a_nickname_on_disk()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                // 'banana' is 1 in the <enum> above.
                Assert.Equal(1, settings.GetEnum("fruit"));

                Assert.True(settings.SetEnum("fruit", 2));

                Assert.Equal(2, settings.GetEnum("fruit"));
                Assert.Contains("fruit='cherry'", KeyfileText(keyfile, "fruit="));
            }));
        }

        [SkippableFact]
        public void A_flags_key_is_the_bitwise_or_of_the_nicknames_it_holds()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                // cheese (1) | olives (4)
                Assert.Equal(5u, settings.GetFlags("toppings"));

                Assert.True(settings.SetFlags("toppings", 3u));   // cheese | ham

                Assert.Equal(3u, settings.GetFlags("toppings"));

                var line = KeyfileText(keyfile, "toppings=");
                Assert.Contains("'cheese'", line);
                Assert.Contains("'ham'", line);
                Assert.DoesNotContain("'olives'", line);
            }));
        }

        [SkippableFact]
        public void A_child_schema_has_its_own_path_and_its_own_group_in_the_keyfile()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                Assert.Equal(new[] { "sub" }, settings.ListChildren());

                var child = settings.GetChild("sub");

                Assert.Equal(ChildSchemaId, child.SchemaId);
                Assert.Equal(SchemaPath + "sub/", child.Path);
                Assert.Equal("inner", child.GetString("nested"));

                child.SetString("nested", "outer");

                // The child's keys land under their own group, named by the path
                // segment below the backend's root.
                var text = KeyfileText(keyfile, "nested=");
                Assert.Contains("[sub]", text);
                Assert.Contains("nested='outer'", text);
            }));
        }

        [SkippableFact]
        public void A_schema_key_carries_the_summary_description_and_type_from_the_xml()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                var key = settings.SettingsSchema.GetKey("greeting");

                Assert.Equal("greeting", key.Name);
                Assert.Equal("The greeting", key.Summary);
                Assert.Equal("Words used to greet somebody.", key.Description);
                Assert.Equal("s", key.ValueType.ToString());
                Assert.Equal("hello", (string) key.DefaultValue);

                Assert.Equal(
                    new[] { "count", "enabled", "fruit", "greeting", "huge", "ratio", "toppings", "words" },
                    settings.ListKeys().OrderBy(k => k, StringComparer.Ordinal).ToArray());
            }));
        }

        [SkippableFact]
        public void Binding_a_setting_to_a_property_carries_changes_in_both_directions()
        {
            SkipWithoutSchemas();

            Run(() => WithSettings((settings, keyfile) =>
            {
                var label = new Gtk.Label("placeholder");

                settings.Bind("greeting", label.Handle, "label", GLib.SettingsBindFlags.Default);

                // Binding writes the setting into the property straight away.
                Assert.Equal("hello", label.Text);

                // Settings to property goes through ::changed, i.e. the loop.
                settings.SetString("greeting", "from settings");
                PumpUntil(() => label.Text == "from settings", 3000);
                Assert.Equal("from settings", label.Text);

                // Property to settings goes through notify, which is immediate.
                label.Text = "from the widget";
                Assert.Equal("from the widget", settings.GetString("greeting"));

                GLib.Settings.Unbind(label.Handle, "label");

                settings.SetString("greeting", "after unbind");
                PumpUntil(() => label.Text != "from the widget", 500);
                Assert.Equal("from the widget", label.Text);
            }));
        }

        // ----------------------------------------------------------- GFileInfo

        [SkippableFact(Skip = null)]
        public void QueryInfo_reports_the_size_and_kind_the_test_put_on_disk()
        {
            Run(() => WithTempDir(dir =>
            {
                var payload = Encoding.UTF8.GetBytes("twenty-four bytes here!!");
                Assert.Equal(24, payload.Length);
                File.WriteAllBytes(Path.Combine(dir, "sized.bin"), payload);

                var info = FileAt(dir, "sized.bin")
                           .QueryInfo("standard::*", GLib.FileQueryInfoFlags.None, null);

                Assert.Equal("sized.bin", info.Name);
                Assert.Equal(24L, info.Size);
                Assert.Equal(GLib.FileType.Regular, info.FileType);
                Assert.False(info.IsSymlink);

                var directory = GLib.FileFactory.NewForPath(dir)
                                .QueryInfo("standard::type", GLib.FileQueryInfoFlags.None, null);
                Assert.Equal(GLib.FileType.Directory, directory.FileType);
            }));
        }

        [Fact]
        public void An_info_only_carries_the_attributes_that_were_asked_for()
        {
            // The attribute string is not a hint: asking for standard::name and
            // then reading standard::size gets a zero that looks like an empty
            // file rather than an error.
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "narrow.txt"), "content");

                var file = FileAt(dir, "narrow.txt");

                var narrow = file.QueryInfo("standard::name", GLib.FileQueryInfoFlags.None, null);
                Assert.True(narrow.HasAttribute("standard::name"));
                Assert.False(narrow.HasAttribute("standard::size"));
                Assert.Equal(0L, narrow.Size);

                var wide = file.QueryInfo("standard::name,standard::size",
                                          GLib.FileQueryInfoFlags.None, null);
                Assert.True(wide.HasAttribute("standard::size"));
                Assert.Equal(7L, wide.Size);
            }));
        }

        [Fact]
        public void An_attribute_set_by_hand_round_trips_at_every_width()
        {
            // A GFileInfo is also a plain attribute bag, and this is the layer
            // the metadata renames GetAttributeInt/Long/UInt/ULong sit on.
            Run(() =>
            {
                var info = new GLib.FileInfo();

                info.SetAttributeString("test::text", "value");
                info.SetAttributeBoolean("test::flag", true);
                info.SetAttributeInt("test::narrow", -1234);
                info.SetAttributeUInt("test::unsigned", 4000000000u);
                info.SetAttributeLong("test::wide", -9007199254740993L);
                info.SetAttributeStringv("test::list", new[] { "a", "b" });

                Assert.Equal("value", info.GetAttributeString("test::text"));
                Assert.True(info.GetAttributeBoolean("test::flag"));
                Assert.Equal(-1234, info.GetAttributeInt("test::narrow"));
                Assert.Equal(4000000000u, info.GetAttributeUInt("test::unsigned"));
                Assert.Equal(-9007199254740993L, info.GetAttributeLong("test::wide"));
                Assert.Equal(new[] { "a", "b" }, info.GetAttributeStringv("test::list"));

                Assert.Equal(GLib.FileAttributeType.Uint32, info.GetAttributeType("test::unsigned"));
                Assert.Equal("-1234", info.GetAttributeAsString("test::narrow"));

                Assert.Equal(new[] { "test::flag", "test::list", "test::narrow",
                                     "test::text", "test::unsigned", "test::wide" },
                             info.ListAttributes("test").OrderBy(a => a, StringComparer.Ordinal).ToArray());

                info.RemoveAttribute("test::text");
                Assert.False(info.HasAttribute("test::text"));
                Assert.True(info.HasNamespace("test"));
            });
        }

        // ------------------------------------------------------- GFileEnumerator

        [Fact]
        public void A_directory_enumerates_exactly_the_children_the_test_created()
        {
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "one.txt"), "1");
                File.WriteAllText(Path.Combine(dir, "two.txt"), "22");
                Directory.CreateDirectory(Path.Combine(dir, "nested"));

                using var enumerator = GLib.FileFactory.NewForPath(dir)
                    .EnumerateChildren("standard::name,standard::type,standard::size",
                                       GLib.FileQueryInfoFlags.None, null);

                var seen = new Dictionary<string, GLib.FileType>();
                var sizes = new Dictionary<string, long>();
                foreach (var info in enumerator)
                {
                    seen[info.Name] = info.FileType;
                    sizes[info.Name] = info.Size;
                }

                Assert.Equal(new[] { "nested", "one.txt", "two.txt" },
                             seen.Keys.OrderBy(n => n, StringComparer.Ordinal).ToArray());
                Assert.Equal(GLib.FileType.Directory, seen["nested"]);
                Assert.Equal(GLib.FileType.Regular, seen["one.txt"]);
                Assert.Equal(1L, sizes["one.txt"]);
                Assert.Equal(2L, sizes["two.txt"]);
            }));
        }

        [Fact]
        public void An_enumerator_returns_null_once_and_stays_exhausted()
        {
            // NextFile returning null is the terminator the hand-written
            // IEnumerator in FileEnumerator.cs relies on; a second call that
            // rewound would make every foreach over a directory infinite.
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "only.txt"), "x");

                using var enumerator = GLib.FileFactory.NewForPath(dir)
                    .EnumerateChildren("standard::name", GLib.FileQueryInfoFlags.None, null);

                Assert.Equal("only.txt", enumerator.NextFile().Name);
                Assert.Null(enumerator.NextFile());
                Assert.Null(enumerator.NextFile());

                Assert.True(enumerator.Close(null));
                Assert.True(enumerator.IsClosed);
            }));
        }

        [Fact]
        public void The_enumerator_names_the_directory_it_was_opened_on()
        {
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "child.txt"), "x");

                using var enumerator = GLib.FileFactory.NewForPath(dir)
                    .EnumerateChildren("standard::name", GLib.FileQueryInfoFlags.None, null);

                var info = enumerator.NextFile();

                // GetChild composes the child path from the enumerator's own
                // container, which is the only way to turn a name back into a
                // GFile without knowing where the enumeration started.
                Assert.Equal(Path.Combine(dir, "child.txt"), enumerator.GetChild(info).Path);
                Assert.Equal(dir, enumerator.Container.Path);
            }));
        }

        // ------------------------------------------------------ copy/move/delete

        [Fact]
        public void Copying_a_file_leaves_the_original_and_reproduces_its_bytes()
        {
            Run(() => WithTempDir(dir =>
            {
                var payload = "copy me, byte for byte";
                File.WriteAllText(Path.Combine(dir, "src.txt"), payload, new UTF8Encoding(false));

                var source = FileAt(dir, "src.txt");
                var target = FileAt(dir, "dst.txt");

                Assert.True(source.Copy(target, GLib.FileCopyFlags.None, null, null));

                Assert.Equal(payload, File.ReadAllText(Path.Combine(dir, "src.txt")));
                Assert.Equal(payload, File.ReadAllText(Path.Combine(dir, "dst.txt")));
            }));
        }

        [Fact]
        public void Copying_onto_an_existing_file_needs_Overwrite_and_says_so()
        {
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "src.txt"), "new");
                File.WriteAllText(Path.Combine(dir, "dst.txt"), "old");

                var source = FileAt(dir, "src.txt");
                var target = FileAt(dir, "dst.txt");

                var refused = Assert.Throws<GLib.GException>(
                    () => source.Copy(target, GLib.FileCopyFlags.None, null, null));
                Assert.Equal((int) GLib.IOErrorEnum.Exists, refused.Code);
                Assert.Equal("old", File.ReadAllText(Path.Combine(dir, "dst.txt")));

                Assert.True(source.Copy(target, GLib.FileCopyFlags.Overwrite, null, null));
                Assert.Equal("new", File.ReadAllText(Path.Combine(dir, "dst.txt")));
            }));
        }

        [Fact]
        public void A_copy_reports_progress_that_ends_at_the_size_of_the_file()
        {
            // The progress callback is the one place a caller sees the copy
            // happening, and its two arguments are easy to swap. A megabyte is
            // several buffers, so the last call is not also the first.
            Run(() => WithTempDir(dir =>
            {
                var payload = new byte[1024 * 1024];
                for (var i = 0; i < payload.Length; i++)
                    payload[i] = (byte) (i % 251);
                File.WriteAllBytes(Path.Combine(dir, "big.bin"), payload);

                long lastCurrent = -1, lastTotal = -1;
                var calls = 0;
                var monotonic = true;

                GLib.FileProgressCallback progress = (current, total, data) =>
                {
                    calls++;
                    if (current < lastCurrent)
                        monotonic = false;
                    lastCurrent = current;
                    lastTotal = total;
                };

                Assert.True(FileAt(dir, "big.bin")
                            .Copy(FileAt(dir, "big.copy"), GLib.FileCopyFlags.None, null, progress));

                Assert.True(calls > 1, "a megabyte should take more than one buffer");
                Assert.True(monotonic, "progress went backwards");
                Assert.Equal(payload.Length, lastTotal);
                Assert.Equal(payload.Length, lastCurrent);
                Assert.Equal(payload, File.ReadAllBytes(Path.Combine(dir, "big.copy")));
            }));
        }

        [Fact]
        public void Moving_a_file_takes_the_bytes_with_it_and_leaves_nothing_behind()
        {
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "src.txt"), "moved");

                Assert.True(FileAt(dir, "src.txt")
                            .Move(FileAt(dir, "dst.txt"), GLib.FileCopyFlags.None, null, null));

                Assert.False(File.Exists(Path.Combine(dir, "src.txt")));
                Assert.Equal("moved", File.ReadAllText(Path.Combine(dir, "dst.txt")));
            }));
        }

        [Fact]
        public void Renaming_through_SetDisplayName_returns_the_file_at_its_new_name()
        {
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "before.txt"), "same bytes");

                var renamed = FileAt(dir, "before.txt").SetDisplayName("after.txt", null);

                Assert.Equal(Path.Combine(dir, "after.txt"), renamed.Path);
                Assert.False(File.Exists(Path.Combine(dir, "before.txt")));
                Assert.Equal("same bytes", File.ReadAllText(Path.Combine(dir, "after.txt")));
            }));
        }

        [Fact]
        public void Deleting_a_file_that_is_not_there_raises_NotFound()
        {
            Run(() => WithTempDir(dir =>
            {
                var missing = FileAt(dir, "never-existed.txt");

                Assert.False(missing.QueryExists(null));

                var error = Assert.Throws<GLib.GException>(() => missing.Delete(null));
                Assert.Equal((int) GLib.IOErrorEnum.NotFound, error.Code);

                File.WriteAllText(Path.Combine(dir, "doomed.txt"), "x");
                Assert.True(FileAt(dir, "doomed.txt").Delete(null));
                Assert.False(File.Exists(Path.Combine(dir, "doomed.txt")));
            }));
        }

        [Fact]
        public void MakeDirectoryWithParents_creates_the_whole_chain_and_MakeDirectory_does_not()
        {
            Run(() => WithTempDir(dir =>
            {
                var deep = FileAt(dir, "a", "b", "c");

                var error = Assert.Throws<GLib.GException>(() => deep.MakeDirectory(null));
                Assert.Equal((int) GLib.IOErrorEnum.NotFound, error.Code);
                Assert.False(Directory.Exists(Path.Combine(dir, "a")));

                Assert.True(deep.MakeDirectoryWithParents(null));
                Assert.True(Directory.Exists(Path.Combine(dir, "a", "b", "c")));

                // A second call fails rather than succeeding silently.
                var again = Assert.Throws<GLib.GException>(() => deep.MakeDirectoryWithParents(null));
                Assert.Equal((int) GLib.IOErrorEnum.Exists, again.Code);
            }));
        }

        // ------------------------------------------------------- the async shape

        [Fact]
        public void An_async_read_calls_back_on_the_thread_that_started_it()
        {
            // Gio runs the work on a pool thread and dispatches the callback to
            // the thread-default main context of whoever started it. If that
            // were not so, every Gio callback in a Gtk program would be touching
            // widgets from the wrong thread.
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "async.txt"), "read me asynchronously",
                                  new UTF8Encoding(false));

                var caller = Thread.CurrentThread.ManagedThreadId;
                var callbackThread = -1;
                string sourcePath = null;
                GLib.IAsyncResult result = null;

                var file = FileAt(dir, "async.txt");
                file.ReadAsync(0, null, (source, res, data) =>
                {
                    callbackThread = Thread.CurrentThread.ManagedThreadId;
                    sourcePath = GLib.FileAdapter.GetObject(source)?.Path;
                    result = res;
                });

                Assert.True(PumpUntil(() => result != null), "the read callback never arrived");

                Assert.Equal(caller, callbackThread);

                // GAsyncResult names the object the operation was started on, so
                // one callback can serve several files. It is compared with
                // g_file_equal rather than by reference: a GInterfaceAdapter is
                // a fresh wrapper each time, unlike a GLib.Object.
                Assert.Equal(Path.Combine(dir, "async.txt"), sourcePath);
                Assert.True(file.Equal(GLib.FileAdapter.GetObject(result.SourceObject)));

                using var stream = file.ReadFinish(result);
                var buffer = new byte[64];
                var read = (int) stream.Read(buffer, (ulong) buffer.Length, null);

                Assert.Equal("read me asynchronously", Encoding.UTF8.GetString(buffer, 0, read));
            }));
        }

        [Fact]
        public void LoadContentsAsync_finishes_with_the_bytes_the_test_wrote()
        {
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "load.txt"), "loaded through the loop",
                                  new UTF8Encoding(false));

                GLib.IAsyncResult result = null;
                var file = FileAt(dir, "load.txt");
                file.LoadContentsAsync(null, (source, res, data) => result = res);

                Assert.True(PumpUntil(() => result != null), "the load callback never arrived");

                Assert.True(file.LoadContentsFinish(result, out var contents, out var length, out _));
                Assert.Equal("loaded through the loop", contents);
                Assert.Equal((ulong) Encoding.UTF8.GetByteCount(contents), length);
            }));
        }

        [Fact]
        public void AppendToAsync_finishes_with_a_stream_that_adds_to_the_file()
        {
            // append_to is the mode most easily implemented as "replace", and
            // the difference only shows when the file already has something in
            // it.
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "log.txt"), "first line\n", new UTF8Encoding(false));

                GLib.IAsyncResult result = null;
                var file = FileAt(dir, "log.txt");
                file.AppendToAsync(GLib.FileCreateFlags.None, 0, null,
                                   (source, res, data) => result = res);

                Assert.True(PumpUntil(() => result != null), "the append callback never arrived");

                using (var stream = file.AppendToFinish(result))
                {
                    var payload = Encoding.UTF8.GetBytes("second line\n");
                    Assert.Equal(payload.Length, (int) stream.Write(payload, (ulong) payload.Length, null));
                    Assert.True(stream.Close(null));
                }

                Assert.Equal("first line\nsecond line\n",
                             File.ReadAllText(Path.Combine(dir, "log.txt")));
            }));
        }

        [Fact]
        public void An_async_operation_started_with_a_cancelled_cancellable_finishes_as_cancelled()
        {
            // The callback still runs -- that is the contract, and a Finish call
            // that was skipped because "it was cancelled anyway" would leak the
            // GTask every time.
            Run(() => WithTempDir(dir =>
            {
                File.WriteAllText(Path.Combine(dir, "never.txt"), "unreachable");

                var cancellable = new GLib.Cancellable();
                cancellable.Cancel();

                GLib.IAsyncResult result = null;
                var file = FileAt(dir, "never.txt");
                file.LoadContentsAsync(cancellable, (source, res, data) => result = res);

                Assert.True(PumpUntil(() => result != null), "the cancelled callback never arrived");

                var error = Assert.Throws<GLib.GException>(
                    () => file.LoadContentsFinish(result, out _, out _, out _));
                Assert.Equal((int) GLib.IOErrorEnum.Cancelled, error.Code);
            }));
        }

        [Fact]
        public void Cancelling_from_the_progress_callback_stops_a_copy_that_is_already_running()
        {
            // A cancellable is only interesting once the operation has started.
            // The progress callback runs inside g_file_copy, so cancelling there
            // is in-flight by construction rather than by luck.
            Run(() => WithTempDir(dir =>
            {
                var payload = new byte[4 * 1024 * 1024];
                File.WriteAllBytes(Path.Combine(dir, "large.bin"), payload);

                var cancellable = new GLib.Cancellable();
                var calls = 0;

                GLib.FileProgressCallback progress = (current, total, data) =>
                {
                    calls++;
                    if (current > 0)
                        cancellable.Cancel();
                };

                var error = Assert.Throws<GLib.GException>(
                    () => FileAt(dir, "large.bin").Copy(FileAt(dir, "large.copy"),
                                                        GLib.FileCopyFlags.None, cancellable, progress));

                Assert.Equal((int) GLib.IOErrorEnum.Cancelled, error.Code);
                Assert.True(cancellable.IsCancelled);
                Assert.True(calls > 0);

                // What GIO does with the half-written destination is not the
                // same everywhere -- Windows leaves it on disk -- so the only
                // thing a caller can rely on is that it is not the whole file.
                var copy = new FileInfo(Path.Combine(dir, "large.copy"));
                var copied = copy.Exists ? copy.Length : 0;
                Assert.True(copied < payload.Length,
                            "a cancelled copy produced all " + payload.Length + " bytes");
            }));
        }

        [Fact]
        public void A_cancellable_raises_cancelled_once_and_can_be_reset()
        {
            Run(() =>
            {
                var cancellable = new GLib.Cancellable();
                var raised = 0;
                cancellable.Cancelled += (o, e) => raised++;

                Assert.False(cancellable.IsCancelled);

                cancellable.Cancel();
                cancellable.Cancel();

                Assert.Equal(1, raised);
                Assert.True(cancellable.IsCancelled);

                cancellable.Reset();
                Assert.False(cancellable.IsCancelled);

                cancellable.Cancel();
                Assert.Equal(2, raised);
            });
        }

        // ---------------------------------------------------------- GFileMonitor

        [Fact]
        public void A_directory_monitor_reports_the_file_the_test_creates_and_then_deletes()
        {
            // GFileMonitor::changed and GSettings::changed used to share one
            // GLib.ChangedArgs class, whose only member reads Args[0] as a
            // string. Args[0] here is a GFile, so reading it threw
            // InvalidCastException from inside the signal marshaller and there
            // was no way at all to learn what had happened to which file.
            Run(() => WithTempDir(dir =>
            {
                var events = new List<(string Name, GLib.FileMonitorEvent Type)>();

                var monitor = GLib.FileFactory.NewForPath(dir)
                                    .MonitorDirectory(GLib.FileMonitorFlags.None, null);
                monitor.RateLimit = 0;
                monitor.FileChanged += (o, args) =>
                    events.Add((args.File?.Basename, args.EventType));

                var path = Path.Combine(dir, "watched.txt");
                File.WriteAllText(path, "hello");

                Assert.True(PumpUntil(() => events.Any(e => e.Type == GLib.FileMonitorEvent.Created)),
                            "no CREATED event arrived for a file the test created");
                Assert.Equal("watched.txt",
                             events.First(e => e.Type == GLib.FileMonitorEvent.Created).Name);

                File.Delete(path);

                Assert.True(PumpUntil(() => events.Any(e => e.Type == GLib.FileMonitorEvent.Deleted)),
                            "no DELETED event arrived for a file the test deleted");
                Assert.Equal("watched.txt",
                             events.First(e => e.Type == GLib.FileMonitorEvent.Deleted).Name);

                Assert.True(monitor.Cancel());
                Assert.True(monitor.IsCancelled);
                PumpUntil(() => false, 200);
                monitor.Dispose();
            }));
        }

        [Fact]
        public void A_cancelled_monitor_reports_nothing_further()
        {
            Run(() => WithTempDir(dir =>
            {
                var events = 0;

                var monitor = GLib.FileFactory.NewForPath(dir)
                                    .MonitorDirectory(GLib.FileMonitorFlags.None, null);
                monitor.RateLimit = 0;
                monitor.FileChanged += (o, args) => events++;

                File.WriteAllText(Path.Combine(dir, "before.txt"), "x");
                Assert.True(PumpUntil(() => events > 0), "the monitor reported nothing at all");

                monitor.Cancel();
                var afterCancel = events;

                File.WriteAllText(Path.Combine(dir, "after.txt"), "y");
                PumpUntil(() => events > afterCancel, 1000);

                Assert.Equal(afterCancel, events);
                PumpUntil(() => false, 200);
                monitor.Dispose();
            }));
        }
    }
}
