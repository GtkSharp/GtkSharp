using System;
using System.IO;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GioStream</c> adapts a GLib stream to <c>System.IO.Stream</c>, and
    /// <c>Gtk.Builder</c> turns XML into live widgets. Both are hand-written, both
    /// were effectively untested, and both are the kind of adapter where a
    /// plausible-looking implementation can be wrong at the edges — a short read,
    /// a seek past the end, a widget the XML named but the builder did not return.
    /// </summary>
    public class StreamAndBuilderTests : GtkTestBase
    {
        public StreamAndBuilderTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ GioStream

        private static GLib.MemoryInputStream InputOf(string text)
            => new GLib.MemoryInputStream(new GLib.Bytes(Encoding.UTF8.GetBytes(text)));

        [Fact]
        public void A_GioStream_over_an_input_stream_reads_what_was_written_into_it()
        {
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("hello gio"));

                Assert.True(stream.CanRead);
                Assert.False(stream.CanWrite);

                using var reader = new StreamReader(stream, Encoding.UTF8);

                Assert.Equal("hello gio", reader.ReadToEnd());
            });
        }

        [Fact]
        public void Reading_in_small_chunks_reassembles_the_whole_payload()
        {
            // A Read implementation that ignored offset or count would produce
            // the right length and the wrong bytes, which only shows up when the
            // buffer is filled across several calls.
            Run(() =>
            {
                const string payload = "abcdefghijklmnopqrstuvwxyz";

                using var stream = new GLib.GioStream(InputOf(payload));

                var buffer = new byte[payload.Length];
                var filled = 0;
                int read;
                while (filled < buffer.Length &&
                       (read = stream.Read(buffer, filled,
                                           Math.Min(5, buffer.Length - filled))) > 0)
                    filled += read;

                Assert.Equal(payload.Length, filled);
                Assert.Equal(payload, Encoding.UTF8.GetString(buffer));
            });
        }

        [Fact]
        public void Reading_into_the_middle_of_a_buffer_respects_the_offset()
        {
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("XYZ"));

                var buffer = new byte[6];
                buffer[0] = (byte) '-';
                buffer[5] = (byte) '-';

                var read = stream.Read(buffer, 1, 3);

                Assert.Equal(3, read);
                Assert.Equal("-XYZ", Encoding.UTF8.GetString(buffer, 0, 4));
                Assert.Equal((byte) '-', buffer[5]);   // untouched past the count
            });
        }

        [Fact]
        public void Reading_more_than_the_buffer_holds_is_rejected()
        {
            // The guard read "offset + count - 1 > buffer.Length", which let a
            // request exactly one byte too long through -- and with offset 0
            // that count went straight to the native read, past the end of the
            // managed buffer. Write next door had the same guard written
            // correctly, which is what gave it away.
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("0123456789"));

                var buffer = new byte[8];

                Assert.Throws<ArgumentException>(() => stream.Read(buffer, 0, 9));
                Assert.Throws<ArgumentException>(() => stream.Read(buffer, 1, 8));

                // The exactly-fitting request is still allowed.
                Assert.Equal(8, stream.Read(buffer, 0, 8));
            });
        }

        [Fact]
        public void A_short_read_at_an_offset_leaves_the_rest_of_the_buffer_alone()
        {
            // The offset path copied the whole scratch buffer rather than the
            // bytes actually read, so a short read wrote its trailing zeroes
            // over data the caller already had.
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("ab"));

                var buffer = new byte[10];
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = (byte) '#';

                var read = stream.Read(buffer, 2, 8);

                Assert.Equal(2, read);
                Assert.Equal("##ab######", Encoding.UTF8.GetString(buffer));
            });
        }

        [Fact]
        public void Reading_past_the_end_returns_zero_rather_than_blocking_or_throwing()
        {
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("ab"));

                var buffer = new byte[8];

                Assert.Equal(2, stream.Read(buffer, 0, 8));
                Assert.Equal(0, stream.Read(buffer, 0, 8));
            });
        }

        [Fact]
        public void A_GioStream_over_an_output_stream_writes_what_it_is_given()
        {
            Run(() =>
            {
                var buffer = GLib.Marshaller.Malloc(64);
                var target = new GLib.MemoryOutputStream(buffer, 64, null, null);

                using (var stream = new GLib.GioStream(target))
                {
                    Assert.True(stream.CanWrite);
                    Assert.False(stream.CanRead);

                    var payload = Encoding.UTF8.GetBytes("written through GioStream");
                    stream.Write(payload, 0, payload.Length);
                    stream.Flush();
                }

                Assert.Equal((ulong) "written through GioStream".Length, target.DataSize);
            });
        }

        [Fact]
        public void Writing_from_the_middle_of_a_buffer_respects_offset_and_count()
        {
            Run(() =>
            {
                var raw = GLib.Marshaller.Malloc(64);
                var target = new GLib.MemoryOutputStream(raw, 64, null, null);

                using (var stream = new GLib.GioStream(target))
                {
                    var payload = Encoding.UTF8.GetBytes("....payload....");
                    stream.Write(payload, 4, 7);
                    stream.Flush();
                }

                Assert.Equal(7ul, target.DataSize);
            });
        }

        [Fact]
        public void A_seekable_GioStream_reports_and_changes_its_position()
        {
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("0123456789"));

                Assert.True(stream.CanSeek);
                Assert.Equal(0, stream.Position);

                stream.Seek(4, SeekOrigin.Begin);
                Assert.Equal(4, stream.Position);

                var buffer = new byte[2];
                Assert.Equal(2, stream.Read(buffer, 0, 2));
                Assert.Equal("45", Encoding.UTF8.GetString(buffer));
                Assert.Equal(6, stream.Position);

                stream.Seek(-3, SeekOrigin.Current);
                Assert.Equal(3, stream.Position);

                stream.Seek(-1, SeekOrigin.End);
                Assert.Equal(9, stream.Position);
            });
        }

        [Fact]
        public void Setting_Position_rewinds_and_the_same_bytes_come_back()
        {
            Run(() =>
            {
                using var stream = new GLib.GioStream(InputOf("repeatable"));

                var first = new byte[6];
                stream.Read(first, 0, 6);

                stream.Position = 0;

                var second = new byte[6];
                stream.Read(second, 0, 6);

                Assert.Equal(first, second);
            });
        }

        /// <summary>Runs <paramref name="body"/> with a path in the temp
        /// directory, and deletes whatever ended up there.</summary>
        private static void WithTempPath(Action<string> body)
        {
            var path = Path.Combine(Path.GetTempPath(),
                                    "gtksharp-giostream-" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                body(path);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Fact]
        public void A_GioStream_over_a_real_file_reads_the_file_back()
        {
            // Both file constructors threw NotImplementedException, so the class
            // could only wrap a stream the caller had already opened -- the one
            // case where it saves nobody any work.
            Run(() => WithTempPath(path =>
            {
                File.WriteAllText(path, "from disk", Encoding.UTF8);

                using var stream = new GLib.GioStream(path, FileMode.Open);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                Assert.Equal("from disk", reader.ReadToEnd());
            }));
        }

        [Fact]
        public void A_GioStream_opened_by_uri_reads_the_same_file()
        {
            Run(() => WithTempPath(path =>
            {
                File.WriteAllText(path, "by uri", Encoding.UTF8);

                using var stream = new GLib.GioStream(new Uri(path), FileMode.Open);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                Assert.Equal("by uri", reader.ReadToEnd());
            }));
        }

        [Fact]
        public void Opening_a_file_for_Create_writes_it_and_replaces_what_was_there()
        {
            Run(() => WithTempPath(path =>
            {
                File.WriteAllText(path, "old contents that are longer", Encoding.UTF8);

                using (var stream = new GLib.GioStream(path, FileMode.Create))
                {
                    var payload = Encoding.UTF8.GetBytes("new");
                    stream.Write(payload, 0, payload.Length);
                }

                Assert.Equal("new", File.ReadAllText(path, Encoding.UTF8));
            }));
        }

        [Fact]
        public void Opening_a_file_for_Append_adds_to_what_is_there()
        {
            Run(() => WithTempPath(path =>
            {
                File.WriteAllText(path, "first", Encoding.UTF8);

                using (var stream = new GLib.GioStream(path, FileMode.Append))
                {
                    var payload = Encoding.UTF8.GetBytes("-second");
                    stream.Write(payload, 0, payload.Length);
                }

                Assert.Equal("first-second", File.ReadAllText(path, Encoding.UTF8));
            }));
        }

        [Fact]
        public void CreateNew_refuses_a_file_that_already_exists()
        {
            Run(() => WithTempPath(path =>
            {
                File.WriteAllText(path, "in the way", Encoding.UTF8);

                Assert.ThrowsAny<GLib.GException>(
                    () => new GLib.GioStream(path, FileMode.CreateNew));

                Assert.Equal("in the way", File.ReadAllText(path, Encoding.UTF8));
            }));
        }

        [Fact]
        public void OpenOrCreate_creates_a_missing_file_rather_than_failing()
        {
            Run(() => WithTempPath(path =>
            {
                Assert.False(File.Exists(path));

                using (var stream = new GLib.GioStream(path, FileMode.OpenOrCreate))
                    Assert.Equal(0, stream.Read(new byte[4], 0, 4));

                Assert.True(File.Exists(path));
            }));
        }

        // -------------------------------------------------------- Gtk.Builder

        private const string SimpleUi = @"<?xml version='1.0' encoding='UTF-8'?>
<interface>
  <object class='GtkBox' id='root'>
    <property name='orientation'>vertical</property>
    <property name='spacing'>6</property>
    <child>
      <object class='GtkLabel' id='caption'>
        <property name='label'>from the builder</property>
      </object>
    </child>
    <child>
      <object class='GtkButton' id='action'>
        <property name='label'>Press</property>
      </object>
    </child>
  </object>
</interface>";

        [Fact]
        public void A_builder_returns_the_objects_the_xml_named()
        {
            Run(() =>
            {
                var builder = new Gtk.Builder();
                Assert.True(builder.AddFromString(SimpleUi));

                var root = builder.GetObject("root") as Gtk.Box;
                var caption = builder.GetObject("caption") as Gtk.Label;
                var action = builder.GetObject("action") as Gtk.Button;

                Assert.NotNull(root);
                Assert.NotNull(caption);
                Assert.NotNull(action);

                Assert.Equal(Gtk.Orientation.Vertical, root.Orientation);
                Assert.Equal(6, root.Spacing);
                Assert.Equal("from the builder", caption.Text);
                Assert.Equal("Press", action.Label);
            });
        }

        [Fact]
        public void A_builder_reproduces_the_parent_child_structure()
        {
            Run(() =>
            {
                var builder = new Gtk.Builder();
                Assert.True(builder.AddFromString(SimpleUi));

                var root = (Gtk.Box) builder.GetObject("root");
                var caption = (Gtk.Label) builder.GetObject("caption");

                Assert.Same(root, caption.Parent);
                Assert.Same(caption, root.FirstChild);
            });
        }

        [Fact]
        public void Asking_a_builder_for_a_name_it_never_saw_returns_null()
        {
            Run(() =>
            {
                var builder = new Gtk.Builder();
                Assert.True(builder.AddFromString(SimpleUi));

                Assert.Null(builder.GetObject("no-such-object"));
            });
        }

        [Fact]
        public void Malformed_builder_xml_raises_rather_than_returning_a_broken_tree()
        {
            Run(() =>
            {
                var builder = new Gtk.Builder();

                Assert.ThrowsAny<GLib.GException>(
                    () => builder.AddFromString("<interface><object class='NoSuchWidget' id='x'/></interface>"));
            });
        }

        [Fact]
        public void A_builder_can_be_fed_from_a_stream()
        {
            // The Stream constructor is hand-written: it reads the stream into a
            // string and hands it to AddFromString, so it is worth proving the
            // widgets come out the same way.
            Run(() =>
            {
                using var source = new MemoryStream(Encoding.UTF8.GetBytes(SimpleUi));

                var builder = new Gtk.Builder(source);

                Assert.Equal("from the builder", ((Gtk.Label) builder.GetObject("caption")).Text);
            });
        }

        [Fact]
        public void The_same_object_asked_for_twice_is_the_same_wrapper()
        {
            // GLib.Object.GetObject caches by native address; two lookups
            // returning different wrappers would mean two managed objects
            // owning one native one.
            Run(() =>
            {
                var builder = new Gtk.Builder();
                Assert.True(builder.AddFromString(SimpleUi));

                Assert.Same(builder.GetObject("action"), builder.GetObject("action"));
            });
        }
    }
}
