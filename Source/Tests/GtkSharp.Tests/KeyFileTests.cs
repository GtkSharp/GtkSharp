using System;
using System.IO;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.KeyFile</c> is 746 hand-written lines that nothing exercised. It is
    /// also the easiest thing in the library to hold to a real standard: every
    /// setter has a matching getter, and the whole file serialises to text, so a
    /// value that survives Set → ToData → LoadFromData → Get has genuinely made it
    /// through the marshalling in both directions.
    /// </summary>
    public class KeyFileTests : GtkTestBase
    {
        public KeyFileTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Writes the file out and reads it back into a fresh KeyFile,
        /// so an assertion cannot be satisfied by an in-memory cache.</summary>
        private static GLib.KeyFile RoundTrip(GLib.KeyFile original)
        {
            var text = original.ToData();

            var reloaded = new GLib.KeyFile();
            Assert.True(reloaded.LoadFromData(text, GLib.KeyFileFlags.KeepComments),
                        "the data a KeyFile produced should load back into one");
            return reloaded;
        }

        [Fact]
        public void A_string_survives_being_written_out_and_read_back()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("Desktop Entry", "Name", "GtkSharp");

                Assert.Equal("GtkSharp", RoundTrip(file).GetString("Desktop Entry", "Name"));
            });
        }

        [Fact]
        public void A_string_containing_a_separator_and_an_escape_survives()
        {
            // The list separator is ';' and '\' starts an escape, so a value
            // containing both is where naive marshalling gives itself away.
            Run(() =>
            {
                const string awkward = @"a;b\c;";

                var file = new GLib.KeyFile();
                file.SetString("G", "K", awkward);

                Assert.Equal(awkward, RoundTrip(file).GetString("G", "K"));
            });
        }

        [Fact]
        public void A_non_ascii_string_survives_the_utf8_round_trip()
        {
            Run(() =>
            {
                const string text = "Größe — ünïcode ✓ 日本語";

                var file = new GLib.KeyFile();
                file.SetString("G", "K", text);

                Assert.Equal(text, RoundTrip(file).GetString("G", "K"));
            });
        }

        [Fact]
        public void Booleans_integers_and_doubles_keep_their_types()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetBoolean("T", "flag", true);
                file.SetInteger("T", "count", -42);
                file.SetDouble("T", "ratio", 0.25);

                var read = RoundTrip(file);

                Assert.True(read.GetBoolean("T", "flag"));
                Assert.Equal(-42, read.GetInteger("T", "count"));
                Assert.Equal(0.25, read.GetDouble("T", "ratio"), 6);
            });
        }

        [Fact]
        public void A_64_bit_integer_keeps_precision_a_32_bit_one_would_lose()
        {
            // long.MaxValue / 3 does not fit in an int, so a getter or setter
            // that narrowed on the way through would come back wrong rather
            // than merely truncated.
            Run(() =>
            {
                const long big = long.MaxValue / 3;
                const ulong bigger = ulong.MaxValue / 3;

                var file = new GLib.KeyFile();
                file.SetInt64("T", "signed", big);
                file.SetUInt64("T", "unsigned", bigger);

                var read = RoundTrip(file);

                Assert.Equal(big, read.GetInt64("T", "signed"));
                Assert.Equal(bigger, read.GetUInt64("T", "unsigned"));
            });
        }

        [Fact]
        public void A_string_list_keeps_its_elements_and_their_order()
        {
            Run(() =>
            {
                var list = new[] { "first", "second", "third" };

                var file = new GLib.KeyFile();
                file.SetStringList("G", "items", list);

                Assert.Equal(list, RoundTrip(file).GetStringList("G", "items"));
            });
        }

        [Fact]
        public void A_single_element_list_does_not_collapse_to_a_bare_string()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetStringList("G", "items", new[] { "only" });

                var read = RoundTrip(file).GetStringList("G", "items");

                Assert.Single(read);
                Assert.Equal("only", read[0]);
            });
        }

        [Fact]
        public void Integer_and_boolean_and_double_lists_round_trip()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetIntegerList("L", "ints", new[] { 1, -2, 3 });
                file.SetBooleanList("L", "bools", new[] { true, false, true });
                file.SetDoubleList("L", "doubles", new[] { 0.5, 1.5 });

                var read = RoundTrip(file);

                Assert.Equal(new[] { 1, -2, 3 }, read.GetIntegerList("L", "ints"));
                Assert.Equal(new[] { true, false, true }, read.GetBooleanList("L", "bools"));
                Assert.Equal(new[] { 0.5, 1.5 }, read.GetDoubleList("L", "doubles"));
            });
        }

        [Fact]
        public void Groups_and_keys_are_reported_after_a_reload()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("First", "a", "1");
                file.SetString("First", "b", "2");
                file.SetString("Second", "c", "3");

                var read = RoundTrip(file);

                Assert.Equal(new[] { "First", "Second" }, read.Groups);
                Assert.Equal(new[] { "a", "b" }, read.GetKeys("First"));
                Assert.Equal("First", read.StartGroup);
            });
        }

        [Fact]
        public void HasGroup_and_HasKey_answer_for_what_is_and_is_not_there()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("Present", "key", "value");

                Assert.True(file.HasGroup("Present"));
                Assert.False(file.HasGroup("Absent"));
                Assert.True(file.HasKey("Present", "key"));
                Assert.False(file.HasKey("Present", "other"));
            });
        }

        [Fact]
        public void Removing_a_key_leaves_the_group_and_its_other_keys()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("G", "keep", "yes");
                file.SetString("G", "drop", "no");

                Assert.True(file.RemoveKey("G", "drop"));

                Assert.False(file.HasKey("G", "drop"));
                Assert.True(file.HasKey("G", "keep"));
                Assert.True(file.HasGroup("G"));
            });
        }

        [Fact]
        public void Removing_a_group_takes_its_keys_with_it()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("Doomed", "key", "value");
                file.SetString("Kept", "key", "value");

                Assert.True(file.RemoveGroup("Doomed"));

                Assert.False(file.HasGroup("Doomed"));
                Assert.Equal(new[] { "Kept" }, file.Groups);
            });
        }

        [Fact]
        public void A_comment_is_written_out_and_read_back()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("G", "key", "value");
                Assert.True(file.SetComment("G", "key", "why this key exists"));

                // Comments only survive the file if KeepComments was asked for,
                // which RoundTrip does.
                var read = RoundTrip(file);

                Assert.Contains("why this key exists", read.GetComment("G", "key"));
            });
        }

        [Fact]
        public void A_localised_string_is_kept_separately_from_the_plain_one()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("Desktop Entry", "Name", "Settings");
                file.SetLocaleString("Desktop Entry", "Name", "it", "Impostazioni");

                Assert.Equal("Settings", file.GetString("Desktop Entry", "Name"));
                Assert.Equal("Impostazioni", file.GetLocaleString("Desktop Entry", "Name", "it"));
            });
        }

        [Fact]
        public void A_translation_only_survives_a_reload_when_KeepTranslations_is_asked_for()
        {
            // Loading discards translations for locales other than the running
            // one unless the flag is given, and GetLocaleString then falls back
            // to the untranslated value rather than failing -- so a caller who
            // forgets the flag gets plausible wrong answers, not an error.
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("Desktop Entry", "Name", "Settings");
                file.SetLocaleString("Desktop Entry", "Name", "it", "Impostazioni");
                var text = file.ToData();

                var dropped = new GLib.KeyFile();
                Assert.True(dropped.LoadFromData(text, GLib.KeyFileFlags.None));
                Assert.Equal("Settings", dropped.GetLocaleString("Desktop Entry", "Name", "it"));

                var kept = new GLib.KeyFile();
                Assert.True(kept.LoadFromData(text, GLib.KeyFileFlags.KeepTranslations));
                Assert.Equal("Impostazioni", kept.GetLocaleString("Desktop Entry", "Name", "it"));
            });
        }

        [Fact]
        public void Reading_a_missing_key_raises_a_GException_naming_the_failure()
        {
            // This is the GError path: g_key_file_get_string sets a GError the
            // binding must turn into a managed exception. Before the throws-ABI
            // fix, the argument was not even passed and nothing was ever raised.
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("G", "present", "value");

                var error = Assert.Throws<GLib.GException>(() => file.GetString("G", "absent"));

                Assert.Contains("absent", error.Message);
            });
        }

        [Fact]
        public void Reading_a_string_as_an_integer_raises_rather_than_returning_zero()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("G", "key", "not a number");

                Assert.Throws<GLib.GException>(() => file.GetInteger("G", "key"));
            });
        }

        [Fact]
        public void A_key_file_saved_to_disk_reloads_with_the_same_values()
        {
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-keyfile-" + Guid.NewGuid().ToString("N") + ".ini");
                try
                {
                    var file = new GLib.KeyFile();
                    file.SetString("Desktop Entry", "Name", "Saved");
                    file.SetInteger("Desktop Entry", "Version", 2);
                    file.Save(path);

                    Assert.True(File.Exists(path));

                    var reloaded = new GLib.KeyFile();
                    Assert.True(reloaded.LoadFromFile(path, GLib.KeyFileFlags.KeepComments));

                    Assert.Equal("Saved", reloaded.GetString("Desktop Entry", "Name"));
                    Assert.Equal(2, reloaded.GetInteger("Desktop Entry", "Version"));
                }
                finally
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            });
        }

        [Fact]
        public void A_hand_written_ini_file_parses_into_the_expected_values()
        {
            // The other direction: text this test wrote, not text the binding
            // produced, so a symmetrical escaping bug cannot hide in it.
            Run(() =>
            {
                var ini = "[Desktop Entry]\n" +
                          "Type=Application\n" +
                          "Name=Text Editor\n" +
                          "Categories=Utility;TextEditor;\n" +
                          "Terminal=false\n";

                var file = new GLib.KeyFile();
                Assert.True(file.LoadFromData(Encoding.UTF8.GetBytes(ini), GLib.KeyFileFlags.None));

                Assert.Equal("Application", file.GetString("Desktop Entry", "Type"));
                Assert.Equal(new[] { "Utility", "TextEditor" },
                             file.GetStringList("Desktop Entry", "Categories"));
                Assert.False(file.GetBoolean("Desktop Entry", "Terminal"));
            });
        }

        [Fact]
        public void Changing_the_list_separator_changes_how_a_list_is_written()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetListSeparator(',');
                file.SetStringList("G", "items", new[] { "a", "b" });

                var text = Encoding.UTF8.GetString(file.ToData());

                Assert.Contains("a,b", text);

                // And a reader using the same separator gets the elements back.
                var read = new GLib.KeyFile();
                read.SetListSeparator(',');
                Assert.True(read.LoadFromData(file.ToData(), GLib.KeyFileFlags.None));
                Assert.Equal(new[] { "a", "b" }, read.GetStringList("G", "items"));
            });
        }

        [Fact]
        public void GetValue_returns_the_raw_text_where_GetString_unescapes_it()
        {
            Run(() =>
            {
                var file = new GLib.KeyFile();
                file.SetString("G", "key", "line one\nline two");

                // The stored form escapes the newline; GetString undoes that.
                Assert.Equal(@"line one\nline two", file.GetValue("G", "key"));
                Assert.Equal("line one\nline two", file.GetString("G", "key"));
            });
        }
    }
}
